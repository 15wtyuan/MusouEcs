using Unity.Entities;
using UnityEngine;

namespace MusouEcs
{
    public class MonsterGeneratorAuthoring : MonoBehaviour
    {
        public GameObject monsterPrefab = null;
        public int cntX = 100;
        public int cntY = 100;


        class MonsterGeneratorAuthoringBaker : Baker<MonsterGeneratorAuthoring>
        {
            public override void Bake(MonsterGeneratorAuthoring generatorAuthoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MonsterGeneratorData
                {
                    MonsterProtoType = GetEntity(generatorAuthoring.monsterPrefab, TransformUsageFlags.Dynamic),
                    CntX = generatorAuthoring.cntX,
                    CntY = generatorAuthoring.cntY
                });
            }
        }
    }
}