using Unity.Entities;

namespace MusouEcs
{
    public struct PlayerGeneratorData : IComponentData
    {
        public Entity PlayerProtoType;
    }
    
    public struct PlayerData : IComponentData
    {
        
    }
}