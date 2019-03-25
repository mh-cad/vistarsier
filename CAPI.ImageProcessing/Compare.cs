using CAPI.ImageProcessing.Abstraction;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPI.ImageProcessing
{
    public class Compare
    {
        public static INifti CompareMSLesionIncrease(INifti input, INifti reference)
        {
            INifti output = CompareMSLesion(input, reference, 0, 5f);
            output.ColorMap = ColorMaps.RedScale();

            return output;
        }

        public static INifti CompareMSLesionDecrease(INifti input, INifti reference)
        {
            INifti output = CompareMSLesion(input, reference, -5f, 0);
            output.ColorMap = ColorMaps.ReverseGreenScale();

            return output;
        }

        public static INifti CompareMSLesion(INifti input, INifti reference, float lowerBound, float upperBound, float backgroundThreshold = 10)
        {
            INifti output = input.DeepCopy();

            float mean = (float)input.voxels.Where(val => val > backgroundThreshold).Mean();
            float stdDev = (float)input.voxels.Where(val => val > backgroundThreshold).StandardDeviation();
            //float range = input.voxels.Max() - input.voxels.Min();
            // Values from trial and error....
            float minRelevantValue = mean - (2*stdDev); 
            float maxRelevantValue = mean + (3*stdDev);

            lowerBound *= stdDev;
            upperBound *= stdDev;

            if (input.voxels.Length != reference.voxels.Length) throw new Exception("Input and reference don't match size");

            for (int i = 0; i < input.voxels.Length; ++i)
            {
                output.voxels[i] = reference.voxels[i] - input.voxels[i];
                // Set output to relevant min/max bounds (usually used to zero on bound or the other)
                if (output.voxels[i] < lowerBound) output.voxels[i] = lowerBound;
                if (output.voxels[i] > upperBound) output.voxels[i] = upperBound;
                // We want to ignore changes below the minimum relevant value.
                if (input.voxels[i] < minRelevantValue) output.voxels[i] = 0;
                if (reference.voxels[i] < minRelevantValue) output.voxels[i] = 0;
                // And above the maximum relevant value.
                if (input.voxels[i] > maxRelevantValue) output.voxels[i] = 0;
                if (reference.voxels[i] > maxRelevantValue) output.voxels[i] = 0;
                // If we haven't changed by at least 1 stdDev we're not significant
                if (Math.Abs(output.voxels[i]) < Math.Abs(stdDev)) output.voxels[i] = 0;
            }

            output.voxels = output.voxels;

            return output;
        }
    }
}
