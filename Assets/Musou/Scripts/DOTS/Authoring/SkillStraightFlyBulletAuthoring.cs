using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusouEcs
{
    public class SkillStraightFlyBulletAuthoring : MonoBehaviour
    {
        public float flySpeed;

        private class SkillStraightFlyBulletAuthoringBaker : Baker<SkillStraightFlyBulletAuthoring>
        {
            public override void Bake(SkillStraightFlyBulletAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new StraightFlyBulletData
                {
                    FlySpeed = authoring.flySpeed,
                });
            }
        }
    }
}