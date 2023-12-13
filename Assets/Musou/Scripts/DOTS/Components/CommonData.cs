using Unity.Entities;
using Unity.Mathematics;

namespace MusouEcs
{
    public struct MoveSpeedData : IComponentData
    {
        public float Speed;
    }

    public struct MoveDirectionData : IComponentData
    {
        public float3 Direction;
    }

    public struct RepelMoveData : IComponentData, IEnableableComponent
    {
        public float RepelSpeed;
        public float3 RepelDirection;
        public float RepelTime;
    }
}