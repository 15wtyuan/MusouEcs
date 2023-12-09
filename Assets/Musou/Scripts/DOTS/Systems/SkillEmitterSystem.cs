using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace MusouEcs
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    public partial class SkillEmitterSystem : SystemBase
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

                    // 子弹发射角度
                    var perChangeDegrees = 0f;
                    var bulletCount = emitterData.ValueRO.BulletCount;
                    if (bulletCount > 1)
                    {
                        var angleRange = emitterData.ValueRO.AngleRange;
                        createDir = createDir.RotatedByDegrees(-angleRange / 2f);
                        if (angleRange == 360)
                        {
                            perChangeDegrees = angleRange / 1f / (bulletCount);
                        }
                        else
                        {
                            perChangeDegrees = angleRange / 1f / (bulletCount - 1);
                        }
                    }

                    for (int i = 0; i < bulletCount; i++)
                    {
                        // var dir = createDir.RotatedByDegrees(perChangeDegrees * i);
                        // var emitterPos = createPos + offsetPos.RotatedByDegrees(Vector2.SignedAngle(Vector2.right, dir));
                    }
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
                    createDir = Random.insideUnitCircle.normalized;
                    break;
                }
                case EmitterDirType.CloserTarget:
                {
                    var playerPos = SharedStaticPlayerData.SharedValue.Data.PlayerPosition;
                    _playerPos[0] = playerPos;
                    var monsterIndex = MusouMain.Inst.Gsb.SearchClosestPoint(_playerPos);
                    if (monsterIndex[0] >= 0)
                    {
                        var monsterEntity =
                            SharedStaticMonsterData.SharedValue.Data.GsbIndex2MonsterEntity[monsterIndex[0]];
                        var monsterTransform = SystemAPI.GetComponentRW<LocalTransform>(monsterEntity);
                        createDir = new Vector2(monsterTransform.ValueRW.Position.x,
                            monsterTransform.ValueRW.Position.y) - playerPos;
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