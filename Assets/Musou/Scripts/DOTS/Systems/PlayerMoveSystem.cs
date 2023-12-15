using MusouEcs;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace MusouEcs
{
    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [UpdateAfter(typeof(PlayerGenerateSystem))]
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
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerData>, RefRO<MoveSpeedData>>())
            {
                var force = SharedStaticPlayerData.SharedValue.Data.PlayerMoveDir.normalized;
                var delta = new float3(force) * (speedData.ValueRO.Speed * deltaTime);

                var newPos = transform.ValueRW.Position + delta;
                transform.ValueRW.Position = new float3(newPos.x, newPos.y, newPos.y * 0.01f);
                SharedStaticPlayerData.SharedValue.Data.PlayerPosition = (Vector3)transform.ValueRW.Position;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}