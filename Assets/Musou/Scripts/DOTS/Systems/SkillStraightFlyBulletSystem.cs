using Unity.Burst;
using Unity.Entities;

namespace MusouEcs
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [UpdateAfter(typeof(SkilBulletFollowPlayerSystem))]
    [UpdateBefore(typeof(SkillBulletMoveSystem))]
    public partial struct SkillStraightFlyBulletSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<StraightFlyBulletData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (straightFlyBulletData, translateData) in
                     SystemAPI.Query<RefRO<StraightFlyBulletData>, RefRW<BulletTranslateData>>())
            {
                translateData.ValueRW.Delta += straightFlyBulletData.ValueRO.FlySpeed * deltaTime *
                                               straightFlyBulletData.ValueRO.FlyDir;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}