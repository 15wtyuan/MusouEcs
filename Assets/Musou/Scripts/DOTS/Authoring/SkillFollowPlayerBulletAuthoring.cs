using Unity.Entities;
using UnityEngine;

namespace MusouEcs
{
    public class SkillFollowPlayerBulletAuthoring : MonoBehaviour
    {
        private class SkillFollowPlayerBulletAuthoringBaker : Baker<SkillFollowPlayerBulletAuthoring>
        {
            public override void Bake(SkillFollowPlayerBulletAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BulletFollowPlayerData());
            }
        }
    }
}