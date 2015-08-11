using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox
{
    static class DefinitionsAndConstants
    {
        public static readonly double[] _AvailableRanges = new double[] { 10.0, 5.0, 2.5, 1.25 };
        public static readonly int[] _AvailableGains = new int[] { 1, 10, 100 };
        public static readonly int[] _AvailableCutOffFrequencies = new int[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150 };
        public static readonly byte[] _bitmask = new byte[] { 1, 2, 4, 8 };
        public static readonly int[] _FilterGain_Bits = new int[] { 4, 5, 6, 7 };
        public static readonly int[] _Frequency_Bits = new int[] { 0, 1, 2, 3 };

        public static double _GetClosestValueInArray(double[] arr, double val)
        {
            Array.Sort(arr);

            var closest = arr[arr.Length - 1];
            var minDifference = double.MaxValue;

            foreach (var element in arr)
            {
                var difference = Math.Abs(element - val);
                if (minDifference > difference)
                {
                    minDifference = difference;
                    closest = element;
                }
            }

            return closest;
        }

        public static int _GetClosestValueInArray(int[] arr, int val)
        {
            Array.Sort(arr);

            var closest = arr[arr.Length - 1];
            var minDifference = int.MaxValue;

            foreach (var element in arr)
            {
                var difference = Math.Abs(element - val);
                if (minDifference > difference)
                {
                    minDifference = difference;
                    closest = element;
                }
            }

            return closest;
        }
    }
}
