using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace MusouEcs
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [UpdateAfter(typeof(OrcaMoveSystem))]
    public partial struct PlayerRenderSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var elapsedTime = SystemAPI.Time.DeltaTime;
            
            foreach (var (_, renderFrameData, renderFaceData, renderAniData, entity) in
                     SystemAPI
                         .Query<RefRO<PlayerData>, RefRW<MusouRenderFrameData>, RefRW<MusouRenderFaceData>,
                             RefRW<MusouRenderAniData>>()
                         .WithEntityAccess())
            {
                var renderAniSharedData = state.EntityManager.GetSharedComponent<MusouRenderAniSharedData>(entity);
                if (SharedStaticPlayerData.SharedValue.Data.PlayerMoveDir != Vector3.zero)
                {
                    renderFaceData.ValueRW.Face = SharedStaticPlayerData.SharedValue.Data.PlayerMoveDir.x > 0 ? 1 : -1;
                }

                renderAniData.ValueRW.Timer += elapsedTime;
                if (!(renderAniData.ValueRO.Timer >= 1f / renderAniSharedData.FrameRate)) return;
                renderAniData.ValueRW.Timer = 0;

                renderFrameData.ValueRW.CurFrame += 1;
                //播放下一帧
                if (renderFrameData.ValueRO.CurFrame > renderAniSharedData.EndFarme)
                {
                    renderFrameData.ValueRW.CurFrame = renderAniSharedData.BeginFarme;
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}