using System.Collections.Generic;
using Nebukam.Common;
using Nebukam.ORCA;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace MusouEcs
{
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [RequireMatchingQueriesForUpdate]
    [UpdateAfter(typeof(MonsterMoveToPlayerSystem))]
    public partial class OrcaMoveSystem : SystemBase
    {
        private NativeQueue<float3> _nativeQueue = new(Allocator.Persistent);
        private ORCABundle<Agent> _bundle;
        private Dictionary<Entity, Agent> _entity2AgentMap;
        private Agent _playerAgent;
        private float3 _playerLastPos = float3.zero;

        protected override void OnCreate()
        {
            base.OnCreate();

            RequireForUpdate<MoveDirectionData>();

            _bundle = new ORCABundle<Agent>();
            _bundle.plane = AxisPair.XY;

            _entity2AgentMap = new Dictionary<Entity, Agent>();
        }

        protected override void OnUpdate()
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            var playerEntity = SystemAPI.GetSingletonEntity<PlayerData>();
            var playerTransform = SystemAPI.GetComponentRW<LocalTransform>(playerEntity);
            if (_playerAgent == null)
            {
                _playerAgent = _bundle.agents.Add(float3.zero);
                _playerAgent.prefVelocity = float3.zero;
                _playerAgent.maxSpeed = 0;
                _playerAgent.radius = 0.4f;
                _playerAgent.radiusObst = 0.4f;
                _playerLastPos = float3.zero;
                _playerAgent.layerOccupation = ORCALayer.L0;
                _playerAgent.layerIgnore = ORCALayer.L0;
            }

            var direction = playerTransform.ValueRW.Position - _playerLastPos;
            var directionNormalized = math.normalizesafe(direction);
            _playerLastPos = playerTransform.ValueRW.Position;
            _playerAgent.prefVelocity = directionNormalized;
            _playerAgent.maxSpeed = math.length(direction) / deltaTime;

            var playOrcaDynamicData = SystemAPI.GetComponentRW<MoveDirectionData>(playerEntity);
            playOrcaDynamicData.ValueRW.Direction = directionNormalized;

            foreach (var (transform, directionData, speedData, _, entity) in
                     SystemAPI
                         .Query<RefRO<LocalTransform>, RefRO<MoveDirectionData>, RefRO<MoveSpeedData>,
                             RefRO<MonsterData>>()
                         .WithEntityAccess())
            {
                if (_entity2AgentMap.TryGetValue(entity, out var value))
                {
                    value.prefVelocity = directionData.ValueRO.Direction;
                    value.maxSpeed = speedData.ValueRO.Speed;
                }
                else
                {
                    var agent = _bundle.agents.Add(transform.ValueRO.Position);
                    _entity2AgentMap.Add(entity, agent);
                    agent.prefVelocity = directionData.ValueRO.Direction;
                    agent.maxSpeed = speedData.ValueRO.Speed;
                    var orcaSharedData = EntityManager.GetSharedComponent<OrcaSharedData>(entity);
                    agent.radius = orcaSharedData.Radius;
                    agent.radiusObst = orcaSharedData.RadiusObst;
                    agent.maxNeighbors = orcaSharedData.MaxNeighbors;
                    agent.neighborDist = orcaSharedData.NeighborDist;
                    agent.layerOccupation = ORCALayer.L0;
                }
            }

            if (_bundle.orca.TryComplete())
            {
                var camera = MusouCamera.Main;
                float3 cameraPosition = camera.transform.position;
                var orthographicSize = camera.orthographicSize + 1f; //防止图片过大带来误差
                var yBottom = cameraPosition.y - orthographicSize;
                var yTop = cameraPosition.y + orthographicSize;
                var screenHeight = Screen.height;
                var screenWidth = Screen.width;
                var horizonSize = orthographicSize / screenHeight * screenWidth;
                var xLeft = cameraPosition.x - horizonSize;
                var xRight = cameraPosition.x + horizonSize;

                var index = 0;
                var gsbIndex2MonsterEntity = SharedStaticMonsterData.SharedValue.Data.GsbIndex2MonsterEntity;
                foreach (var (transform, _, entity) in
                         SystemAPI.Query<RefRW<LocalTransform>, RefRO<MonsterData>>().WithEntityAccess())
                {
                    if (!_entity2AgentMap.TryGetValue(entity, out var value)) continue;

                    transform.ValueRW.Position = value.pos;

                    //检查是否可以渲染，顺便也只能攻击到可被渲染的怪物
                    var posY = transform.ValueRO.Position.y;
                    var posX = transform.ValueRO.Position.x;

                    var isMusouSpriteDataEnabled = SystemAPI.IsComponentEnabled<MusouSpriteData>(entity);

                    if (posY < yBottom || posY > yTop || posX > xRight || posX < xLeft)
                    {
                        if (isMusouSpriteDataEnabled)
                            SystemAPI.SetComponentEnabled<MusouSpriteData>(entity, false);

                        continue;
                    }

                    if (!isMusouSpriteDataEnabled)
                        SystemAPI.SetComponentEnabled<MusouSpriteData>(entity, true);

                    _nativeQueue.Enqueue(value.pos);
                    gsbIndex2MonsterEntity[index] = entity;
                    index++;
                }

                _bundle.orca.Schedule(deltaTime);

                var nativeArray = _nativeQueue.ToArray(Allocator.Temp);
                _nativeQueue.Clear();

                if (nativeArray.Length <= 0)
                {
                    MusouMain.Inst.Gsb.Clean();
                }
                else
                {
                    MusouMain.Inst.Gsb.InitGrid(nativeArray);
                }

                nativeArray.Dispose();
            }
            else
            {
                _bundle.orca.Schedule(deltaTime);
            }

            //遍历所有 OrcaCleanUpData 去除销毁的entity
            foreach (var (_, entity) in
                     SystemAPI.Query<RefRO<OrcaCleanUpData>>().WithEntityAccess())
            {
                if (!_entity2AgentMap.TryGetValue(entity, out var value)) continue;
                _bundle.agents.Remove(value);
                _entity2AgentMap.Remove(entity);
            }

            var query = GetEntityQuery(typeof(OrcaCleanUpData));
            EntityManager.RemoveComponent<OrcaCleanUpData>(query);
        }
    }
}