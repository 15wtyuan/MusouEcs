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
    }
}