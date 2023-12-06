using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace MusouEcs
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    public partial struct EmitterSkillSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EmitterData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
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
                    Vector2 createDir;
                    switch ((EmitterDirType)emitterData.ValueRO.EmitterDirType)
                    {
                        case EmitterDirType.None:
                        {
                            break;
                        }
                        case EmitterDirType.PlayerFace:
                        {
                            // var actor = emitterEntity as ActorEntity;
                            // //根据玩家方向
                            // var face = actor.GetFace();
                            // if (face > 0)
                            // {
                            //     createDir = Vector2.right;
                            // }
                            // else
                            // {
                            //     createDir = Vector2.left;
                            // }
                            break;
                        }
                        case EmitterDirType.Random:
                        {
                            createDir = UnityEngine.Random.insideUnitCircle.normalized;
                            break;
                        }
                    }
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}