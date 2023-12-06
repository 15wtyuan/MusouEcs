using Unity.Entities;
using Unity.Mathematics;

namespace MusouEcs
{
    public struct SkillData : IComponentData
    {
    }

    public struct SkillDamageData : ISharedComponentData
    {
        public float3 DamageRadius;
        public int Damage;
    }

    // 子弹数据
    public struct BulletTranslateData : IComponentData
    {
        public float3 Delta;
        public float3 FollowPosDelta;
    }

    public struct BulletBornData : IComponentData
    {
        public float3 BornPos;
        public float3 EmitterPos;
    }

    public struct BulletLifeData : IComponentData
    {
        public float Timer;
    }

    public struct StraightFlyBulletData : IComponentData
    {
        public float3 FlyDir;
        public float FlySpeed;
    }

    public struct RotateFlyBulletData : IComponentData
    {
        public float3 FollowPos;
        public float3 RotateSpeed;
    }

    public struct RotateSelfBulletData : IComponentData
    {
        public float3 RotateSpeed;
    }

    // 发射器数据
    public enum EmitterDirType
    {
        None,
        PlayerFace,
        Random,
        CloserTarget,
        PlayerMoveDir,
    }

    public struct EmitterData : IComponentData
    {
        public Entity BulletProtoType;
        public float EmitterInterval;
        public int BulletCount;
        public int EmitterDirType;
    }

    public struct EmitterTimerData : IComponentData
    {
        public float Timer;
    }
}