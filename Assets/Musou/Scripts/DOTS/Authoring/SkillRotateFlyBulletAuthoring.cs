using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusouEcs
{
    public class SkillRotateFlyBulletAuthoring : MonoBehaviour
    {
        public float rotateSpeed;

        private class SkillRotateFlyBulletAuthoringBaker : Baker<SkillRotateFlyBulletAuthoring>
        {
            public override void Bake(SkillRotateFlyBulletAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new RotateFlyBulletData
                {
                    RotateSpeed = authoring.rotateSpeed,
                });
            }
        }
    }
}