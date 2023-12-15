using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace MusouEcs
{
    public class PlayerAuthoring : MonoBehaviour
    {
        public int beginFarme;
        public int endFarme;
        public int frameRate;

        public float speed;

        private class PlayerAuthoringBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerData());
                AddComponent(entity, new MoveSpeedData
                {
                    Speed = authoring.speed,
                });

                AddComponent(entity, new MoveDirectionData
                {
                    Direction = float2.zero,
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

                AddComponent(entity, new MusouRenderBlankData
                {
                    BlankOpacity = 0f,
                });

                AddComponent(entity, new MusouRenderBlankTimeData
                {
                    BlankEndTime = 0f,
                });
            }
        }
    }
}