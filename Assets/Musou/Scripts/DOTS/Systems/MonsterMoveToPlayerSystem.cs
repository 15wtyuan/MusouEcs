using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;

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
    
    public readonly partial struct MonsterMoveToPlayerAspect : IAspect
    {
        public readonly RefRO<LocalTransform> LocalTransform;
        public readonly RefRO<MonsterData> MonsterData;
        public readonly RefRW<OrcaDynamicData> DirectionData;

        public void Move(float3 targetPos, float deltaTime)
        {
            var dir = (targetPos - LocalTransform.ValueRO.Position);
            dir = math.normalizesafe(dir);
            DirectionData.ValueRW.Direction = dir;
        }
    }
    
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