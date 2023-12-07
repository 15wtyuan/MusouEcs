using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace MusouEcs
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    public partial class EmitterSkillSystem : SystemBase
    {
        private Vector3[] _playerPos = new Vector3[1];

        protected override void OnCreate()
        {
            RequireForUpdate<EmitterData>();
        }

        protected override void OnUpdate()
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (emitterData, emitterTimerData) in
                     SystemAPI.Query<RefRO<EmitterData>, RefRW<EmitterTimerData>>())
            {
                emitterTimerData.ValueRW.Timer += deltaTime;
                if (emitterTimerData.ValueRO.Timer >= emitterData.ValueRO.EmitterInterval)
                {
                    emitterTimerData.ValueRW.Timer -= emitterData.ValueRO.EmitterInterval;

                    // 发射子弹
                    // 确定发射方向
                    var createDir = GetCreateDir((EmitterDirType)emitterData.ValueRO.EmitterDirType);

                    //子弹发射角度
                    // var dir = createDir.RotatedByDegrees(perChangeDegrees * i);
                    // if (skillLevelConfig.diffDirection != 1)
                    // {
                    //     if (skillLevelConfig.direction == (int)AttackDirectionType.CloserTarget)
                    //     {
                    //         dir = (target.GetFixPosition() - createPos).normalized;
                    //     }
                    //     else if (skillLevelConfig.direction == (int)AttackDirectionType.Random)
                    //     {
                    //         //为了让技能不重叠起来，这里面应该是各个方向都有概率
                    //         var changeDegrees = 360f / bulletCount;
                    //         var randomDegree = Random.Range(0, changeDegrees);
                    //         dir = createDir.RotatedByDegrees(changeDegrees * i + randomDegree);
                    //     }
                    // }
                    //
                    // if (skillLevelConfig.flyBeginAngle > 0f)
                    // {
                    //     dir = dir.RotatedByDegrees(skillLevelConfig.flyBeginAngle);
                    // }
                    //
                    // var emitterPos = createPos + offsetPos.RotatedByDegrees(Vector2.SignedAngle(Vector2.right, dir));
                }
            }
        }

        private Vector2 GetCreateDir(EmitterDirType emitterDirType)
        {
            var createDir = Vector2.right;
            switch (emitterDirType)
            {
                case EmitterDirType.None:
                {
                    break;
                }
                case EmitterDirType.PlayerFace:
                {
                    //根据玩家方向
                    var face = SharedStaticPlayerData.SharedValue.Data.PlayerFace;
                    createDir = face > 0 ? Vector2.right : Vector2.left;
                    break;
                }
                case EmitterDirType.Random:
                {
                    createDir = UnityEngine.Random.insideUnitCircle.normalized;
                    break;
                }
                case EmitterDirType.CloserTarget:
                {
                    var playerPos = SharedStaticPlayerData.SharedValue.Data.PlayerPosition;
                    _playerPos[0] = playerPos;
                    var monsterIndex = MusouMain.Inst.Gsb.searchClosestPoint(_playerPos);
                    if (monsterIndex[0] >= 0)
                    {
                        var monsterEntity =
                            SharedStaticMonsterData.SharedValue.Data.GsbIndex2MonsterEntity[monsterIndex[0]];
                        var monsterTransform = EntityManager.GetComponentData<LocalTransform>(monsterEntity);
                        createDir = new Vector2(monsterTransform.Position.x, monsterTransform.Position.y) -
                                    playerPos;
                    }

                    break;
                }
                case EmitterDirType.PlayerMoveDir:
                {
                    //根据玩家方向
                    var lastMoveDir = SharedStaticPlayerData.SharedValue.Data.PlayerMoveDir;
                    if (lastMoveDir != Vector3.zero)
                    {
                        createDir = lastMoveDir.normalized;
                    }

                    break;
                }
            }

            return createDir;
        }
    }
}