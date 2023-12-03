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
    [UpdateAfter(typeof(ORCAMoveSystem))]
    public partial struct MusouSpritePreRenderSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerData>();
            state.RequireForUpdate<MusouSpriteData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var playerPos = SharedStaticPlayerData.SharedValue.Data.PlayerPosition;
            var elapsedTime = SystemAPI.Time.DeltaTime;
            var job = new SpritePreRenderJob
                { ElapsedTime = elapsedTime, PlayerPos = new float3(playerPos.x, playerPos.y, 0) };
            job.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}