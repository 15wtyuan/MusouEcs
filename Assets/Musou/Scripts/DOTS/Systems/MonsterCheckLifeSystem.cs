using Unity.Burst;
using Unity.Entities;

namespace MusouEcs
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [UpdateAfter(typeof(SkillDamageSystem))]
    public partial struct MonsterCheckLifeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MonsterData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (monsterData, entity) in
                     SystemAPI.Query<RefRW<MonsterData>>().WithEntityAccess())
            {
                if (monsterData.ValueRO.Hp <= 0)
                {
                    state.EntityManager.DestroyEntity(entity);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}