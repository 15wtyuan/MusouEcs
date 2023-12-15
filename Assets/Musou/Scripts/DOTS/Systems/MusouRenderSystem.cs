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
            var elapsedTime = SystemAPI.Time.DeltaTime;
            var job = new MusouRenderJob
                { ElapsedTime = elapsedTime };
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
        private readonly MusouRenderAniSharedData _renderAniSharedData;

        public void FixPos()
        {
            var dirDelta = _moveDirectionData.ValueRO.Direction;

            if (dirDelta.x > 0.05 || dirDelta.x < -0.05)
            {
                var face = dirDelta.x > 0 ? 1 : -1;
                _renderFaceData.ValueRW.Face = face;
            }
        }

        public void RunAni(float elapsedTime)
        {
            if (_renderAniData.ValueRO.Timer == 0)
            {
                PlayNextFrame();
            }

            _renderAniData.ValueRW.Timer += elapsedTime;
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
        [ReadOnly] public float ElapsedTime;

        private void Execute(MusouRenderAspect aspect)
        {
            aspect.FixPos();
            aspect.RunAni(ElapsedTime);
        }
    }
}