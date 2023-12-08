using Unity.Entities;
using UnityEngine;

namespace MusouEcs
{
    public class SkillDamageAuthoring : MonoBehaviour
    {
        public float damageRadius = 1;

        public int damage = 1;
        
        private class SkillDamageAuthoringBaker : Baker<SkillDamageAuthoring>
        {
            public override void Bake(SkillDamageAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent<SkillDamageTag>(entity);

                AddSharedComponent(entity, new SkillDamageData
                {
                    DamageRadius = authoring.damageRadius,
                    Damage = authoring.damage,
                });
            }
        }
    }
}