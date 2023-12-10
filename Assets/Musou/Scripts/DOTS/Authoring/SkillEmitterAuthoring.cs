using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusouEcs
{
    public class SkillEmitterAuthoring : MonoBehaviour
    {
        public GameObject bulletPrefab;
        public float emitterInterval;
        public float createOffset;
        public int bulletCount;
        public float angleRange;
        public EmitterDirType emitterDirType;

        private class SkillEmitterAuthoringBaker : Baker<SkillEmitterAuthoring>
        {
            public override void Bake(SkillEmitterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new EmitterData
                {
                    BulletProtoType = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic),
                    EmitterInterval = authoring.emitterInterval,
                    CreateOffset = authoring.createOffset,
                    BulletCount = authoring.bulletCount,
                    AngleRange = authoring.angleRange,
                    EmitterDirType = (int)authoring.emitterDirType,
                });

                AddComponent(entity, new EmitterTimerData
                {
                    Timer = 0,
                });
            }
        }
    }
}