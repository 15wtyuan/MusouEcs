using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace MusouEcs
{
    public class SkillBulletAuthoring : MonoBehaviour
    {
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
            }
        }
    }
}