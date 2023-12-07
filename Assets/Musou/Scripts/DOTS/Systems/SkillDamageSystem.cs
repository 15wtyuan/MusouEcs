using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace MusouEcs
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [UpdateAfter(typeof(SkillBulletMoveSystem))]
    public partial class SkillDamageSystem : SystemBase
    {
        private NativeArray<float3> _queryPoints;

        protected override void OnCreate()
        {
            RequireForUpdate<SkillDamageData>();
        }

        protected override void OnUpdate()
        {
            _queryPoints = new NativeArray<float3>(1, Allocator.Temp);

            foreach (var (tag, transform, entity) in
                     SystemAPI.Query<RefRO<SkillDamageTag>, RefRO<LocalTransform>>().WithEntityAccess())
            {
                var skillDamageData = EntityManager.GetSharedComponent<SkillDamageData>(entity);
                _queryPoints[0] = transform.ValueRO.Position;
                var results = MusouMain.Inst.Gsb.searchWithin(_queryPoints, skillDamageData.DamageRadius, 100);

                //下面可改成job
                for (var i = 0; i < results.Length; i++)
                {
                    var t = results[i];
                    var monsterEntity =
                        SharedStaticMonsterData.SharedValue.Data.GsbIndex2MonsterEntity[t];
                    var monsterData = EntityManager.GetComponentData<MonsterData>(monsterEntity);
                    monsterData.Hp -= skillDamageData.Damage;
                }

                results.Dispose();
            }

            _queryPoints.Dispose();
        }
    }
}