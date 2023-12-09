using Nebukam.ORCA;
using Unity.Entities;
using Unity.Mathematics;

namespace MusouEcs
{
    public struct SpeedData : IComponentData
    {
        public float Speed;
    }

    public struct OrcaDynamicData : IComponentData
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

    public class OrcaAgentData : IComponentData
    {
        public Agent Agent;
    }

    public struct OrcaCleanUpData : ICleanupComponentData
    {
    }
}