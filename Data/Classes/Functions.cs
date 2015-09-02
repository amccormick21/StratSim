using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DataSources
{
    /// <summary>
    /// Contains methods and functions relevant to Strategy Simulation and F1
    /// </summary>
    public static class Functions
    {
        /// <summary>
        /// Sorts an array of type T using the quicksort algorithm
        /// </summary>
        /// <typeparam name="T">The type of elements in the array to be sorted</typeparam>
        /// <param name="Array">An array of elements to be sorted</param>
        /// <param name="first">The location at which to commence the sort</param>
        /// <param name="last">The location at which to end the sort</param>
        /// <param name="Comparer">A comparing function for two types in the array.
        /// a > b produces an ascending sort.
        /// The comparer should not include the 'equal to' statement.</param>
        public static void QuickSort<T>(ref T[] Array, int first, int last, Func<T, T, bool> Comparer)
        {
            //Validates the list to more than one item
            if (first < last)
            {
                T pivotValue;
                int leftPointer, rightPointer, pivot;
                T temp;

                leftPointer = first + 1;
                rightPointer = last;
                pivot = first;
                pivotValue = Array[pivot];


                //Commences sort
                while (leftPointer <= rightPointer)
                {
                    //scan to the left of the pivot
                    while ((leftPointer <= rightPointer) && !Comparer(Array[leftPointer], pivotValue))
                    {
                        leftPointer++;
                    }
                    //scan to the right of the pivot
                    while ((leftPointer <= rightPointer) && Comparer(Array[rightPointer], pivotValue))
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
                QuickSort<T>(ref Array, first, rightPointer - 1, Comparer);
                QuickSort<T>(ref Array, rightPointer + 1, last, Comparer);
            }
        }

        /// <summary>
        /// Sorts an array of type T using the insertion sort algorithm
        /// </summary>
        /// <typeparam name="T">The type of elements in the array to be sorted</typeparam>
        /// <param name="array">An array of elements to be sorted</param>
        /// <param name="first">The location at which to commence the sort</param>
        /// <param name="last">The location at which to end the sort</param>
        /// <param name="Comparer">A comparing function for two types in the array.
        /// a > b produces an ascending sort.
        /// The comparer should not include the 'equal to' statement.</param>
        public static void InsertionSort<T>(ref T[] array, int first, int last, Func<T, T, bool> comparer)
        {
            int pointer;
            T currentValue;

            for (int arrayPosition = first + 1; arrayPosition <= last; ++arrayPosition)
            {
                currentValue = array[arrayPosition];
                pointer = arrayPosition;
                while ((--pointer >= 0) && !comparer(currentValue, array[pointer]))
                {
                    array[pointer + 1] = array[pointer];
                }
                array[pointer + 1] = currentValue;
            }
        }

        /// <summary>
        /// Initiates a binary search of an array for the specified value
        /// </summary>
        /// <param name="Array">The array in which to find the value</param>
        /// <param name="itemToFind">The target item to find</param>
        /// <param name="isSorted">Represents whether the array is already sorted or not</param>
        /// <param name="Index">Outputs the index at which the required item was found.
        /// Outputs -1 if item is not found.</param>
        /// <returns>True if the item is anywhere in the array</returns>
        public static bool StartBinarySearch(ref int[] Array, int itemToFind, bool isSorted, out int Index)
        {
            Index = 0;
            int first = 0;
            int last = Array.Length - 1;

            if (!isSorted) { Array.Sort<int>((a, b) => a > b); }

            return BinarySearch(ref Array, itemToFind, first, last, out Index);
        }

        /// <summary>
        /// Searches for a specified element in an array using a binary search algorithm.
        /// </summary>
        /// <param name="Array">The array to search</param>
        /// <param name="itemToFind">The item to find in the array</param>
        /// <param name="first">The first index to search</param>
        /// <param name="last">The last index to search</param>
        /// <param name="exitLoop">Represents whether the whole array has been searched. This is true if: '/n'
        /// The value is confirmed not in the array, or '/n'
        /// The value is found</param>
        /// <param name="Index">Outputs the index at which the specified item was found.</param>
        /// <returns>True if the item is in the list.</returns>
        static bool BinarySearch(ref int[] Array, int itemToFind, int first, int last, out int Index)
        {
            Index = -1;
            int topIndex = last;
            int bottomIndex = first;
            int middleIndex = 0;
            bool itemFound = false;

            if (topIndex >= bottomIndex)
            {
                middleIndex = (topIndex - bottomIndex) / 2 + bottomIndex;

                if (Array[middleIndex] == itemToFind)
                {
                    itemFound = true;
                    Index = middleIndex;
                }
                else
                {
                    if (topIndex > bottomIndex)
                    {
                        if (Array[middleIndex] > itemToFind)
                        {
                            itemFound = BinarySearch(ref Array, itemToFind, bottomIndex, middleIndex - 1, out Index);
                        }
                        else
                        {
                            itemFound = BinarySearch(ref Array, itemToFind, middleIndex + 1, topIndex, out Index);
                        }
                    }
                }
            }
            return itemFound;
        }

        /* The following routines are validation routines
         * They will return the value that is sent to them for quick assignment.
         * They require a bound, an error message, and a boolean that is updated if the value is false
         * These statements can be stacked so that the incorrect value is set to true if at least one value is incorrect.
         */
        public static float ValidateNotEqualTo(float value, float notEqualTo, string field, ref bool incorrectValue, bool showDialog)
        {
            //assign to incorrect value
            if (!incorrectValue)
            {
                incorrectValue = (value == notEqualTo);

                //select on boolean for speed of execution
                if (incorrectValue && showDialog)
                {
                    //display a warning message
                    string warningMessage = "The field " + field + " cannot be set to " + Convert.ToString(notEqualTo);
                    var warning = MessageBox.Show(warningMessage, "Incorrect Value", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            return value;
        }
        public static float ValidateLessThan(float value, float notGreaterThan, string field, ref bool incorrectValue, bool showDialog)
        {
            ValidateNotEqualTo(value, notGreaterThan, field, ref incorrectValue, showDialog);
            ValidateLessThanEqualTo(value, notGreaterThan, field, ref incorrectValue, showDialog);
            return value;
        }
        public static float ValidateGreaterThan(float value, float notLessThan, string field, ref bool incorrectValue, bool showDialog)
        {
            ValidateNotEqualTo(value, notLessThan, field, ref incorrectValue, showDialog);
            ValidateGreaterThanEqualTo(value, notLessThan, field, ref incorrectValue, showDialog);
            return value;
        }
        public static float ValidateGreaterThanEqualTo(float value, float notLessThan, string field, ref bool incorrectValue, bool showDialog)
        {
            if (!incorrectValue)
            {
                //assign to incorrect value
                incorrectValue = (value < notLessThan);

                //switch on boolean for speed of execution
                if (incorrectValue && showDialog)
                {
                    //display a warning message
                    string warningMessage = "The field " + field + " cannot be less than " + Convert.ToString(notLessThan);
                    var warning = MessageBox.Show(warningMessage, "Incorrect Value", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            return value;
        }
        public static float ValidateLessThanEqualTo(float value, float notGreaterThan, string field, ref bool incorrectValue, bool showDialog)
        {
            if (!incorrectValue)
            {
                //assign to incorrect value
                incorrectValue = (value > notGreaterThan);

                //switch on boolean for speed of execution
                if (incorrectValue && showDialog)
                {
                    //display a warning message
                    string warningMessage = "The field " + field + " cannot be greater than " + Convert.ToString(notGreaterThan);
                    var warning = MessageBox.Show(warningMessage, "Incorrect Value", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            //return the value
            return value;
        }
        public static float ValidateBetweenExclusive(float value, float lowerBound, float upperBound, string field, ref bool incorrectValue, bool showDialog)
        {
            value = ValidateLessThan(value, upperBound, field, ref incorrectValue, showDialog);
            value = ValidateGreaterThan(value, lowerBound, field, ref incorrectValue, showDialog);

            return value;
        }
        public static float ValidateBetweenInclusive(float value, float lowerBound, float upperBound, string field, ref bool incorrectValue, bool showDialog)
        {
            value = ValidateLessThanEqualTo(value, upperBound, field, ref incorrectValue, showDialog);
            value = ValidateGreaterThanEqualTo(value, lowerBound, field, ref incorrectValue, showDialog);

            return value;
        }

        /// <summary>
        /// Opens a dialog box with the specified message and caption.
        /// </summary>
        /// <param name="message">The message to be displayed to the user, identifying the error and 
        /// providing information on how to fix it.</param>
        /// <param name="caption">The message to be displayed in the header of the page</param>
        /// <returns>True if the 'yes' button is clicked, else false</returns>
        public static bool StartDialog(string message, string caption)
        {
            bool valueToReturn = false;

            var buttons = MessageBoxButtons.YesNo;
            var icon = MessageBoxIcon.Warning;
            var defaultButton = MessageBoxDefaultButton.Button2;

            var dialog = MessageBox.Show(message, caption, buttons, icon, defaultButton);

            switch (dialog)
            {
                case DialogResult.Yes:
                    valueToReturn = true;
                    break;
                case DialogResult.No:
                    valueToReturn = false;
                    break;
                default: valueToReturn = false; break;
            }

            return valueToReturn;
        }

        /// <summary>
        /// Initialises a 2D array with every element at the specified value
        /// </summary>
        /// <typeparam name="T">he type of the array</typeparam>
        /// <param name="columns">The width of the array</param>
        /// <param name="rows">The length of the array</param>
        /// <param name="valueToSet">The value to set all elements to</param>
        /// <returns>The populated array of values</returns>
        public static T[,] InitialiseArrayAtValue<T>(int columns, int rows, T valueToSet)
        {
            T[,] newArray = new T[columns, rows];

            for (int column = 0; column < columns; column++)
            {
                for (int row = 0; row < rows; row++)
                {
                    newArray[column, row] = valueToSet;
                }
            }

            return newArray;
        }

        /// <summary>
        /// Gets the sum of the squares of the errors in vertical placement for an array of points
        /// </summary>
        /// <param name="points">an [x, 2] array of (x,y) coordinate pairs</param>
        /// <returns></returns>
        static float ErrorSquared(float[,] points, float gradient, float intercept)
        {
            float total = 0;
            float pointX, pointY, delta;
            for (int i = 0; i < points.Length / 2; i++)
            {
                pointX = points[i, 0];
                pointY = points[i, 1];
                delta = pointY - (gradient * pointX + intercept);
                total += (delta * delta);
            }
            return total;
        }

        // Find the least squares linear fit.
        // Return the total error.
        public static float LinearLeastSquaresFit(float[,] points, out float gradient, out float intercept)
        {
            // Perform the calculation.
            // Find the values S1, Sx, Sy, Sxx, and Sxy.
            float S1 = points.Length / 2;
            float Sx = 0;
            float Sy = 0;
            float Sxx = 0;
            float Sxy = 0;
            for (int i = 0; i < points.Length / 2; i++)
            {
                Sx += points[i, 0];
                Sy += points[i, 1];
                Sxx += points[i, 0] * points[i, 0];
                Sxy += points[i, 0] * points[i, 1];
            }

            // Solve for m and b.
            gradient = (Sxy * S1 - Sx * Sy) / (Sxx * S1 - Sx * Sx);
            intercept = (Sxy * Sx - Sy * Sxx) / (Sx * Sx - S1 * Sxx);

            return (float)Math.Sqrt(ErrorSquared(points, gradient, intercept));
        }
    }

}
