using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusouEcs
{
    public class PlayerAuthoring : MonoBehaviour
    {
        public float speed;

        private class PlayerAuthoringBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerData());
                AddComponent(entity, new SpeedData
                {
                    Speed = authoring.speed,
                });
            }
        }
    }
}