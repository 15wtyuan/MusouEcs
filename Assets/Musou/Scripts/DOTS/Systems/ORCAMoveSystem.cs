﻿using System.Collections.Generic;
using Nebukam.Common;
using Nebukam.ORCA;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace MusouEcs
{
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [RequireMatchingQueriesForUpdate]
    [UpdateAfter(typeof(MonsterMoveToPlayerSystem))]
    public partial class ORCAMoveSystem : SystemBase
    {
        private ORCABundle<Agent> _bundle;
        private Dictionary<Entity, Agent> _entity2AgentMap;
        private Agent _playerAgent;
        private float3 _playerLastPos = float3.zero;

        protected override void OnCreate()
        {
            base.OnCreate();
            
            RequireForUpdate<OrcaDynamicData>();

            _bundle = new ORCABundle<Agent>();
            _bundle.plane = AxisPair.XY;

            _entity2AgentMap = new Dictionary<Entity, Agent>();
        }

        protected override void OnUpdate()
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerData>();
            var playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
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

            var de = playerTransform.Position - _playerLastPos;
            _playerLastPos = playerTransform.Position;
            _playerAgent.prefVelocity = math.normalizesafe(de);
            _playerAgent.maxSpeed = math.length(de) / deltaTime;

            foreach (var (transform, directionData, speedData, entity) in
                     SystemAPI.Query<RefRO<LocalTransform>, RefRO<OrcaDynamicData>, RefRO<SpeedData>>()
                         .WithEntityAccess())
            {
                if (_entity2AgentMap.TryGetValue(entity, out var value))
                {
                    value.prefVelocity = directionData.ValueRO.Direction;
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
                foreach (var (transform, orcaDynamicData, entity) in
                         SystemAPI.Query<RefRW<LocalTransform>, RefRO<OrcaDynamicData>>().WithEntityAccess())
                {
                    if (_entity2AgentMap.TryGetValue(entity, out var value))
                    {
                        transform.ValueRW.Position = value.pos;
                    }
                }

                _bundle.orca.Schedule(deltaTime);
            }
            else
            {
                _bundle.orca.Schedule(deltaTime);
            }

            //遍历所有 OrcaCleanUpData 去除销毁的entity
            foreach (var (cleanUpData, entity) in
                     SystemAPI.Query<RefRO<OrcaCleanUpData>>().WithNone<OrcaDynamicData>().WithEntityAccess())
            {
                if (!_entity2AgentMap.TryGetValue(entity, out var value)) continue;
                _bundle.agents.Remove(value);
                _entity2AgentMap.Remove(entity);
            }
        }
    }
}