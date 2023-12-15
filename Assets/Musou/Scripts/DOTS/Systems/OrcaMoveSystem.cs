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

            var direction = playerTransform.ValueRW.Position.xy - _playerLastPos.xy;
            var directionNormalized = math.normalizesafe(direction);
            _playerLastPos.xy = playerTransform.ValueRW.Position.xy;
            _playerAgent.prefVelocity = new float3(directionNormalized.x, directionNormalized.y, 0);
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
                    value.prefVelocity = new float3(directionData.ValueRO.Direction.x,
                        directionData.ValueRO.Direction.y, 0);
                    value.maxSpeed = speedData.ValueRO.Speed;
                }
                else
                {
                    var agent = _bundle.agents.Add(new float3(transform.ValueRO.Position.x,
                        transform.ValueRO.Position.y, 0));
                    _entity2AgentMap.Add(entity, agent);
                    agent.prefVelocity = new float3(directionData.ValueRO.Direction.x,
                        directionData.ValueRO.Direction.y, 0);
                    agent.maxSpeed = speedData.ValueRO.Speed;
                    var orcaSharedData = EntityManager.GetSharedComponent<OrcaSharedData>(entity);
                    agent.radius = orcaSharedData.Radius;
                    agent.radiusObst = orcaSharedData.RadiusObst;
                    agent.maxNeighbors = orcaSharedData.MaxNeighbors;
                    agent.neighborDist = orcaSharedData.NeighborDist;
                    agent.layerOccupation = ORCALayer.L0;
                }
            }

            foreach (var (repelMoveData, entity) in SystemAPI.Query<RefRO<RepelMoveData>>().WithEntityAccess())
            {
                if (!_entity2AgentMap.TryGetValue(entity, out var value)) continue;
                value.prefVelocity = new float3(repelMoveData.ValueRO.RepelDirection.x,
                    repelMoveData.ValueRO.RepelDirection.y, 0);
                value.maxSpeed = repelMoveData.ValueRO.RepelSpeed;
            }

            if (_bundle.orca.TryComplete())
            {
                var index = 0;
                var gsbIndex2MonsterEntity = SharedStaticMonsterData.SharedValue.Data.GsbIndex2MonsterEntity;
                foreach (var (transform, _, entity) in
                         SystemAPI.Query<RefRW<LocalTransform>, RefRO<MonsterData>>().WithEntityAccess())
                {
                    if (!_entity2AgentMap.TryGetValue(entity, out var value)) continue;

                    transform.ValueRW.Position = new float3(value.pos.x, value.pos.y, value.pos.y * 0.01f);
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