using Musou.Scripts.DOTS.Systems;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace MusouEcs
{
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [CreateAfter(typeof(MonsterGenerateSystem))]
    [UpdateAfter(typeof(PlayerMoveSystem))]
    public partial struct MonsterMoveToPlayerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var playerPos = SharedStaticPlayerData.SharedValue.Data.PlayerPosition;
            var elapsedTime = SystemAPI.Time.DeltaTime;
            var job = new MonsterMoveToPlayerJob
                { ElapsedTime = elapsedTime, Target = new float3(playerPos.x, playerPos.y, 0) };
            job.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}