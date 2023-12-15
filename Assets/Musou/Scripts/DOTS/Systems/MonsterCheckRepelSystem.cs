using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace MusouEcs
{
    [BurstCompile]
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    public partial struct MonsterCheckRepelSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var ecbParallel = ecb.AsParallelWriter();

            var job = new MonsterCheckRepelJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
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
    public partial struct MonsterCheckRepelJob : IJobEntity
    {
        [ReadOnly] public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter Ecb;

        private void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref RepelMoveData repelMoveData)
        {
            repelMoveData.RepelTime -= DeltaTime;
            if (repelMoveData.RepelTime <= 0)
            {
                Ecb.SetComponentEnabled<RepelMoveData>(chunkIndex, entity, false);
            }
        }
    }
}