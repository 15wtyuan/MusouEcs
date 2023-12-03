using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace MusouEcs
{
    public struct MusouSpriteData : IComponentData
    {
        public Vector4 AtlasData; // 图集列 图集行 显示列 显示行 
        public Matrix4x4 Matrix4X4; // 矩阵换算
        public int Face; //面朝方向
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
}