using Unity.Entities;
using UnityEngine;

namespace MusouEcs
{
    public class PlayerGeneratorAuthoring : MonoBehaviour
    {
        public GameObject playerPrefab;

        private class PlayerGeneratorAuthoringBaker : Baker<PlayerGeneratorAuthoring>
        {
            public override void Bake(PlayerGeneratorAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PlayerGeneratorData
                {
                    PlayerProtoType = GetEntity(authoring.playerPrefab, TransformUsageFlags.Dynamic),
                });
            }
        }
    }
}