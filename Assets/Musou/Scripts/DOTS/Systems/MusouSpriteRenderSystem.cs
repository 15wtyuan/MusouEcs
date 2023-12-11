using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace MusouEcs
{
    internal struct AtlasData
    {
        public int TexIndex;
        public Vector4 AtlasRect;
        public float IsBlank;
    }

    [BurstCompile]
    internal struct RenderData : IComparable<RenderData>
    {
        public float3 Position;
        public Matrix4x4 Matrix;
        public AtlasData AtlasData;

        public int CompareTo(RenderData other)
        {
            // 按y轴排序
            return other.Position.y.CompareTo(Position.y);
        }
    }

    [BurstCompile]
    internal struct CombineArraysParallelJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<RenderData> NativeArray;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<Matrix4x4> Matrix4X4Array;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float> TexIndexArray;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<Vector4> AtlasRectArray;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float> IsBlankArray;

        public int StartIndex;

        public void Execute(int index)
        {
            var renderData = NativeArray[index];
            Matrix4X4Array[StartIndex + index] = renderData.Matrix;
            TexIndexArray[StartIndex + index] = renderData.AtlasData.TexIndex;
            AtlasRectArray[StartIndex + index] = renderData.AtlasData.AtlasRect;
            IsBlankArray[StartIndex + index] = renderData.AtlasData.IsBlank;
        }
    }


    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [RequireMatchingQueriesForUpdate]
    [UpdateAfter(typeof(MusouSpritePreRenderSystem))]
    public partial class MusouSpriteRenderSystem : SystemBase
    {
        private readonly int rectPropertyId = Shader.PropertyToID("_Rect");
        private readonly int texIndexPropertyId = Shader.PropertyToID("_TexIndex");
        private readonly int isBlankPropertyId = Shader.PropertyToID("_Blank");

        private const int SliceCount = 1023; // 一次渲染最大为1023

        private NativeQueue<RenderData> _nativeRenderQueue = new(Allocator.Persistent);
        private readonly MaterialPropertyBlock _materialPropertyBlock = new();

        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<MusouSpriteData>();
        }

        protected override void OnUpdate()
        {
            var camera = MusouCamera.Main;
            float3 cameraPosition = camera.transform.position;
            var orthographicSize = camera.orthographicSize + 1f; //防止图片过大带来误差
            var yBottom = cameraPosition.y - orthographicSize;
            var yTop = cameraPosition.y + orthographicSize;
            var screenHeight = Screen.height;
            var screenWidth = Screen.width;
            var horizonSize = orthographicSize / screenHeight * screenWidth;
            var xLeft = cameraPosition.x - horizonSize;
            var xRight = cameraPosition.x + horizonSize;

            var curtime = SystemAPI.Time.ElapsedTime;
            var playerPos = SharedStaticPlayerData.SharedValue.Data.PlayerPosition;

            foreach (var (transform, spriteDate) in
                     SystemAPI.Query<RefRO<LocalTransform>, RefRO<MusouSpriteData>>())
            {
                var posY = transform.ValueRO.Position.y;
                var posX = transform.ValueRO.Position.x;

                if (posY < yBottom || posY > yTop) continue;
                if (posX > xRight || posX < xLeft) continue;

                var renderData = new RenderData
                {
                    Position = transform.ValueRO.Position,
                    Matrix = spriteDate.ValueRO.Matrix4X4,
                    AtlasData = new AtlasData
                    {
                        TexIndex = spriteDate.ValueRO.TexIndex,
                        AtlasRect = spriteDate.ValueRO.AtlasRect,
                        IsBlank = spriteDate.ValueRO.BlankEndTime > curtime ? 1 : 0,
                    }
                };
                _nativeRenderQueue.Enqueue(renderData);
            }

            Render(_nativeRenderQueue);
            _nativeRenderQueue.Clear();
        }

        private void Render(NativeQueue<RenderData> renderQueue)
        {
            var nativeArray = renderQueue.ToArray(Allocator.TempJob);

            JobHandle chainHandle = new JobHandle();
            // Call sort
            chainHandle = MultithreadedSort.Sort(nativeArray, chainHandle);
            Dependency = chainHandle;
            CompleteDependency();

            var visibleEntityTotal = nativeArray.Length;

            var nativeMatrixArray =
                new NativeArray<Matrix4x4>(visibleEntityTotal, Allocator.TempJob);
            var nativeAtlasRectArray = new NativeArray<Vector4>(visibleEntityTotal, Allocator.TempJob);
            var nativeTexIndexArray = new NativeArray<float>(visibleEntityTotal, Allocator.TempJob);
            var nativeIsBlankArray = new NativeArray<float>(visibleEntityTotal, Allocator.TempJob);

            var combineArraysParallelJob = new CombineArraysParallelJob
            {
                StartIndex = 0,
                NativeArray = nativeArray,
                Matrix4X4Array = nativeMatrixArray,
                TexIndexArray = nativeTexIndexArray,
                AtlasRectArray = nativeAtlasRectArray,
                IsBlankArray = nativeIsBlankArray,
            };
            Dependency = combineArraysParallelJob.Schedule(nativeArray.Length, 10);
            CompleteDependency();
            nativeArray.Dispose();

            var matrixInstancedArray = new Matrix4x4[SliceCount];
            var atlasRectInstancedArray = new Vector4[SliceCount];
            var texIndexInstancedArray = new float[SliceCount];
            var isBlankInstancedArray = new float[SliceCount];

            for (var i = 0; i < visibleEntityTotal; i += SliceCount)
            {
                var sliceSize = math.min(visibleEntityTotal - i, SliceCount);

                NativeArray<Matrix4x4>.Copy(nativeMatrixArray, i, matrixInstancedArray, 0, sliceSize);
                NativeArray<Vector4>.Copy(nativeAtlasRectArray, i, atlasRectInstancedArray, 0, sliceSize);
                NativeArray<float>.Copy(nativeTexIndexArray, i, texIndexInstancedArray, 0, sliceSize);
                NativeArray<float>.Copy(nativeIsBlankArray, i, isBlankInstancedArray, 0, sliceSize);

                _materialPropertyBlock.SetVectorArray(rectPropertyId, atlasRectInstancedArray);
                _materialPropertyBlock.SetFloatArray(texIndexPropertyId, texIndexInstancedArray);
                _materialPropertyBlock.SetFloatArray(isBlankPropertyId, isBlankInstancedArray);

                Graphics.DrawMeshInstanced(
                    GameHandler.Instance.quadMesh,
                    0,
                    GameHandler.Instance.uniMaterial,
                    matrixInstancedArray,
                    sliceSize,
                    _materialPropertyBlock
                );
            }

            nativeMatrixArray.Dispose();
            nativeAtlasRectArray.Dispose();
        }
    }
}