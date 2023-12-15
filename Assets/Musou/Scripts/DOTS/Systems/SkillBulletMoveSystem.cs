using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace MusouEcs
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [UpdateAfter(typeof(SkilBulletFollowPlayerSystem))]
    public partial struct SkillBulletMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BulletTranslateData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (translateData, transform) in
                     SystemAPI.Query<RefRW<BulletTranslateData>, RefRW<LocalTransform>>())
            {
                var delta = translateData.ValueRO.Delta + translateData.ValueRO.FollowPosDelta;
                transform.ValueRW.Position.xy += delta;
                transform.ValueRW.Position.z = transform.ValueRW.Position.y * 0.01f - 1;
                translateData.ValueRW.FollowPosDelta = float2.zero;
                translateData.ValueRW.Delta = float2.zero;
                translateData.ValueRW.LastDelta = delta;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}