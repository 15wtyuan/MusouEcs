using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace MusouEcs
{
    public struct SharedStaticPlayerData
    {
        public static readonly SharedStatic<SharedStaticPlayerData> SharedValue
            = SharedStatic<SharedStaticPlayerData>.GetOrCreate<SharedStaticPlayerData>();

        public Vector3 PlayerMoveDir;
        public Vector2 PlayerPosition;
        public int PlayerFace;
    }

    public struct SharedStaticMonsterData
    {
        public static readonly SharedStatic<SharedStaticMonsterData> SharedValue
            = SharedStatic<SharedStaticMonsterData>.GetOrCreate<SharedStaticMonsterData>();

        public NativeHashMap<int, Entity> GsbIndex2MonsterEntity;

        public SharedStaticMonsterData(int initialCapacity)
        {
            GsbIndex2MonsterEntity = new NativeHashMap<int, Entity>(initialCapacity, Allocator.Persistent);
        }
    }
}