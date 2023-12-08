using Unity.Burst;
using Unity.Collections;
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
    
    public readonly partial struct SpritePreRenderAspect : IAspect
    {
        public readonly RefRO<LocalTransform> LocalTransform;
        public readonly RefRW<MusouSpriteData> SpriteData;
        public readonly RefRW<MusouSpriteAniData> SpriteAniData;
        public readonly MusouSpriteAniSharedData SpriteAniSharedData;

        public void FixPos(float3 playerPos)
        {
            //怪物的朝向一般是向着玩家即可
            var dirDelta = playerPos - LocalTransform.ValueRO.Position;

            if (dirDelta.x > 0.05 || dirDelta.x < -0.05)
            {
                var face = dirDelta.x > 0 ? 1 : -1;
                SpriteData.ValueRW.Face = face;
            }

            SpriteData.ValueRW.Matrix4X4 =
                Matrix4x4.TRS(LocalTransform.ValueRO.Position, Quaternion.identity,
                    new Vector3(SpriteData.ValueRW.Face * SpriteAniSharedData.Scale.x, SpriteAniSharedData.Scale.y,
                        SpriteAniSharedData.Scale.z));
        }

        public void RunAni(float elapsedTime)
        {
            if (SpriteAniData.ValueRO.Timer == 0)
            {
                PlayNextFrame();
            }

            SpriteAniData.ValueRW.Timer += elapsedTime;
            if (!(SpriteAniData.ValueRO.Timer >= 1f / SpriteAniSharedData.FrameRate)) return;

            PlayNextFrame();
            SpriteAniData.ValueRW.Timer = 0;
        }

        private void PlayNextFrame()
        {
            //播放下一帧
            if (SpriteAniData.ValueRO.CurFrame > SpriteAniSharedData.EndFarme)
            {
                SpriteAniData.ValueRW.CurFrame = SpriteAniSharedData.BeginFarme;
            }

            var curRow = SpriteAniData.ValueRW.CurFrame / (int)SpriteData.ValueRO.AtlasData.x + 1;
            var curCol = SpriteAniData.ValueRW.CurFrame - ((curRow - 1) * (int)SpriteData.ValueRO.AtlasData.x);
            if (curCol == 0)
            {
                curRow--;
                curCol = (int)SpriteData.ValueRO.AtlasData.x;
            }

            curRow = (int)SpriteData.ValueRO.AtlasData.y - curRow + 1;
            SpriteData.ValueRW.AtlasData = new Vector4((int)SpriteData.ValueRO.AtlasData.x,
                (int)SpriteData.ValueRO.AtlasData.y, curRow, curCol);

            SpriteAniData.ValueRW.CurFrame++;
        }
    }
    
    public partial struct SpritePreRenderJob : IJobEntity
    {
        [ReadOnly] public float ElapsedTime;
        [ReadOnly] public float3 PlayerPos;

        private void Execute(SpritePreRenderAspect aspect)
        {
            aspect.FixPos(PlayerPos);
            aspect.RunAni(ElapsedTime);
        }
    }
}