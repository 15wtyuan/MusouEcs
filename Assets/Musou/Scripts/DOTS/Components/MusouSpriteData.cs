using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace MusouEcs
{
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

    [MaterialProperty("_BlankOpacity")]
    public struct MusouRenderBlankData : IComponentData
    {
        public float BlankOpacity; //闪白参数
    }

    public struct MusouRenderBlankTimeData : IComponentData
    {
        public double BlankEndTime; //闪白结束时间
    }

    public struct MusouRenderAniData : IComponentData
    {
        public float Timer; //计时器
    }
}