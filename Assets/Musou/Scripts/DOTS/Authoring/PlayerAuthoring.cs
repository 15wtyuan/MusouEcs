﻿using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusouEcs
{
    public class PlayerAuthoring : MonoBehaviour
    {
        public int texIndex;
        public Vector2 texSize = Vector2.one;
        public Vector2 atlasSize;
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
                AddComponent(entity, new SpeedData
                {
                    Speed = authoring.speed,
                });

                // 渲染相关, 这里的版本是使用shader graph 显示，但是没解决渲染排序问题
                // AddComponent(entity, new MusouRenderAniData());
                //
                // AddComponent(entity, new MusouRenderFrameData
                // {
                //     CurFrame = 1,
                // });
                //
                // AddComponent(entity, new MusouRenderFaceData
                // {
                //     Face = 1,
                // });
                //
                // AddSharedComponent(entity, new MusouRenderAniSharedData
                // {
                //     BeginFarme = authoring.beginFarme,
                //     EndFarme = authoring.endFarme,
                //     FrameRate = authoring.frameRate,
                // });

                AddComponent(entity, new MusouSpriteData
                {
                    TexIndex = authoring.texIndex,
                    AtlasRect = new Vector4(authoring.atlasSize.x, authoring.atlasSize.y, 1, 1),
                    Face = 1,
                });

                AddComponent(entity, new MusouSpriteAniData
                {
                    CurFrame = 1,
                });

                var scale = new float3(1, 1, 1);
                if (authoring.texSize.x > authoring.texSize.y)
                {
                    scale.y = authoring.texSize.y / authoring.texSize.x;
                }
                else if (authoring.texSize.x < authoring.texSize.y)
                {
                    scale.x = authoring.texSize.x / authoring.texSize.y;
                }

                AddSharedComponent(entity, new MusouSpriteAniSharedData
                {
                    BeginFarme = authoring.beginFarme,
                    EndFarme = authoring.endFarme,
                    FrameRate = authoring.frameRate,
                    Scale = scale,
                });

                AddComponent(entity, new OrcaDynamicData
                {
                    Direction = float3.zero,
                });
            }
        }
    }
}