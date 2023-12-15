using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace MusouEcs
{
    public struct MusouSpriteData : IComponentData, IEnableableComponent
    {
        public int TexIndex; // 纹理索引
        public Vector4 AtlasRect; // 图集列 图集行 显示列 显示行 
        public Matrix4x4 Matrix4X4; // 矩阵换算
        public int Face; //面朝方向
        public float BlankEndTime; //闪白结束时间
    }

    public struct MusouSpriteAniSharedData : ISharedComponentData
    {
        public int BeginFarme;
        public int EndFarme;
        public int FrameRate;
        public float3 Scale;
    }

    public struct MusouSpriteAniData : IComponentData
    {
        public float Timer; //计时器
        public int CurFrame; //当前帧
    }

    // 渲染
    public struct MusouRenderAniSharedData : ISharedComponentData
    {
        public int BeginFarme;
        public int EndFarme;
        public int FrameRate;
    }

    [MaterialProperty("_Frame")]
    public struct MusouRenderFrameData : IComponentData
    {
        public float Frame; //当前帧
    }

    [MaterialProperty("_Face")]
    public struct MusouRenderFaceData : IComponentData
    {
        public float Face; //当前帧
    }

    public struct MusouRenderAniData : IComponentData
    {
        public float Timer; //计时器
    }
}