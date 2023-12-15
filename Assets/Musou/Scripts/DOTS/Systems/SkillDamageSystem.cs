using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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

                foreach (var (skillDamageData, transform, bulletEntity) in
                         SystemAPI.Query<RefRO<SkillDamageData>, RefRO<LocalToWorld>>().WithEntityAccess())
                {
                    var skillDamageShareData = EntityManager.GetSharedComponent<SkillDamageSharedData>(bulletEntity);
                    var queryPoint = transform.ValueRO.Position;
                    _queryPoints[0] = new float3(queryPoint.x, queryPoint.y, 0);
                    var results = MusouMain.Inst.Gsb.SearchWithin(_queryPoints, skillDamageShareData.DamageRadius, 100);

                    var bulletDamageData =
                        SystemAPI.GetComponentRO<BulletDamageData>(skillDamageData.ValueRO.SkillEntity);
                    var dmgDict =
                        EntityManager.GetComponentObject<BulletDamageDictData>(skillDamageData.ValueRO.SkillEntity)
                            .DamageDict;

                    foreach (var monsterEntity in from t in results
                             where t >= 0
                             select SharedStaticMonsterData.SharedValue.Data.GsbIndex2MonsterEntity[t])
                    {
                        // 防止重复受击
                        if (dmgDict.TryGetValue(monsterEntity, out var value))
                        {
                            if (curTime - value < bulletDamageData.ValueRO.DamageInterval)
                                continue;
                        }

                        // 伤害
                        var monsterData = SystemAPI.GetComponentRW<MonsterData>(monsterEntity);
                        monsterData.ValueRW.Hp -= skillDamageShareData.Damage;
                        dmgDict[monsterEntity] = (float)curTime;

                        // 受击闪白
                        if (SystemAPI.HasComponent<MusouRenderBlankTimeData>(monsterEntity))
                        {
                            var blankData = SystemAPI.GetComponentRW<MusouRenderBlankTimeData>(monsterEntity);
                            blankData.ValueRW.BlankEndTime = curTime + MusouSetting.BLANK_TIME;
                        }

                        // 技能击退
                        if (skillDamageShareData.RepelSpeed > 0 && SystemAPI.HasComponent<RepelMoveData>(monsterEntity))
                        {
                            var bulletTranslateData =
                                SystemAPI.GetComponentRO<BulletTranslateData>(skillDamageData.ValueRO.SkillEntity);
                            SystemAPI.SetComponentEnabled<RepelMoveData>(monsterEntity, true);
                            var repelMoveData = SystemAPI.GetComponentRW<RepelMoveData>(monsterEntity);
                            repelMoveData.ValueRW.RepelDirection =
                                math.normalize(bulletTranslateData.ValueRO.LastDelta).xy;
                            repelMoveData.ValueRW.RepelTime = skillDamageShareData.RepelTime;
                            repelMoveData.ValueRW.RepelSpeed = skillDamageShareData.RepelSpeed;
                        }
                    }

                    results.Dispose();
                }

                _queryPoints.Dispose();
            }
        }
    }
}