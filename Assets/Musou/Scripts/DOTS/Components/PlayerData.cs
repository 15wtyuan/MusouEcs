using Unity.Entities;

namespace MusouEcs
{
    public struct PlayerGeneratorData : IComponentData
    {
        public Entity PlayerProtoType;
        public Entity BulletProtoType;
    }
    
    public struct PlayerData : IComponentData
    {
        
    }
}