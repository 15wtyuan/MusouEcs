using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MusouEcs
{
    public class MonsterAuthoring : MonoBehaviour
    {
        public int beginFarme;
        public int endFarme;
        public int frameRate;

        public float speed;
        public int hp = 10;

        public float radius = 0.4f;
        public float radiusObst = 0.4f;
        public int maxNeighbors = 15;
        public float neighborDist = 5f;

        private class MonsterAuthoringBaker : Baker<MonsterAuthoring>
        {
            public override void Bake(MonsterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MonsterData
                {
                    Hp = authoring.hp,
                });

                // 渲染

                AddSharedComponent(entity, new MusouRenderAniSharedData
                {
                    BeginFarme = authoring.beginFarme,
                    EndFarme = authoring.endFarme,
                    FrameRate = authoring.frameRate,
                });

                AddComponent(entity, new MusouRenderFrameData
                {
                    Frame = Random.Range(authoring.beginFarme, authoring.endFarme + 1)
                });

                AddComponent(entity, new MusouRenderFaceData
                {
                    Face = 1,
                });

                AddComponent(entity, new MusouRenderAniData
                {
                    Timer = 0,
                });

                AddComponent(entity, new MusouRenderBlankData
                {
                    BlankOpacity = 0f,
                });

                AddComponent(entity, new MusouRenderBlankTimeData
                {
                    BlankEndTime = 0f,
                });

                //怪物移动相关
                AddComponent(entity, new MoveSpeedData
                {
                    Speed = authoring.speed,
                });

                AddComponent(entity, new MoveDirectionData
                {
                    Direction = float2.zero,
                });

                AddSharedComponent(entity, new OrcaSharedData
                {
                    Radius = authoring.radius,
                    RadiusObst = authoring.radiusObst,
                    MaxNeighbors = authoring.maxNeighbors,
                    NeighborDist = authoring.neighborDist,
                });

                AddComponent(entity, new RepelMoveData());
                SetComponentEnabled<RepelMoveData>(entity, false);
            }
        }
    }
}