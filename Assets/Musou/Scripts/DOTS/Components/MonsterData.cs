using Unity.Entities;
using Unity.Mathematics;

namespace MusouEcs
{
    public struct MonsterGeneratorData : IComponentData
    {
        public Entity MonsterProtoType;
        public int CntX;
        public int CntY;
    }

    public struct MonsterData : IComponentData
    {
    }
}