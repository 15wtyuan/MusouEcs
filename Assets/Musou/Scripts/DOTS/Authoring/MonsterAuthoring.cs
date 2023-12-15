using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusouEcs
{
    public class MonsterAuthoring : MonoBehaviour
    {
        public int texIndex;
        public Vector2 texSize = Vector2.one;
        public Vector2 atlasSize;
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

                // 怪物动画相关
                // AddComponent(entity, new MusouSpriteData
                // {
                //     TexIndex = authoring.texIndex,
                //     AtlasRect = new Vector4(authoring.atlasSize.x, authoring.atlasSize.y, 1, 1),
                // });
                //
                // AddComponent(entity, new MusouSpriteAniData
                // {
                //     CurFrame = 1,
                // });
                //
                // var scale = new float3(1, 1, 1);
                // if (authoring.texSize.x > authoring.texSize.y)
                // {
                //     scale.y = authoring.texSize.y / authoring.texSize.x;
                // }
                // else if (authoring.texSize.x < authoring.texSize.y)
                // {
                //     scale.x = authoring.texSize.x / authoring.texSize.y;
                // }
                //
                // AddSharedComponent(entity, new MusouSpriteAniSharedData
                // {
                //     BeginFarme = authoring.beginFarme,
                //     EndFarme = authoring.endFarme,
                //     FrameRate = authoring.frameRate,
                //     Scale = scale,
                // });

                AddSharedComponent(entity, new MusouRenderAniSharedData
                {
                    BeginFarme = authoring.beginFarme,
                    EndFarme = authoring.endFarme,
                    FrameRate = authoring.frameRate,
                });

                AddComponent(entity, new MusouRenderFrameData
                {
                    Frame = authoring.beginFarme,
                });

                AddComponent(entity, new MusouRenderFaceData
                {
                    Face = 1,
                });

                AddComponent(entity, new MusouRenderAniData
                {
                    Timer = 0,
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