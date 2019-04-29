using MathNet.Numerics.Statistics;
using System;
using System.Linq;

namespace CAPI.NiftiLib.Processing
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
        public static INifti ZNormalize(INifti input, INifti reference, float backgroundThreshold = 10)
        {
            INifti output = input.DeepCopy();

            // We take the mean and standard deviation ignoring background.
            var currentMean = input.voxels.Where(val => val > backgroundThreshold).Mean();
            var currentStdDev = input.voxels.Where(val => val > backgroundThreshold).StandardDeviation();
            var mean = (float)reference.voxels.Where(val => val > backgroundThreshold).Mean();
            var stdDev = (float)reference.voxels.Where(val => val > backgroundThreshold).StandardDeviation();

            if (Math.Abs(currentStdDev) < 0.000001) return output;

            for (var i = 0; i < output.voxels.Length; i++)
            {
                output.voxels[i] = (float)((output.voxels[i] - currentMean) / currentStdDev) * stdDev + mean;
            }

            output.RecalcHeaderMinMax(); //update display range

            return output;
        }
    }
}
