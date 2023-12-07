using MusouEcs;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Musou.Scripts.DOTS.Systems
{
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [CreateAfter(typeof(PlayerGenerateSystem))]
    public partial struct PlayerMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (transform, playerData, speedData) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerData>, RefRO<SpeedData>>())
            {
                var force = SharedStaticPlayerData.SharedValue.Data.PlayerMoveDir.normalized;
                var delta = new float3(force) * (speedData.ValueRO.Speed * deltaTime);

                transform.ValueRW.Position += delta;
                if (delta.x != 0)
                {
                    SharedStaticPlayerData.SharedValue.Data.PlayerFace = delta.x > 0 ? 1 : -1;
                }

                SharedStaticPlayerData.SharedValue.Data.PlayerPosition = (Vector3)transform.ValueRW.Position;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}