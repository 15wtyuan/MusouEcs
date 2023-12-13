using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusouEcs
{
    public class SkillDamageAuthoring : MonoBehaviour
    {
        public float damageRadius = 1;
        public int damage = 1;
        public GameObject bullet;

        public float repelTime;
        public float repelSpeed;

        private class SkillDamageAuthoringBaker : Baker<SkillDamageAuthoring>
        {
            public override void Bake(SkillDamageAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new SkillDamageData
                {
                    SkillEntity = GetEntity(authoring.bullet, TransformUsageFlags.Dynamic),
                });

                AddSharedComponent(entity, new SkillDamageSharedData
                {
                    DamageRadius = authoring.damageRadius,
                    Damage = authoring.damage,
                    RepelTime = authoring.repelTime,
                    RepelSpeed = authoring.repelSpeed,
                });
            }
        }
    }
}