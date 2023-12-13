using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace MusouEcs
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [UpdateAfter(typeof(SkilBulletFollowPlayerSystem))]
    [UpdateBefore(typeof(SkillBulletMoveSystem))]
    public partial struct SkillRotateFlyBulletSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RotateFlyBulletData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (rotateFlyBulletData, translateData, transform) in
                     SystemAPI.Query<RefRO<RotateFlyBulletData>, RefRW<BulletTranslateData>, RefRW<LocalTransform>>())
            {
                var curPos = transform.ValueRO.Position;
                var curFollowPos = rotateFlyBulletData.ValueRO.FollowPos;
                var pos =
                    new Vector2(curPos.x, curPos.y).RotatedByDegrees(
                        rotateFlyBulletData.ValueRO.RotateSpeed * deltaTime,
                        new Vector2(curFollowPos.x, curFollowPos.y));
                translateData.ValueRW.Delta += new float3(pos.x, pos.y, 0) - curPos;

                var translateDelta = new Vector2(translateData.ValueRO.Delta.x, translateData.ValueRO.Delta.y);
                var curRotate = Vector2.SignedAngle(Vector2.right, translateDelta);
                transform.ValueRW.Rotation = quaternion.RotateZ(Mathf.Deg2Rad * curRotate);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}