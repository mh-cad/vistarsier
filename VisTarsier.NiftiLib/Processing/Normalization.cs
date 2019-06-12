using MathNet.Numerics.Statistics;
using System;
using System.Linq;

namespace VisTarsier.NiftiLib.Processing
{
    public static class Normalization
    {
        /// <summary>
        /// This method converts the input values to their Z-Scores, which are then multiplied by the reference standard deviation and added to the referenced mean. 
        /// </summary>
        /// <param name="input">Input nifti value</param>
        /// <param name="reference">Reference nifti value</param>
        /// <param name="backgroundThreshold">Any values below this threashold will be ignored when calculating the mean and standard deviation.</param>
        /// <returns>Input nifti, now normalised to the reference nifti distribution.</returns>
        public static INifti<float> ZNormalize(INifti<float> input, INifti<float> reference, float backgroundThreshold = 10)
        {
            dynamic output = input.DeepCopy();

            // We take the mean and standard deviation ignoring background.
            var currentMean = input.Voxels.Where(val => val > backgroundThreshold).Mean();
            var currentStdDev = input.Voxels.Where(val => val > backgroundThreshold).StandardDeviation();
            var mean = (float)reference.Voxels.Where(val => val > backgroundThreshold).Mean();
            var stdDev = (float)reference.Voxels.Where(val => val > backgroundThreshold).StandardDeviation();

            if (Math.Abs(currentStdDev) < 0.000001) return output;

            for (var i = 0; i < output.Voxels.Length; i++)
            {
                output.Voxels[i] = (float)((output.Voxels[i] - currentMean) / currentStdDev) * stdDev + mean;
            }

            output.RecalcHeaderMinMax(); //update display range

            return output;
        }

        /// <summary>
        /// Normalizes the data to a Z-Value
        /// </summary>
        /// <param name="input"></param>
        /// <param name="backgroundThreshold"></param>
        /// <returns></returns>
        public static INifti<float> ZNormalize(INifti<float> input, float backgroundThreshold = 10)
        {
            dynamic output = input.DeepCopy();
            // We take the mean and standard deviation ignoring background.
            var currentMean = input.Voxels.Where(val => val > backgroundThreshold).Mean();
            var currentStdDev = input.Voxels.Where(val => val > backgroundThreshold).StandardDeviation();

            for (var i = 0; i < output.Voxels.Length; i++)
            {
                output.Voxels[i] = (float)((output.Voxels[i] - currentMean) / currentStdDev);
            }

            output.RecalcHeaderMinMax(); //update display range

            return output;
        }

        /// <summary>
        /// Shifts the ditribution to be within the given range. Default is 0-1.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="rangeStart"></param>
        /// <param name="rangeEnd"></param>
        /// <returns></returns>
        public static INifti<float> RangeNormalize(INifti<float> input, float rangeStart = 0, float rangeEnd = 1)
        {
            if (rangeEnd <= rangeStart) throw new ArgumentException("Start of range cannot be greater than end of range.");

            var min = input.Voxels.Min();
            var range = input.Voxels.Max() - input.Voxels.Min();

            var output = input.DeepCopy();

            for (int i = 0; i < output.Voxels.Length; ++i)
            {
                output.Voxels[i] = ((output.Voxels[i] - min) / range) * (rangeEnd - rangeStart) + rangeStart;
            }

            return output;
        }
    }
}
