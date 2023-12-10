using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace MusouEcs
{
    public struct MusouRenderAniSharedData : ISharedComponentData
    {
        public int BeginFarme;
        public int EndFarme;
        public int FrameRate;
        public float3 Scale;
    }

    [MaterialProperty("_Frame")]
    public struct MusouRenderFrameData : IComponentData
    {
        public float CurFrame; //当前帧
    }
    
    [MaterialProperty("_Face")]
    public struct MusouRenderFaceData : IComponentData
    {
        public float Face;
    }

    public struct MusouRenderAniData : IComponentData
    {
        public float Timer; //计时器
    }
}