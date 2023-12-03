using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Mathematics;

namespace MusouEcs
{
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
}