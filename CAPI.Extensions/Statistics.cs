using System;
using System.Linq;

// ReSharper disable MemberCanBePrivate.Global

namespace CAPI.Extensions
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

            for (var i = start; i < end; i++)
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

            for (var i = start; i < end; i++)
            {
                variance += Math.Pow((values[i] - mean), 2);
            }

            var n = end - start;
            if (start > 0) n -= 1;

            return variance / (n);
        }

        public static double StandardDeviation(this float[] values)
        {
            return values.Length == 0 ? 0 : values.StandardDeviation(0, values.Length);
        }

        public static double StandardDeviation(this float[] values, int start, int end)
        {
            var mean = values.Mean(start, end);
            var variance = values.Variance(mean, start, end);

            return Math.Sqrt(variance);
        }

        public static void Normalize(this float[] array, int mean, int stdDev)
        {
            var currentMean = array.Mean();
            var currentStdDev = array.StandardDeviation();

            if (Math.Abs(currentStdDev) < 0.000001) return;

            for (var i = 0; i < array.Length; i++)
                array[i] = (float)((array[i] - currentMean) / currentStdDev) * stdDev + mean;
        }

        public static void Normalize(this float[] array, int mean, int stdDev, bool[] mask)
        {
            var currentMean = array
                .Select((v, i) => new { v, i })
                .Where(x => mask[x.i] && x.v > 0)
                .Select(x => x.v).ToArray()
                .Mean();
            var currentStdDev = array
                .Select((v, i) => new { v, i })
                .Where(x => mask[x.i] && x.v > 0)
                .Select(x => x.v).ToArray()
                .StandardDeviation();

            if (Math.Abs(currentStdDev) < 0.000001) return;

            for (var i = 0; i < array.Length; i++)
                if (mask[i] && array[i] > 0)
                    array[i] = (float)((array[i] - currentMean) / currentStdDev) * stdDev + mean;
        }

        //public static void Normalize(this float[] array, int mean, int stdDev, int min, int max)
        //{
        //    var currentMean = array.Where(i => i > min && i < max).ToArray().Mean();
        //    var currentStdDev = array.Where(i => i > min && i < max).ToArray().StandardDeviation();

        //    for (var i = 0; i < array.Length; ++i)
        //        if (array[i] >= min && array[i] <= max)
        //            array[i] = Math.Abs(currentStdDev) < 0.000001 ? 0 :
        //                (float)((array[i] - currentMean) / currentStdDev * stdDev + mean);
        //}

        public static void Trim(this float[] array, int lower, int upper)
        {
            for (var i = 0; i < array.Length; i++)
                if (array[i] < lower) array[i] = lower;
                else if (array[i] > upper) array[i] = upper;
        }

        //public static float[,] Normalize(this float[,] array, int mean, int stdDev)
        //{
        //    var flatArray = new float[array.Length];
        //    for (var x = 0; x < array.GetLength(0); x++)
        //        for (var y = 0; y < array.GetLength(1); x++)
        //            flatArray[x + y * array.GetLength(0)] = array[x, y];

        //    flatArray.Normalize(mean, stdDev);

        //    for (var x = 0; x < array.GetLength(0); x++)
        //        for (var y = 0; y < array.GetLength(1); x++)
        //            array[x, y] = flatArray[x + y * array.GetLength(0)];

        //    return array;
        //}
    }
}