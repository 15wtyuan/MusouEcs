using Nebukam.ORCA;
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

    public struct OrcaSharedData : ISharedComponentData
    {
        public float Radius;
        public float RadiusObst;
        public int MaxNeighbors;
        public float NeighborDist;
    }

    public struct OrcaCleanUpData : ICleanupComponentData
    {
    }
}