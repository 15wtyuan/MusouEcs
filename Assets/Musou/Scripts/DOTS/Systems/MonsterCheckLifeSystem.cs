using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace MusouEcs
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [UpdateAfter(typeof(SkillDamageSystem))]
    public partial struct MonsterCheckLifeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MonsterData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter ecbParallel = ecb.AsParallelWriter();

            var job = new MonsterCheckLifeJob
            {
                Ecb = ecbParallel
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }

    [BurstCompile]
    public partial struct MonsterCheckLifeJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        private void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref MonsterData monsterData)
        {
            if (monsterData.Hp <= 0)
            {
                Ecb.AddComponent<OrcaCleanUpData>(chunkIndex, entity);
                Ecb.DestroyEntity(chunkIndex, entity);
            }
        }
    }
}