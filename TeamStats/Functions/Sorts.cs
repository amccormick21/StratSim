using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamStats.Statistics;

namespace TeamStats.Functions
{
    public static class Sorts
    {
        /// <summary>
        /// <para>Sorts an array of statistic elements using their specified comparers.</para>
        /// <para>Uses the quicksort algorithm</para>
        /// </summary>
        /// <param name="Array">The array of elements to be sorted</param>
        /// <param name="first">The index of the first element in the list to be sorted</param>
        /// <param name="last">The number of elements in the list to be sorted</param>
        /// <param name="orderType">Specifies whether the sort is ascending or descending</param>
        /// <param name="sortType">Enum representing what the data is to be sorted by</param>
        public static void QuickSort(ref IStatisticElement[] Array, int first, int last, OrderType orderType, SortType sortType)
        {
            //Validates the list to more than one item
            if (first < last)
            {
                IStatisticElement pivotValue;
                int leftPointer, rightPointer, pivot;
                IStatisticElement temp;

                leftPointer = first + 1;
                rightPointer = last;
                pivot = first;
                pivotValue = Array[pivot];

                //Commences sort
                while (leftPointer <= rightPointer)
                {
                    //scan to the left of the pivot
                    while ((leftPointer <= rightPointer) && Array[leftPointer].CompareTo(pivotValue, sortType, orderType) < 0)
                    {
                        leftPointer++;
                    }
                    //scan to the right of the pivot
                    while ((leftPointer <= rightPointer) && Array[rightPointer].CompareTo(pivotValue, sortType, orderType) >= 0)
                    {
                        rightPointer--;
                    }

                    //swap if two values need to be swapped.
                    if (leftPointer < rightPointer)
                    {
                        temp = Array[leftPointer];
                        Array[leftPointer] = Array[rightPointer];
                        Array[rightPointer] = temp;
                    }
                }

                temp = Array[first];
                Array[first] = Array[rightPointer];
                Array[rightPointer] = temp;

                //Quicksort the remaining values
                QuickSort(ref Array, first, rightPointer - 1, orderType, sortType);
                QuickSort(ref Array, rightPointer + 1, last, orderType, sortType);
            }
        }
    }
}
