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
    [UpdateAfter(typeof(OrcaMoveSystem))]
    public partial struct MusouSpritePreRenderSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MusouSpriteData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var elapsedTime = SystemAPI.Time.DeltaTime;
            var job = new SpritePreRenderJob
                { ElapsedTime = elapsedTime };
            job.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }

    public readonly partial struct SpritePreRenderAspect : IAspect
    {
        private readonly RefRO<LocalTransform> localTransform;
        private readonly RefRW<MusouSpriteData> spriteData;
        private readonly RefRW<MusouSpriteAniData> spriteAniData;
        private readonly RefRO<OrcaDynamicData> orcaDynamicData;
        private readonly MusouSpriteAniSharedData spriteAniSharedData;

        public void FixPos()
        {
            var dirDelta = orcaDynamicData.ValueRO.Direction;

            if (dirDelta.x > 0.05 || dirDelta.x < -0.05)
            {
                var face = dirDelta.x > 0 ? 1 : -1;
                spriteData.ValueRW.Face = face;
            }

            spriteData.ValueRW.Matrix4X4 =
                Matrix4x4.TRS(localTransform.ValueRO.Position, Quaternion.identity,
                    new Vector3(spriteData.ValueRO.Face * spriteAniSharedData.Scale.x, spriteAniSharedData.Scale.y,
                        spriteAniSharedData.Scale.z));
        }

        public void RunAni(float elapsedTime)
        {
            if (spriteAniData.ValueRO.Timer == 0)
            {
                PlayNextFrame();
            }

            spriteAniData.ValueRW.Timer += elapsedTime;
            if (!(spriteAniData.ValueRO.Timer >= 1f / spriteAniSharedData.FrameRate)) return;

            PlayNextFrame();
            spriteAniData.ValueRW.Timer = 0;
        }

        private void PlayNextFrame()
        {
            //播放下一帧
            if (spriteAniData.ValueRO.CurFrame > spriteAniSharedData.EndFarme)
            {
                spriteAniData.ValueRW.CurFrame = spriteAniSharedData.BeginFarme;
            }

            var curRow = spriteAniData.ValueRW.CurFrame / (int)spriteData.ValueRO.AtlasRect.x + 1;
            var curCol = spriteAniData.ValueRW.CurFrame - ((curRow - 1) * (int)spriteData.ValueRO.AtlasRect.x);
            if (curCol == 0)
            {
                curRow--;
                curCol = (int)spriteData.ValueRO.AtlasRect.x;
            }

            curRow = (int)spriteData.ValueRO.AtlasRect.y - curRow + 1;
            spriteData.ValueRW.AtlasRect = new Vector4((int)spriteData.ValueRO.AtlasRect.x,
                (int)spriteData.ValueRO.AtlasRect.y, curRow, curCol);

            spriteAniData.ValueRW.CurFrame++;
        }
    }

    public partial struct SpritePreRenderJob : IJobEntity
    {
        [ReadOnly] public float ElapsedTime;

        private void Execute(SpritePreRenderAspect aspect)
        {
            aspect.FixPos();
            aspect.RunAni(ElapsedTime);
        }
    }
}