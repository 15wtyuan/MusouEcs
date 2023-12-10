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
    public partial struct MonsterGenerateSystem : ISystem, ISystemStartStop
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MonsterGeneratorData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            var count = 0;
            //遍历所有 MonsterGeneratorData
            foreach (var generator in
                     SystemAPI.Query<RefRO<MonsterGeneratorData>>())
            {
                var generateCnt = generator.ValueRO.CntX * generator.ValueRO.CntY;
                SharedStaticMonsterData.SharedValue.Data = new SharedStaticMonsterData(generateCnt);
                var monsters =
                    CollectionHelper.CreateNativeArray<Entity>(generateCnt, Allocator.Temp);
                state.EntityManager.Instantiate(generator.ValueRO.MonsterProtoType, monsters);

                foreach (var monster in monsters)
                {
                    var x = count % generator.ValueRO.CntX;
                    var y = count / generator.ValueRO.CntX;
                    var position = new float3(x * 0.4f, y * 0.4f, 0);
                    var transform = SystemAPI.GetComponentRW<LocalTransform>(monster);
                    transform.ValueRW.Position = position;
                    count++;
                }

                monsters.Dispose();
            }

            // 此System只在启动时运行一次，所以在第一次更新后关闭它。
            state.Enabled = false;
        }

        [BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
        }
    }
}