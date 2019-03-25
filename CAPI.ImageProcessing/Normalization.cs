using CAPI.ImageProcessing.Abstraction;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPI.ImageProcessing
{
    public delegate INifti NormalizeDelegate(INifti input, INifti reference, float backgroundThreshold);

    public class Normalization
    {
        public static INifti Normalize(INifti input, INifti reference, float backgroundThreshold = 10)
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

            output.voxels = output.voxels; //update display range

            return output;
        }
    }
}
