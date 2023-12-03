using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace MusouEcs
{
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