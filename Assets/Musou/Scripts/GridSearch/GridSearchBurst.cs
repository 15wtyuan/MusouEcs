using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MusouEcs
{
    public class GridSearchBurst
    {
        private const int Maxgridsize = 256;

        private NativeArray<float3> positions;
        private NativeArray<float3> sortedPos;
        private NativeArray<int2> hashIndex;
        private NativeArray<int2> cellStartEnd;

        private float3 minValue = float3.zero;
        private float3 maxValue = float3.zero;
        private int3 gridDim = int3.zero;

        private float gridReso = -1.0f;
        private readonly int targetGridSize;

        private bool _isInit;
        public bool IsInit => _isInit;

        public GridSearchBurst(float resolution, int targetGrid = 32)
        {
            if (resolution <= 0.0f && targetGrid > 0)
            {
                targetGridSize = targetGrid;
                return;
            }

            if (resolution <= 0.0f && targetGrid <= 0)
            {
                throw new Exception("Wrong target grid size. Choose a resolution > 0 or a target grid > 0");
            }

            gridReso = resolution;
        }

        public void InitGrid(Vector3[] pos)
        {
            positions = new NativeArray<Vector3>(pos, Allocator.Persistent).Reinterpret<float3>();

            _initGrid();
        }

        public void InitGrid(NativeArray<float3> pos)
        {
            positions = new NativeArray<float3>(pos.Length, Allocator.Persistent);
            pos.CopyTo(positions);

            _initGrid();
        }

        private void _initGrid()
        {
            if (positions.Length == 0)
            {
                throw new Exception("Empty position buffer");
            }

            GetMinMaxCoords(positions, out minValue, out maxValue);

            float3 sidelen = maxValue - minValue;
            float maxDist = math.max(sidelen.x, math.max(sidelen.y, sidelen.z));

            //Compute a resolution so that the grid is equal to 32*32*32 cells
            if (gridReso <= 0.0f)
            {
                gridReso = maxDist / targetGridSize;
            }

            int gridSize = math.max(1, (int)math.ceil(maxDist / gridReso));
            gridDim = new int3(gridSize, gridSize, gridSize);

            if (gridSize > Maxgridsize)
            {
                throw new Exception("Grid is to large, adjust the resolution");
            }

            int nCells = gridDim.x * gridDim.y * gridDim.z;

            hashIndex = new NativeArray<int2>(positions.Length, Allocator.Persistent);
            sortedPos = new NativeArray<float3>(positions.Length, Allocator.Persistent);
            cellStartEnd = new NativeArray<int2>(nCells, Allocator.Persistent);

            var assignHashJob = new AssignHashJob
            {
                OriGrid = minValue,
                InvresoGrid = 1.0f / gridReso,
                GridDim = gridDim,
                Pos = positions,
                Nbcells = nCells,
                HashIndex = hashIndex
            };
            var assignHashJobHandle = assignHashJob.Schedule(positions.Length, 128);
            assignHashJobHandle.Complete();

            NativeArray<SortEntry> entries = new NativeArray<SortEntry>(positions.Length, Allocator.TempJob);

            var populateJob = new PopulateEntryJob
            {
                HashIndex = hashIndex,
                Entries = entries
            };
            var populateJobHandle = populateJob.Schedule(positions.Length, 128);
            populateJobHandle.Complete();


            // --- Here we could create a list for each filled cell of the grid instead of allocating the whole grid ---
            // hashIndex.Sort(new int2Comparer());//Sort by hash SUPER SLOW !

            // ------- Sort by hash

            JobHandle handle1 = new JobHandle();
            JobHandle chainHandle = MultithreadedSort.Sort(entries, handle1);
            chainHandle.Complete();
            handle1.Complete();

            var depopulateJob = new DePopulateEntryJob
            {
                HashIndex = hashIndex,
                Entries = entries
            };

            var depopulateJobHandle = depopulateJob.Schedule(positions.Length, 128);
            depopulateJobHandle.Complete();

            entries.Dispose();

            // ------- Sort (end)

            var memsetCellStartJob = new MemsetCellStartJob
            {
                CellStartEnd = cellStartEnd
            };
            var memsetCellStartJobHandle = memsetCellStartJob.Schedule(nCells, 256);
            memsetCellStartJobHandle.Complete();

            var sortCellJob = new SortCellJob
            {
                Pos = positions,
                HashIndex = hashIndex,
                CellStartEnd = cellStartEnd,
                SortedPos = sortedPos
            };


            var sortCellJobHandle = sortCellJob.Schedule();
            sortCellJobHandle.Complete();

            _isInit = true;
        }


        public void Clean()
        {
            if (positions.IsCreated)
                positions.Dispose();
            if (hashIndex.IsCreated)
                hashIndex.Dispose();
            if (cellStartEnd.IsCreated)
                cellStartEnd.Dispose();
            if (sortedPos.IsCreated)
                sortedPos.Dispose();
        }

        public void UpdatePositions(Vector3[] newPos)
        {
            NativeArray<float3> tempPositions =
                new NativeArray<Vector3>(newPos, Allocator.TempJob).Reinterpret<float3>();
            UpdatePositions(tempPositions);
            tempPositions.Dispose();
        }

        ///Update the grid with new positions -> avoid allocating memory if not needed
        public void UpdatePositions(NativeArray<float3> newPos)
        {
            if (newPos.Length != positions.Length)
            {
                return;
            }

            newPos.CopyTo(positions);

            GetMinMaxCoords(positions, out minValue, out maxValue);

            float3 sidelen = maxValue - minValue;
            float maxDist = math.max(sidelen.x, math.max(sidelen.y, sidelen.z));

            int gridSize = (int)math.ceil(maxDist / gridReso);
            gridDim = new int3(gridSize, gridSize, gridSize);

            if (gridSize > Maxgridsize)
            {
                throw new Exception("Grid is to large, adjust the resolution");
            }

            int nCells = gridDim.x * gridDim.y * gridDim.z;

            if (nCells != cellStartEnd.Length)
            {
                cellStartEnd.Dispose();
                cellStartEnd = new NativeArray<int2>(nCells, Allocator.Persistent);
            }

            var assignHashJob = new AssignHashJob
            {
                OriGrid = minValue,
                InvresoGrid = 1.0f / gridReso,
                GridDim = gridDim,
                Pos = positions,
                Nbcells = nCells,
                HashIndex = hashIndex
            };
            var assignHashJobHandle = assignHashJob.Schedule(positions.Length, 128);
            assignHashJobHandle.Complete();


            NativeArray<SortEntry> entries = new NativeArray<SortEntry>(positions.Length, Allocator.TempJob);

            var populateJob = new PopulateEntryJob
            {
                HashIndex = hashIndex,
                Entries = entries
            };
            var populateJobHandle = populateJob.Schedule(positions.Length, 128);
            populateJobHandle.Complete();


            // --- Here we could create a list for each filled cell of the grid instead of allocating the whole grid ---
            // hashIndex.Sort(new int2Comparer());//Sort by hash SUPER SLOW !

            // ------- Sort by hash

            JobHandle handle1 = new JobHandle();
            JobHandle chainHandle = MultithreadedSort.Sort(entries, handle1);
            chainHandle.Complete();
            handle1.Complete();

            var depopulateJob = new DePopulateEntryJob
            {
                HashIndex = hashIndex,
                Entries = entries
            };

            var depopulateJobHandle = depopulateJob.Schedule(positions.Length, 128);
            depopulateJobHandle.Complete();

            entries.Dispose();

            // ------- Sort (end)

            var memsetCellStartJob = new MemsetCellStartJob
            {
                CellStartEnd = cellStartEnd
            };
            var memsetCellStartJobHandle = memsetCellStartJob.Schedule(nCells, 256);
            memsetCellStartJobHandle.Complete();

            var sortCellJob = new SortCellJob
            {
                Pos = positions,
                HashIndex = hashIndex,
                CellStartEnd = cellStartEnd,
                SortedPos = sortedPos
            };

            var sortCellJobHandle = sortCellJob.Schedule();
            sortCellJobHandle.Complete();
        }

        public int[] SearchClosestPoint(Vector3[] queryPoints, bool checkSelf = false, float epsilon = 0.001f)
        {
            NativeArray<float3> qPoints =
                new NativeArray<Vector3>(queryPoints, Allocator.TempJob).Reinterpret<float3>();
            NativeArray<int> results = new NativeArray<int>(queryPoints.Length, Allocator.TempJob);

            var closestPointJob = new ClosestPointJob
            {
                OriGrid = minValue,
                InvresoGrid = 1.0f / gridReso,
                GridDim = gridDim,
                QueryPos = qPoints,
                SortedPos = sortedPos,
                HashIndex = hashIndex,
                CellStartEnd = cellStartEnd,
                Results = results,
                IgnoreSelf = checkSelf,
                SquaredepsilonSelf = epsilon * epsilon
            };

            var closestPointJobHandle = closestPointJob.Schedule(qPoints.Length, 16);
            closestPointJobHandle.Complete();

            int[] res = new int[qPoints.Length];
            results.CopyTo(res);

            qPoints.Dispose();
            results.Dispose();

            return res;
        }

        public NativeArray<int> SearchClosestPoint(NativeArray<float3> qPoints, bool checkSelf = false,
            float epsilon = 0.001f)
        {
            NativeArray<int> results = new NativeArray<int>(qPoints.Length, Allocator.TempJob);

            var closestPointJob = new ClosestPointJob
            {
                OriGrid = minValue,
                InvresoGrid = 1.0f / gridReso,
                GridDim = gridDim,
                QueryPos = qPoints,
                SortedPos = sortedPos,
                HashIndex = hashIndex,
                CellStartEnd = cellStartEnd,
                Results = results,
                IgnoreSelf = checkSelf,
                SquaredepsilonSelf = epsilon * epsilon
            };

            var closestPointJobHandle = closestPointJob.Schedule(qPoints.Length, 16);
            closestPointJobHandle.Complete();

            return results;
        }

        public int[] SearchWithin(Vector3[] queryPoints, float rad, int maxNeighborPerQuery)
        {
            NativeArray<float3> qPoints =
                new NativeArray<Vector3>(queryPoints, Allocator.TempJob).Reinterpret<float3>();
            NativeArray<int> results =
                new NativeArray<int>(queryPoints.Length * maxNeighborPerQuery, Allocator.TempJob);
            int cellsToLoop = (int)math.ceil(rad / gridReso);

            var withinJob = new FindWithinJob
            {
                SquaredRadius = rad * rad,
                MaxNeighbor = maxNeighborPerQuery,
                CellsToLoop = cellsToLoop,
                OriGrid = minValue,
                InvresoGrid = 1.0f / gridReso,
                GridDim = gridDim,
                QueryPos = qPoints,
                SortedPos = sortedPos,
                HashIndex = hashIndex,
                CellStartEnd = cellStartEnd,
                Results = results
            };

            var withinJobHandle = withinJob.Schedule(qPoints.Length, 16);
            withinJobHandle.Complete();

            int[] res = new int[results.Length];
            results.CopyTo(res);

            qPoints.Dispose();
            results.Dispose();

            return res;
        }

        public NativeArray<int> SearchWithin(NativeArray<float3> queryPoints, float rad, int maxNeighborPerQuery)
        {
            NativeArray<int> results =
                new NativeArray<int>(queryPoints.Length * maxNeighborPerQuery, Allocator.TempJob);

            int cellsToLoop = (int)math.ceil(rad / gridReso);

            var withinJob = new FindWithinJob
            {
                SquaredRadius = rad * rad,
                MaxNeighbor = maxNeighborPerQuery,
                CellsToLoop = cellsToLoop,
                OriGrid = minValue,
                InvresoGrid = 1.0f / gridReso,
                GridDim = gridDim,
                QueryPos = queryPoints,
                SortedPos = sortedPos,
                HashIndex = hashIndex,
                CellStartEnd = cellStartEnd,
                Results = results
            };

            var withinJobHandle = withinJob.Schedule(queryPoints.Length, 16);
            withinJobHandle.Complete();

            return results;
        }

        private static void GetMinMaxCoords(NativeArray<float3> mpos, out float3 minV, out float3 maxV)
        {
            NativeArray<float3> tmpmin = new NativeArray<float3>(1, Allocator.TempJob);
            NativeArray<float3> tmpmax = new NativeArray<float3>(1, Allocator.TempJob);
            var mmJob = new GetminmaxJob
            {
                MinVal = tmpmin,
                MaxVal = tmpmax,
                Pos = mpos
            };
            var mmJobHandle = mmJob.Schedule(mpos.Length, new JobHandle());
            mmJobHandle.Complete();
            minV = tmpmin[0];
            maxV = tmpmax[0];
            tmpmin.Dispose();
            tmpmax.Dispose();
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct GetminmaxJob : IJobFor
        {
            public NativeArray<float3> MinVal;
            public NativeArray<float3> MaxVal;
            [ReadOnly] public NativeArray<float3> Pos;

            void IJobFor.Execute(int i)
            {
                if (i == 0)
                {
                    MinVal[0] = Pos[0];
                    MaxVal[0] = Pos[0];
                }
                else
                {
                    var x = math.min(MinVal[0].x, Pos[i].x);
                    var y = math.min(MinVal[0].y, Pos[i].y);
                    var z = math.min(MinVal[0].z, Pos[i].z);
                    MinVal[0] = new float3(x, y, z);
                    x = math.max(MaxVal[0].x, Pos[i].x);
                    y = math.max(MaxVal[0].y, Pos[i].y);
                    z = math.max(MaxVal[0].z, Pos[i].z);
                    MaxVal[0] = new float3(x, y, z);
                }
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct AssignHashJob : IJobParallelFor
        {
            [ReadOnly] public float3 OriGrid;
            [ReadOnly] public float InvresoGrid;
            [ReadOnly] public int3 GridDim;
            [ReadOnly] public int Nbcells;
            [ReadOnly] public NativeArray<float3> Pos;
            public NativeArray<int2> HashIndex;

            void IJobParallelFor.Execute(int index)
            {
                float3 p = Pos[index];

                int3 cell = SpaceToGrid(p, OriGrid, InvresoGrid);
                cell = math.clamp(cell, new int3(0, 0, 0), GridDim - new int3(1, 1, 1));
                int hash = Flatten3DTo1D(cell, GridDim);
                hash = math.clamp(hash, 0, Nbcells - 1);

                int2 v;
                v.x = hash;
                v.y = index;

                HashIndex[index] = v;
            }
        }


        [BurstCompile(CompileSynchronously = true)]
        private struct MemsetCellStartJob : IJobParallelFor
        {
            public NativeArray<int2> CellStartEnd;

            void IJobParallelFor.Execute(int index)
            {
                int2 v;
                v.x = int.MaxValue - 1;
                v.y = int.MaxValue - 1;
                CellStartEnd[index] = v;
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct SortCellJob : IJob
        {
            [ReadOnly] public NativeArray<float3> Pos;
            [ReadOnly] public NativeArray<int2> HashIndex;

            public NativeArray<int2> CellStartEnd;

            public NativeArray<float3> SortedPos;

            void IJob.Execute()
            {
                for (int index = 0; index < HashIndex.Length; index++)
                {
                    int hash = HashIndex[index].x;
                    int id = HashIndex[index].y;
                    int2 newV;

                    int hashm1 = -1;
                    if (index != 0)
                        hashm1 = HashIndex[index - 1].x;


                    if (index == 0 || hash != hashm1)
                    {
                        newV.x = index;
                        newV.y = CellStartEnd[hash].y;

                        CellStartEnd[hash] = newV; // set start

                        if (index != 0)
                        {
                            newV.x = CellStartEnd[hashm1].x;
                            newV.y = index;
                            CellStartEnd[hashm1] = newV; // set end
                        }
                    }

                    if (index == Pos.Length - 1)
                    {
                        newV.x = CellStartEnd[hash].x;
                        newV.y = index + 1;

                        CellStartEnd[hash] = newV; // set end
                    }

                    // Reorder atoms according to sorted indices
                    SortedPos[index] = Pos[id];
                }
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct ClosestPointJob : IJobParallelFor
        {
            [ReadOnly] public float3 OriGrid;
            [ReadOnly] public float InvresoGrid;
            [ReadOnly] public int3 GridDim;
            [ReadOnly] public NativeArray<float3> QueryPos;
            [ReadOnly] public NativeArray<int2> CellStartEnd;
            [ReadOnly] public NativeArray<float3> SortedPos;
            [ReadOnly] public NativeArray<int2> HashIndex;
            [ReadOnly] public bool IgnoreSelf;
            [ReadOnly] public float SquaredepsilonSelf;

            public NativeArray<int> Results;

            void IJobParallelFor.Execute(int index)
            {
                Results[index] = -1;
                float3 p = QueryPos[index];

                int3 cell = SpaceToGrid(p, OriGrid, InvresoGrid);
                cell = math.clamp(cell, new int3(0, 0, 0), GridDim - new int3(1, 1, 1));

                float minD = float.MaxValue;
                int minRes = -1;


                cell = math.clamp(cell, new int3(0, 0, 0), GridDim - new int3(1, 1, 1));


                int neighcellhashf = Flatten3DTo1D(cell, GridDim);
                int idStartf = CellStartEnd[neighcellhashf].x;
                int idStopf = CellStartEnd[neighcellhashf].y;

                if (idStartf < int.MaxValue - 1)
                {
                    for (int id = idStartf; id < idStopf; id++)
                    {
                        float3 posA = SortedPos[id];
                        float d = math.distancesq(p, posA); //Squared distance

                        if (d < minD)
                        {
                            if (IgnoreSelf)
                            {
                                if (d > SquaredepsilonSelf)
                                {
                                    minRes = id;
                                    minD = d;
                                }
                            }
                            else
                            {
                                minRes = id;
                                minD = d;
                            }
                        }
                    }
                }

                if (minRes != -1)
                {
                    Results[index] = HashIndex[minRes].y;
                    return;
                }

                //Corresponding cell was empty, let's search in neighbor cells
                for (int x = -1; x <= 1; x++)
                {
                    int3 curGridId;
                    curGridId.x = cell.x + x;
                    if (curGridId.x >= 0 && curGridId.x < GridDim.x)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            curGridId.y = cell.y + y;
                            if (curGridId.y >= 0 && curGridId.y < GridDim.y)
                            {
                                for (int z = -1; z <= 1; z++)
                                {
                                    curGridId.z = cell.z + z;
                                    if (curGridId.z >= 0 && curGridId.z < GridDim.z)
                                    {
                                        int neighcellhash = Flatten3DTo1D(curGridId, GridDim);
                                        int idStart = CellStartEnd[neighcellhash].x;
                                        int idStop = CellStartEnd[neighcellhash].y;

                                        if (idStart < int.MaxValue - 1)
                                        {
                                            for (int id = idStart; id < idStop; id++)
                                            {
                                                float3 posA = SortedPos[id];
                                                float d = math.distancesq(p, posA); //Squared distance

                                                if (d < minD)
                                                {
                                                    if (IgnoreSelf)
                                                    {
                                                        if (d > SquaredepsilonSelf)
                                                        {
                                                            minRes = id;
                                                            minD = d;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        minRes = id;
                                                        minD = d;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (minRes != -1)
                {
                    Results[index] = HashIndex[minRes].y;
                }
                else
                {
                    //Neighbor cells do not contain anything => compute all distances
                    //Compute all the distances ! = SLOW
                    for (int id = 0; id < SortedPos.Length; id++)
                    {
                        float3 posA = SortedPos[id];
                        float d = math.distancesq(p, posA); //Squared distance

                        if (d < minD)
                        {
                            if (IgnoreSelf)
                            {
                                if (d > SquaredepsilonSelf)
                                {
                                    minRes = id;
                                    minD = d;
                                }
                            }
                            else
                            {
                                minRes = id;
                                minD = d;
                            }
                        }
                    }

                    Results[index] = HashIndex[minRes].y;
                }
            }
        }


        [BurstCompile(CompileSynchronously = true)]
        private struct FindWithinJob : IJobParallelFor
        {
            [ReadOnly] public float SquaredRadius;
            [ReadOnly] public int MaxNeighbor;
            [ReadOnly] public int CellsToLoop;
            [ReadOnly] public float3 OriGrid;
            [ReadOnly] public float InvresoGrid;
            [ReadOnly] public int3 GridDim;
            [ReadOnly] public NativeArray<float3> QueryPos;
            [ReadOnly] public NativeArray<int2> CellStartEnd;
            [ReadOnly] public NativeArray<float3> SortedPos;
            [ReadOnly] public NativeArray<int2> HashIndex;

            [NativeDisableParallelForRestriction] public NativeArray<int> Results;

            void IJobParallelFor.Execute(int index)
            {
                for (int i = 0; i < MaxNeighbor; i++)
                    Results[index * MaxNeighbor + i] = -1;

                float3 p = QueryPos[index];

                int3 cell = SpaceToGrid(p, OriGrid, InvresoGrid);
                cell = math.clamp(cell, new int3(0, 0, 0), GridDim - new int3(1, 1, 1));

                int idRes = 0;

                //First search for the corresponding cell
                int neighcellhashf = Flatten3DTo1D(cell, GridDim);
                int idStartf = CellStartEnd[neighcellhashf].x;
                int idStopf = CellStartEnd[neighcellhashf].y;


                if (idStartf < int.MaxValue - 1)
                {
                    for (int id = idStartf; id < idStopf; id++)
                    {
                        float3 posA = SortedPos[id];
                        float d = math.distancesq(p, posA); //Squared distance
                        if (d <= SquaredRadius)
                        {
                            Results[index * MaxNeighbor + idRes] = HashIndex[id].y;
                            idRes++;
                            //Found enough close points we can stop there
                            if (idRes == MaxNeighbor)
                            {
                                return;
                            }
                        }
                    }
                }

                for (int x = -CellsToLoop; x <= CellsToLoop; x++)
                {
                    int3 curGridId;
                    curGridId.x = cell.x + x;
                    if (curGridId.x >= 0 && curGridId.x < GridDim.x)
                    {
                        for (int y = -CellsToLoop; y <= CellsToLoop; y++)
                        {
                            curGridId.y = cell.y + y;
                            if (curGridId.y >= 0 && curGridId.y < GridDim.y)
                            {
                                for (int z = -CellsToLoop; z <= CellsToLoop; z++)
                                {
                                    curGridId.z = cell.z + z;
                                    if (curGridId.z >= 0 && curGridId.z < GridDim.z)
                                    {
                                        if (x == 0 && y == 0 && z == 0)
                                            continue; //Already done that


                                        int neighcellhash = Flatten3DTo1D(curGridId, GridDim);
                                        int idStart = CellStartEnd[neighcellhash].x;
                                        int idStop = CellStartEnd[neighcellhash].y;

                                        if (idStart < int.MaxValue - 1)
                                        {
                                            for (int id = idStart; id < idStop; id++)
                                            {
                                                float3 posA = SortedPos[id];
                                                float d = math.distancesq(p, posA); //Squared distance

                                                if (d <= SquaredRadius)
                                                {
                                                    Results[index * MaxNeighbor + idRes] = HashIndex[id].y;
                                                    idRes++;
                                                    //Found enough close points we can stop there
                                                    if (idRes == MaxNeighbor)
                                                    {
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //--------- Fast sort stuff
        [BurstCompile(CompileSynchronously = true)]
        private struct PopulateEntryJob : IJobParallelFor
        {
            [NativeDisableParallelForRestriction] public NativeArray<SortEntry> Entries;
            [ReadOnly] public NativeArray<int2> HashIndex;

            public void Execute(int index)
            {
                Entries[index] = new SortEntry(HashIndex[index]);
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct DePopulateEntryJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<SortEntry> Entries;
            public NativeArray<int2> HashIndex;

            public void Execute(int index)
            {
                HashIndex[index] = Entries[index].Value;
            }
        }

        public struct INT2Comparer : IComparer<int2>
        {
            public int Compare(int2 lhs, int2 rhs)
            {
                return lhs.x.CompareTo(rhs.x);
            }
        }

        private static int3 SpaceToGrid(float3 pos3D, float3 originGrid, float invdx)
        {
            return (int3)((pos3D - originGrid) * invdx);
        }

        private static int Flatten3DTo1D(int3 id3d, int3 gridDim)
        {
            return (id3d.z * gridDim.x * gridDim.y) + (id3d.y * gridDim.x) + id3d.x;
            // return (gridDim.y * gridDim.z * id3d.x) + (gridDim.z * id3d.y) + id3d.z;
        }


        public static class ConcreteJobs
        {
            static ConcreteJobs()
            {
                new MultithreadedSort.Merge<SortEntry>().Schedule();
                new MultithreadedSort.QuicksortJob<SortEntry>().Schedule();
            }
        }

        // This is the item to sort
        public readonly struct SortEntry : IComparable<SortEntry>
        {
            public readonly int2 Value;

            public SortEntry(int2 value)
            {
                Value = value;
            }

            public int CompareTo(SortEntry other)
            {
                return Value.x.CompareTo(other.Value.x);
            }
        }
    }
}