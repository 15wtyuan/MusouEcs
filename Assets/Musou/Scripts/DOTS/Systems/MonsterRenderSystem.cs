using MusouEcs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace MusouEcs
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [UpdateAfter(typeof(OrcaMoveSystem))]
    public partial struct MonsterRenderSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerData>();
            state.RequireForUpdate<MusouRenderFrameData>();
            state.RequireForUpdate<MusouRenderAniData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var playerPos = SharedStaticPlayerData.SharedValue.Data.PlayerPosition;
            var elapsedTime = SystemAPI.Time.DeltaTime;
            var job = new MonsterRenderJob
                { ElapsedTime = elapsedTime, PlayerPos = new float3(playerPos.x, playerPos.y, 0) };
            job.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }

    public readonly partial struct MonsterRenderAspect : IAspect
    {
        public readonly RefRW<LocalTransform> LocalTransform;
        public readonly RefRW<MusouRenderFrameData> RenderFrameData;
        public readonly RefRW<MusouRenderAniData> RenderAniData;
        public readonly MusouRenderAniSharedData RenderAniSharedData;

        public void RunAni(float elapsedTime, float3 playerPos)
        {
            //怪物的朝向一般是向着玩家即可
            var dirDelta = playerPos - LocalTransform.ValueRO.Position;

            if (dirDelta.x > 0.05 || dirDelta.x < -0.05)
            {
                var face = dirDelta.x > 0 ? 1 : -1;
                RenderAniData.ValueRW.Face = face;
            }

            var curPosition = LocalTransform.ValueRO.Position;
            LocalTransform.ValueRW.Position = new float3(curPosition.x, curPosition.y, -curPosition.y * 0.1f);

            RenderAniData.ValueRW.Timer += elapsedTime;
            if (!(RenderAniData.ValueRO.Timer >= 1f / RenderAniSharedData.FrameRate)) return;
            RenderAniData.ValueRW.Timer = 0;
            PlayNextFrame();
        }

        private void PlayNextFrame()
        {
            RenderFrameData.ValueRW.CurFrame += 1;
            //播放下一帧
            if (RenderFrameData.ValueRO.CurFrame > RenderAniSharedData.EndFarme)
            {
                RenderFrameData.ValueRW.CurFrame = RenderAniSharedData.BeginFarme;
            }
        }
    }

    public partial struct MonsterRenderJob : IJobEntity
    {
        [ReadOnly] public float ElapsedTime;
        [ReadOnly] public float3 PlayerPos;

        public void Execute(MonsterRenderAspect aspect)
        {
            aspect.RunAni(ElapsedTime, PlayerPos);
        }
    }
}