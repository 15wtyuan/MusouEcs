using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace MusouEcs
{
    public partial struct MonsterMoveToPlayerJob : IJobEntity
    {
        [ReadOnly] public float ElapsedTime;
        [ReadOnly] public float3 Target;

        private void Execute(MonsterMoveToPlayerAspect monsterMoveToPlayerAspect)
        {
            monsterMoveToPlayerAspect.Move(Target, ElapsedTime);
        }
    }
}