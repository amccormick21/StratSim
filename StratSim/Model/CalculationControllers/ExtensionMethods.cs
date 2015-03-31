using System;
using System.Collections.Generic;

namespace StratSim.Model
{
    /// <summary>
    /// Class representing extension methods, predominantly for the list and array classes.
    /// Includes searching and sorting routines.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Sorts a list of type T according to the comparer
        /// </summary>
        /// <typeparam name="T">The type of elements in the list</typeparam>
        /// <param name="comparer">Use a > b for an ascending list.</param>
        public static void Sort<T>(this List<T> List, Func<T, T, bool> comparer)
        {
            T[] Array = List.ToArray();
            Array.Sort<T>(comparer);
        }

        /// <summary>
        /// Sorts an array of type T according to the comparer
        /// </summary>
        /// <typeparam name="T">The type of elements in the array</typeparam>
        /// <param name="comparer">Use a > b for an ascending list.</param>
        public static void Sort<T>(this T[] Array, Func<T, T, bool> comparer)
        {
            int first = 0;
            int last = Array.Length - 1;

            if (last - first >= 4)
            {
                Functions.QuickSort<T>(ref Array, first, last, comparer);
            }
            else
            {
                Functions.InsertionSort<T>(ref Array, first, last, comparer);
            }
        }

        /// <summary>
        /// Checks if the specified element exists in a list.
        /// </summary>
        /// <param name="List"></param>
        /// <param name="itemToFind">The item to find in the list</param>
        /// <returns>True if the value is in the list</returns>
        public static bool Exists(this List<int> List, int itemToFind)
        {
            int Index = 0;

            return List.Exists(itemToFind, false, out Index);
        }

        /// <summary>
        /// Checks if the specified element exists in a list and returns the location of the element in the list.
        /// </summary>
        /// <param name="List"></param>
        /// <param name="itemToFind">The item to find in the list</param>
        /// <param name="isSorted">True if the list is already sorted into ascending order</param>
        /// <param name="Index">Returns the index at which the element is found.
        /// Returns -1 if the value is not in the list.</param>
        /// <returns>True if the specified element can be found in the list.</returns>
        public static bool Exists(this List<int> List, int itemToFind, bool isSorted, out int Index)
        {
            int[] Array = List.ToArray();

            return Array.Exists(itemToFind, isSorted, out Index);
        }

        /// <summary>
        /// Checks if the specified element exists in an array and returns the index of the element in the array.
        /// </summary>
        /// <param name="Array"></param>
        /// <param name="itemToSearch">The item to find in the array</param>
        /// <param name="isSorted">True if the array is already sorted in ascending order</param>
        /// <param name="Index">Returns the index at which the element is found.
        /// Returns -1 if the value is not in the list.</param>
        /// <returns>True if the specified element can be found in the array.</returns>
        public static bool Exists(this int[] Array, int itemToSearch, bool isSorted, out int Index)
        {
            return Functions.StartBinarySearch(ref Array, itemToSearch, isSorted, out Index);
        }
    }
}
