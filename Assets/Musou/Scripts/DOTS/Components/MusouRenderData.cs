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

    public struct MusouRenderAniData : IComponentData
    {
        public float Timer; //计时器
        public int Face; //面朝方向
    }
}