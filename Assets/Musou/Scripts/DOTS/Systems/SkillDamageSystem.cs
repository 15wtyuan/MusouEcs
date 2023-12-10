using System.Linq;
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
            RequireForUpdate<SkillDamageSharedData>();
        }

        protected override void OnUpdate()
        {
            if (MusouMain.Inst.Gsb.IsInit)
            {
                var curTime = SystemAPI.Time.ElapsedTime;

                _queryPoints = new NativeArray<float3>(1, Allocator.TempJob);

                foreach (var (skillDamageData, transform, entity) in
                         SystemAPI.Query<RefRO<SkillDamageData>, RefRO<LocalToWorld>>().WithEntityAccess())
                {
                    var skillDamageShareData = EntityManager.GetSharedComponent<SkillDamageSharedData>(entity);
                    _queryPoints[0] = transform.ValueRO.Position;
                    var results = MusouMain.Inst.Gsb.SearchWithin(_queryPoints, skillDamageShareData.DamageRadius, 100);

                    var bulletDamageData =
                        SystemAPI.GetComponentRO<BulletDamageData>(skillDamageData.ValueRO.SkillEntity);
                    var dmgDict =
                        EntityManager.GetComponentObject<BulletDamageDictData>(skillDamageData.ValueRO.SkillEntity)
                            .DamageDict;

                    foreach (var monsterEntity in from t in results
                             where t > 0
                             select SharedStaticMonsterData.SharedValue.Data.GsbIndex2MonsterEntity[t])
                    {
                        if (dmgDict.TryGetValue(monsterEntity, out var value))
                        {
                            if (curTime - value < bulletDamageData.ValueRO.DamageInterval)
                                continue;
                        }

                        var monsterData = SystemAPI.GetComponentRW<MonsterData>(monsterEntity);
                        monsterData.ValueRW.Hp -= skillDamageShareData.Damage;
                        if (SystemAPI.HasComponent<MusouSpriteData>(monsterEntity))
                        {
                            var spriteData = SystemAPI.GetComponentRW<MusouSpriteData>(monsterEntity);
                            spriteData.ValueRW.BlankEndTime = (float)curTime + MusouSetting.BLANK_TIME;
                        }

                        dmgDict[monsterEntity] = (float)curTime;
                    }

                    results.Dispose();
                }

                _queryPoints.Dispose();
            }
        }
    }
}