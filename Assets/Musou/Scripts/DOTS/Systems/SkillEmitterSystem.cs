using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MusouEcs
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    public partial class SkillEmitterSystem : SystemBase
    {
        private readonly Vector3[] _playerPos = new Vector3[1];
        private Vector2 _saveLastPlayerMoveDir = Vector2.right;

        protected override void OnCreate()
        {
            RequireForUpdate<EmitterData>();
        }

        protected override void OnUpdate()
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            var playerPos = SharedStaticPlayerData.SharedValue.Data.PlayerPosition;
            var playerMoveDir = SharedStaticPlayerData.SharedValue.Data.PlayerMoveDir;
            if (playerMoveDir != Vector3.zero)
            {
                _saveLastPlayerMoveDir = playerMoveDir.normalized;
            }

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
                        if (Math.Abs(angleRange - 360) < float.Epsilon)
                        {
                            perChangeDegrees = angleRange / 1f / (bulletCount);
                        }
                        else
                        {
                            perChangeDegrees = angleRange / 1f / (bulletCount - 1);
                        }
                    }

                    var offsetPos = new Vector2(emitterData.ValueRO.CreateOffset, 0);

                    var bullets =
                        CollectionHelper.CreateNativeArray<Entity>(bulletCount, Allocator.Temp);
                    EntityManager.Instantiate(emitterData.ValueRO.BulletProtoType, bullets);

                    for (int i = 0; i < bulletCount; i++)
                    {
                        var dir = createDir.RotatedByDegrees(perChangeDegrees * i);
                        var angle = Vector2.SignedAngle(Vector2.right, dir);
                        var emitterPos = playerPos + offsetPos.RotatedByDegrees(angle);
                        var bullet = bullets[i];
                        var transform = SystemAPI.GetComponentRW<LocalTransform>(bullet);
                        transform.ValueRW.Position = new float3(emitterPos.x, emitterPos.y, 0);
                        transform.ValueRW.Rotation = quaternion.RotateZ(Mathf.Deg2Rad * angle);

                        if (SystemAPI.HasComponent<StraightFlyBulletData>(bullet))
                        {
                            var straightFlyBulletData =
                                SystemAPI.GetComponentRW<StraightFlyBulletData>(bullet);
                            straightFlyBulletData.ValueRW.FlyDir = new float3(dir.x, dir.y, 0);
                        }
                    }

                    bullets.Dispose();
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
                    var playerEntity = SystemAPI.GetSingletonEntity<PlayerData>();
                    var playerRenderFaceData = EntityManager.GetComponentData<MusouRenderFaceData>(playerEntity);
                    var face = playerRenderFaceData.Face;
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
                    createDir = _saveLastPlayerMoveDir;
                    break;
                }
            }

            return createDir;
        }
    }
}