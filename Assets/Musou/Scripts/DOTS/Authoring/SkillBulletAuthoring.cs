using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace MusouEcs
{
    public class SkillBulletAuthoring : MonoBehaviour
    {
        public float lifeTime;
        public float damageInterval;

        private class SkillBulletAuthoringBaker : Baker<SkillBulletAuthoring>
        {
            public override void Bake(SkillBulletAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BulletTranslateData());
                AddComponent(entity, new BulletLifeData
                {
                    Timer = 0,
                    LifeTime = authoring.lifeTime,
                });

                AddComponent(entity, new BulletDamageData
                {
                    DamageInterval = authoring.damageInterval,
                });

                AddComponentObject(entity, new BulletDamageDictData
                {
                    DamageDict = new Dictionary<Entity, float>(),
                });
            }
        }
    }
}