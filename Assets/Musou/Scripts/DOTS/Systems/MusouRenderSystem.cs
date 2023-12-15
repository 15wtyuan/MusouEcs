using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace MusouEcs
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [UpdateAfter(typeof(OrcaMoveSystem))]
    public partial struct MusouRenderSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MusouRenderAniSharedData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var elapsedTime = SystemAPI.Time.ElapsedTime;
            var deltaTime = SystemAPI.Time.DeltaTime;
            var job = new MusouRenderJob
            {
                ElapsedTime = elapsedTime,
                DeltaTime = deltaTime,
            };
            job.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }

    [BurstCompile]
    public readonly partial struct MusouRenderAspect : IAspect
    {
        private readonly RefRW<MusouRenderFaceData> _renderFaceData;
        private readonly RefRW<MusouRenderAniData> _renderAniData;
        private readonly RefRW<MusouRenderFrameData> _renderFrameData;
        private readonly RefRO<MoveDirectionData> _moveDirectionData;
        private readonly RefRW<MusouRenderBlankData> _renderBlankData;
        private readonly RefRO<MusouRenderBlankTimeData> _renderBlankTimeData;
        private readonly MusouRenderAniSharedData _renderAniSharedData;

        public void FixFace()
        {
            var dirDelta = _moveDirectionData.ValueRO.Direction;
            if (!(dirDelta.x > 0.05) && !(dirDelta.x < -0.05)) return;
            var face = dirDelta.x > 0 ? 1 : -1;
            _renderFaceData.ValueRW.Face = face;
        }

        public void FixBlank(double elapsedTime)
        {
            _renderBlankData.ValueRW.BlankOpacity = _renderBlankTimeData.ValueRO.BlankEndTime > elapsedTime ? 0.5f : 0f;
        }

        public void RunAni(float deltaTime)
        {
            if (_renderAniData.ValueRO.Timer == 0)
            {
                PlayNextFrame();
            }

            _renderAniData.ValueRW.Timer += deltaTime;
            if (!(_renderAniData.ValueRO.Timer >= 1f / _renderAniSharedData.FrameRate)) return;

            PlayNextFrame();
            _renderAniData.ValueRW.Timer = 0;
        }

        private void PlayNextFrame()
        {
            //播放下一帧
            _renderFrameData.ValueRW.Frame++;
            if (_renderFrameData.ValueRO.Frame > _renderAniSharedData.EndFarme)
            {
                _renderFrameData.ValueRW.Frame = _renderAniSharedData.BeginFarme;
            }
        }
    }

    [BurstCompile]
    public partial struct MusouRenderJob : IJobEntity
    {
        [ReadOnly] public double ElapsedTime;
        [ReadOnly] public float DeltaTime;

        private void Execute(MusouRenderAspect aspect)
        {
            aspect.FixFace();
            aspect.FixBlank(ElapsedTime);
            aspect.RunAni(DeltaTime);
        }
    }
}