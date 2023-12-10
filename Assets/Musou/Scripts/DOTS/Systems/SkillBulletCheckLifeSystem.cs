using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace MusouEcs
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [UpdateAfter(typeof(SkillDamageSystem))]
    public partial struct SkillBulletCheckLifeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BulletLifeData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter ecbParallel = ecb.AsParallelWriter();

            var job = new SkillBulletCheckLifeJob
            {
                Ecb = ecbParallel,
                DeltaTime = SystemAPI.Time.DeltaTime,
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile] public void OnDestroy(ref SystemState state)
        {
        }
    }

    [BurstCompile]
    public partial struct SkillBulletCheckLifeJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public float DeltaTime;

        private void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref BulletLifeData bulletLifeData)
        {
            if (bulletLifeData.Timer >= bulletLifeData.LifeTime)
            {
                Ecb.DestroyEntity(chunkIndex, entity);
            }
            else
            {
                bulletLifeData.Timer += DeltaTime;
            }
        }
    }
}