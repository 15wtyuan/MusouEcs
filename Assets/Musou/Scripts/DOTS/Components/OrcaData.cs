using Unity.Entities;

namespace MusouEcs
{
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