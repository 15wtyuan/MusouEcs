using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace MusouEcs
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [UpdateAfter(typeof(OrcaMoveSystem))]
    public partial struct SkilBulletFollowPlayerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerData>();
            state.RequireForUpdate<BulletTranslateData>();
            state.RequireForUpdate<BulletFollowPlayerData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerData>();
            var playerTransform = SystemAPI.GetComponentRO<LocalTransform>(playerEntity);
            foreach (var (translateData, followPlayerData, entity) in
                     SystemAPI.Query<RefRW<BulletTranslateData>, RefRW<BulletFollowPlayerData>>().WithEntityAccess())
            {
                var delta = playerTransform.ValueRO.Position.xy - followPlayerData.ValueRO.LastPlayerPos;
                followPlayerData.ValueRW.LastPlayerPos = playerTransform.ValueRO.Position.xy;
                translateData.ValueRW.FollowPosDelta += delta;

                if (SystemAPI.HasComponent<RotateFlyBulletData>(entity))
                {
                    var rotateFlyBulletData = SystemAPI.GetComponentRW<RotateFlyBulletData>(entity);
                    rotateFlyBulletData.ValueRW.FollowPos = playerTransform.ValueRO.Position.xy;
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}