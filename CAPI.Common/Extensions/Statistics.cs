using System;

namespace CAPI.Common.Extensions
{
    public static class Statistics
    {
        public static double Mean(this float[] values)
        {
            return values.Length == 0 ? 0 : values.Mean(0, values.Length);
        }

        public static double Mean(this float[] values, int start, int end)
        {
            double s = 0;

            for (int i = start; i < end; i++)
            {
                s += values[i];
            }

            return s / (end - start);
        }

        public static double Variance(this float[] values)
        {
            return values.Variance(values.Mean(), 0, values.Length);
        }

        public static double Variance(this float[] values, double mean)
        {
            return values.Variance(mean, 0, values.Length);
        }

        public static double Variance(this float[] values, double mean, int start, int end)
        {
            double variance = 0;

            for (int i = start; i < end; i++)
            {
                variance += Math.Pow((values[i] - mean), 2);
            }

            int n = end - start;
            if (start > 0) n -= 1;

            return variance / (n);
        }

        public static double StandardDeviation(this float[] values)
        {
            return values.Length == 0 ? 0 : values.StandardDeviation(0, values.Length);
        }

        public static double StandardDeviation(this float[] values, int start, int end)
        {
            double mean = values.Mean(start, end);
            double variance = values.Variance(mean, start, end);

            return Math.Sqrt(variance);
        }

        public static float[] Normalize(this float[] array, int mean, int stdDev)
        {
            var currMean = array.Mean();
            var currStdDev = array.StandardDeviation();

            for (var i = 0; i < array.Length; ++i)
                array[i] = Math.Abs(currStdDev) < 0.000001 ? 0 :
                    (float)((array[i] - currMean) / currStdDev) * stdDev + mean;

            return array;
        }

        public static float[] Trim(this float[] array, int lower, int upper)
        {
            for (var i = 0; i < array.Length; i++)
                if (array[i] < lower) array[i] = lower;
                else if (array[i] > upper) array[i] = upper;

            return array;
        }

        public static float[,] Normalize(this float[,] array, int mean, int stdDev)
        {
            var flatArray = new float[array.Length];
            for (var x = 0; x < array.GetLength(0); x++)
                for (var y = 0; y < array.GetLength(1); x++)
                    flatArray[x + y * array.GetLength(0)] = array[x, y];

            flatArray.Normalize(mean, stdDev);

            for (var x = 0; x < array.GetLength(0); x++)
                for (var y = 0; y < array.GetLength(1); x++)
                    array[x, y] = flatArray[x + y * array.GetLength(0)];

            return array;
        }
    }
}