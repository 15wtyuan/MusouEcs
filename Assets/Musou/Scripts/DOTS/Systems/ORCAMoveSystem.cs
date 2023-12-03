using System.Collections.Generic;
using Nebukam.Common;
using Nebukam.ORCA;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace MusouEcs
{
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [UpdateAfter(typeof(MonsterMoveToPlayerSystem))]
    public partial class ORCAMoveSystem : SystemBase
    {
        private ORCABundle<Agent> _bundle;
        private Dictionary<Entity, Agent> _entity2AgentMap;

        protected override void OnCreate()
        {
            base.OnCreate();

            _bundle = new ORCABundle<Agent>();
            _bundle.plane = AxisPair.XY;

            _entity2AgentMap = new Dictionary<Entity, Agent>();
        }

        protected override void OnUpdate()
        {
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
                }
            }

            var deltaTime = SystemAPI.Time.DeltaTime;
            if (_bundle.orca.TryComplete())
            {
                foreach (var (transform, directionData, entity) in
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