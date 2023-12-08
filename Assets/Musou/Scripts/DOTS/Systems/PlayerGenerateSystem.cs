using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace MusouEcs
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    public partial struct PlayerGenerateSystem : ISystem, ISystemStartStop
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerGeneratorData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            var generator = SystemAPI.GetSingleton<PlayerGeneratorData>();
            var playerEntity = state.EntityManager.Instantiate(generator.PlayerProtoType);
            var transform = SystemAPI.GetComponentRW<LocalTransform>(playerEntity);
            transform.ValueRW.Position = float3.zero;

            //todo 临时生成一下
            var bulletEntity = state.EntityManager.Instantiate(generator.BulletProtoType);
            var bulletTransform = SystemAPI.GetComponentRW<LocalTransform>(bulletEntity);
            bulletTransform.ValueRW.Position = float3.zero;

            // 此System只在启动时运行一次，所以在第一次更新后关闭它。
            state.Enabled = false;
        }

        [BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
        }
    }
}