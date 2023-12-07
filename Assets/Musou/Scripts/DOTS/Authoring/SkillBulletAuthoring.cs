using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusouEcs
{
    public class SkillBulletAuthoring : MonoBehaviour
    {
        public float damageRadius = 1;

        public int damage = 1;


        private class SkillBulletAuthoringBaker : Baker<SkillBulletAuthoring>
        {
            public override void Bake(SkillBulletAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BulletTranslateData());

                var playerPos = SharedStaticPlayerData.SharedValue.Data.PlayerPosition;
                AddComponent(entity, new BulletFollowPlayerData
                {
                    LastPlayerPos = new float3(playerPos.x, playerPos.y, 0),
                });

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