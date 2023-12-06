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
    internal struct RenderData
    {
        public float3 Position;
        public Matrix4x4 Matrix;
        public Vector4 AtlasData;
    }

    internal struct PointDataComparer : IComparer<RenderData>
    {
        public int Compare(RenderData a, RenderData b)
        {
            var c = b.Position.y - a.Position.y;
            if (c == 0)
            {
                return 0;
            }

            return c > 0 ? 1 : -1;
        }
    }

    [BurstCompile]
    internal struct SortByPositionJob : IJob
    {
        public NativeArray<RenderData> SortArray;

        public void Execute()
        {
            SortArray.Sort(new PointDataComparer());
        }
    }

    [BurstCompile]
    internal struct CombineArraysParallelJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<RenderData> NativeArray;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<Matrix4x4> Matrix4X4Array;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<Vector4> AtlasData;

        public int StartIndex;

        public void Execute(int index)
        {
            var renderData = NativeArray[index];
            Matrix4X4Array[StartIndex + index] = renderData.Matrix;
            AtlasData[StartIndex + index] = renderData.AtlasData;
        }
    }


    [UpdateInGroup(typeof(MusouUpdateGroup))]
    [RequireMatchingQueriesForUpdate]
    [UpdateAfter(typeof(MusouSpritePreRenderSystem))]
    public partial class MusouSpriteRenderSystem : SystemBase
    {
        private NativeQueue<RenderData> _nativeQueue = new(Allocator.Persistent);
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

            foreach (var (transform, spriteDate) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<MusouSpriteData>>())
            {
                var posY = transform.ValueRW.Position.y;
                var posX = transform.ValueRW.Position.x;

                if (posY < yBottom || posY > yTop) continue;
                if (posX > xRight || posX < xLeft) continue;

                var renderData = new RenderData
                {
                    Matrix = spriteDate.ValueRO.Matrix4X4,
                    Position = transform.ValueRW.Position,
                    AtlasData = spriteDate.ValueRO.AtlasData
                };
                _nativeQueue.Enqueue(renderData);
            }

            var nativeArray = _nativeQueue.ToArray(Allocator.TempJob);
            _nativeQueue.Clear();

            var sortJob = new SortByPositionJob
            {
                SortArray = nativeArray
            };
            Dependency = sortJob.Schedule();
            CompleteDependency();

            var visibleEntityTotal = nativeArray.Length;
            var nativeMatrixArray =
                new NativeArray<Matrix4x4>(visibleEntityTotal, Allocator.TempJob);
            var nativeAtlasData = new NativeArray<Vector4>(visibleEntityTotal, Allocator.TempJob);

            var combineArraysParallelJob = new CombineArraysParallelJob
            {
                StartIndex = 0,
                NativeArray = nativeArray,
                Matrix4X4Array = nativeMatrixArray,
                AtlasData = nativeAtlasData
            };
            Dependency = combineArraysParallelJob.Schedule(nativeArray.Length, 10);
            CompleteDependency();
            nativeArray.Dispose();

            var rectPropertyId = Shader.PropertyToID("_Rect");
            const int sliceCount = 1023; // 一次渲染最大为1023
            var matrixInstancedArray = new Matrix4x4[sliceCount];
            var uvInstancedArray = new Vector4[sliceCount];

            for (var i = 0; i < visibleEntityTotal; i += sliceCount)
            {
                var sliceSize = math.min(visibleEntityTotal - i, sliceCount);

                NativeArray<Matrix4x4>.Copy(nativeMatrixArray, i, matrixInstancedArray, 0, sliceSize);
                NativeArray<Vector4>.Copy(nativeAtlasData, i, uvInstancedArray, 0, sliceSize);
                _materialPropertyBlock.SetVectorArray(rectPropertyId, uvInstancedArray);

                Graphics.DrawMeshInstanced(
                    GameHandler.Instance.quadMesh,
                    0,
                    GameHandler.Instance.unitMaterial,
                    matrixInstancedArray,
                    sliceSize,
                    _materialPropertyBlock
                );
            }

            nativeMatrixArray.Dispose();
            nativeAtlasData.Dispose();
        }
    }
}