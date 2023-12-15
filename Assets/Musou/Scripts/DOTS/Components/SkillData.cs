using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

namespace MusouEcs
{
    public struct SkillData : IComponentData
    {
    }

    public struct SkillDamageData : IComponentData
    {
        public Entity SkillEntity;
    }

    public struct SkillDamageSharedData : ISharedComponentData
    {
        public float DamageRadius;
        public int Damage;

        public float RepelTime;
        public float RepelSpeed;
    }

    // 子弹数据
    public struct BulletDamageData : IComponentData
    {
        public float DamageInterval;
    }

    public class BulletDamageDictData : IComponentData, IDisposable
    {
        public Dictionary<Entity, float> DamageDict;

        public void Dispose()
        {
            DamageDict.Clear();
            DamageDict = null;
        }
    }

    public struct BulletTranslateData : IComponentData
    {
        public float3 Delta;
        public float3 FollowPosDelta;
        public float3 LastDelta;
    }

    public struct BulletFollowPlayerData : IComponentData
    {
        public float3 LastPlayerPos;
    }

    public struct BulletBornData : IComponentData
    {
        public float3 BornPos;
        public float3 EmitterPos;
    }

    public struct BulletLifeData : IComponentData
    {
        public float Timer;
        public float LifeTime;
    }

    public struct StraightFlyBulletData : IComponentData
    {
        public float3 FlyDir;
        public float FlySpeed;
    }

    public struct RotateFlyBulletData : IComponentData
    {
        public float3 FollowPos;
        public float RotateSpeed;
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
        public float CreateOffset;
        public int BulletCount;
        public float AngleRange;
        public int EmitterDirType;
    }

    public struct EmitterTimerData : IComponentData
    {
        public float Timer;
    }
}