using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace MusouEcs
{
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