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
                transform.ValueRW.Position += delta;
                translateData.ValueRW.FollowPosDelta = float3.zero;
                translateData.ValueRW.Delta = float3.zero;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}