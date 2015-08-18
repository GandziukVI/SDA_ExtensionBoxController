using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox
{
    public enum PolarityEnum
    {
        Polarity_Bipolar,
        Polarity_Unipolar
    }

    public enum RangesEnum
    {
        Range_10,
        Range_5,
        Range_2_5,
        Range_1_25
    }
    public struct ChannelNamesEnum
    {
        public const string AIN1="AIn1";
        public const string AIN2 = "AIn2";
        public const string AIN3 = "AIn3";
        public const string AIN4 = "AIn4";

        public const string AOUT1 = "AOut1";
        public const string AOUT2 = "AOut2";

        public const string DIOA = "DIOA";
        public const string DIOB = "DIOB";
        public const string DIOC = "DIOC";
        public const string DIOD = "DIOD";
    }

    public struct AvailableRanges
    {
        public const double Range_10 = 10.0;
        public const double Range_5 = 5.0;
        public const double Range_2_5 = 2.5;
        public const double Range_1_25 = 1.25;
        public static double FromRangeEnum(RangesEnum range)
        {
            switch (range)
            {
                case RangesEnum.Range_5:
                    return Range_5;
                case RangesEnum.Range_2_5:
                    return Range_2_5;
                case RangesEnum.Range_1_25:
                    return Range_1_25;
                case RangesEnum.Range_10:
                default:
                    return Range_10;
            }
        }
    }
    static class DefinitionsAndConstants
    {
        
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
