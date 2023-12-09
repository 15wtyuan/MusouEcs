using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace MusouEcs
{
    //Multithreaded sort from https://coffeebraingames.wordpress.com/2020/06/07/a-multithreaded-sorting-attempt/
    public static class MultithreadedSort
    {
        // Use quicksort when sub-array length is less than or equal than this value
        private const int QuicksortThresholdLength = 400;

        public static JobHandle Sort<T>(NativeArray<T> array, JobHandle parentHandle)
            where T : unmanaged, IComparable<T>
        {
            return MergeSort(array, new SortRange(0, array.Length - 1), parentHandle);
        }

        // public static JobHandle Sort<T>(NativeArray<T> array, JobHandle parentHandle)
        // where T : unmanaged, IComparable<T> {
        //     return NativeSortExtension.SortJob(array, parentHandle);
        // }


        private static JobHandle MergeSort<T>(NativeArray<T> array, SortRange range, JobHandle parentHandle)
            where T : unmanaged, IComparable<T>
        {
            if (range.Length <= QuicksortThresholdLength)
            {
                // Use quicksort
                return new QuicksortJob<T>
                {
                    Array = array,
                    Left = range.Left,
                    Right = range.Right
                }.Schedule(parentHandle);
            }

            int middle = range.Middle;

            SortRange left = new SortRange(range.Left, middle);
            JobHandle leftHandle = MergeSort(array, left, parentHandle);

            SortRange right = new SortRange(middle + 1, range.Right);
            JobHandle rightHandle = MergeSort(array, right, parentHandle);

            JobHandle combined = JobHandle.CombineDependencies(leftHandle, rightHandle);

            return new Merge<T>
            {
                Array = array,
                First = left,
                Second = right
            }.Schedule(combined);
        }

        public readonly struct SortRange
        {
            public readonly int Left;
            public readonly int Right;

            public SortRange(int left, int right)
            {
                Left = left;
                Right = right;
            }

            public int Length
            {
                get { return Right - Left + 1; }
            }

            public int Middle
            {
                get
                {
                    return (Left + Right) >> 1; // divide 2
                }
            }

            public int Max
            {
                get { return Right; }
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        public struct Merge<T> : IJob where T : unmanaged, IComparable<T>
        {
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<T> Array;

            public SortRange First;
            public SortRange Second;

            public void Execute()
            {
                int firstIndex = First.Left;
                int secondIndex = Second.Left;
                int resultIndex = First.Left;

                // Copy first
                NativeArray<T> copy = new NativeArray<T>(Second.Right - First.Left + 1, Allocator.Temp);
                for (int i = First.Left; i <= Second.Right; ++i)
                {
                    int copyIndex = i - First.Left;
                    copy[copyIndex] = Array[i];
                }

                while (firstIndex <= First.Max || secondIndex <= Second.Max)
                {
                    if (firstIndex <= First.Max && secondIndex <= Second.Max)
                    {
                        // both subranges still have elements
                        T firstValue = copy[firstIndex - First.Left];
                        T secondValue = copy[secondIndex - First.Left];

                        if (firstValue.CompareTo(secondValue) < 0)
                        {
                            // first value is lesser
                            Array[resultIndex] = firstValue;
                            ++firstIndex;
                            ++resultIndex;
                        }
                        else
                        {
                            Array[resultIndex] = secondValue;
                            ++secondIndex;
                            ++resultIndex;
                        }
                    }
                    else if (firstIndex <= First.Max)
                    {
                        // Only the first range has remaining elements
                        T firstValue = copy[firstIndex - First.Left];
                        Array[resultIndex] = firstValue;
                        ++firstIndex;
                        ++resultIndex;
                    }
                    else if (secondIndex <= Second.Max)
                    {
                        // Only the second range has remaining elements
                        T secondValue = copy[secondIndex - First.Left];
                        Array[resultIndex] = secondValue;
                        ++secondIndex;
                        ++resultIndex;
                    }
                }

                copy.Dispose();
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        public struct QuicksortJob<T> : IJob where T : unmanaged, IComparable<T>
        {
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<T> Array;

            public int Left;
            public int Right;

            public void Execute()
            {
                Quicksort(Left, Right);
            }

            private void Quicksort(int left, int right)
            {
                int i = left;
                int j = right;
                T pivot = Array[(left + right) / 2];

                while (i <= j)
                {
                    // Lesser
                    while (Array[i].CompareTo(pivot) < 0)
                    {
                        ++i;
                    }

                    // Greater
                    while (Array[j].CompareTo(pivot) > 0)
                    {
                        --j;
                    }

                    if (i <= j)
                    {
                        // Swap
                        (Array[i], Array[j]) = (Array[j], Array[i]);

                        ++i;
                        --j;
                    }
                }

                // Recurse
                if (left < j)
                {
                    Quicksort(left, j);
                }

                if (i < right)
                {
                    Quicksort(i, right);
                }
            }
        }
    }
}