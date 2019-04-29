using MathNet.Numerics.Statistics;
using System;
using System.Linq;

namespace CAPI.NiftiLib.Processing
{
    public static class Compare
    {
        /// <summary>
        /// Compares the increase in values from the reference Nifti (prior) to the input Nifti (current).
        /// The inputs should be pre-registered and normalised.
        /// </summary>
        /// <param name="input">Current example</param>
        /// <param name="reference">Prior example</param>
        /// <returns>Nifti who's values are the meaningful increase between prior and current.</returns>
        public static INifti CompareMSLesionIncrease(INifti input, INifti reference)
        {
            INifti output = CompareMSLesion(input, reference);
            for (int i = 0; i < output.voxels.Length; ++i) if (output.voxels[i] < 0) output.voxels[i] = 0;
            output.RecalcHeaderMinMax(); // This will update the header range.
            output.ColorMap = ColorMaps.RedScale();

            return output;
        }

        /// <summary>
        /// Compares the decrease in values from the reference Nifti (prior) to the input Nifti (current).
        /// The inputs should be pre-registered and normalised.
        /// </summary>
        /// <param name="input">Current example</param>
        /// <param name="reference">Prior example</param>
        /// <returns>Nifti who's values are the meaningful decrease (less than 0) between prior and current.</returns>
        public static INifti CompareMSLesionDecrease(INifti input, INifti reference)
        {
            INifti output = CompareMSLesion(input, reference);
            for (int i = 0; i < output.voxels.Length; ++i) if (output.voxels[i] > 0) output.voxels[i] = 0;
            output.RecalcHeaderMinMax(); // This will update the header range.
            output.ColorMap = ColorMaps.ReverseGreenScale();

            return output;
        }

        /// <summary>
        /// Compares the meaningful change in value between the reference Nifti (prior) and the input Nifti (current).
        /// This function also allows significant fine-tuning of the cut-off values.
        /// </summary>
        /// <param name="input">Current Nifti</param>
        /// <param name="reference">Prior Nifti</param>
        /// <param name="backgroundThreshold">Absolute value of background threashold. Any voxels with a value less than this are considered background and ignored.</param>
        /// <param name="minRelevantStd">Minimum relevant value in number of standard deviations from the mean. e.g. a value of -1 will mean that the minimum relevant value will be the mean - 1 standard deviation. Voxels below this threashold are ignored.</param>
        /// <param name="maxRelevantStd">Maximum relevant value in number of standard deviations from the mean. e.g. a value of 3 will mean that the maximum relevant value will be the mean + 3 standard deviations. Voxels above this threashold are ignored.</param>
        /// <param name="minChange">Minimum difference to be considered significant (e.g. noise threshold). Value is given in multiples of the standard deviation for the input voxels (ignoring background).</param>
        /// <param name="maxChange">Maximum difference to be considered significant. Value is given in multiples of the standard deviation for the input voxels (ignoring background).</param>
        /// <returns>INifti object which contains the relevant difference between the reference nifti and the input nifti.</returns>
        public static INifti CompareMSLesion(INifti input, INifti reference, float backgroundThreshold = 10, float minRelevantStd = -1, float maxRelevantStd = 5, float minChange = 0.8f, float maxChange = 5)
        {
            INifti output = input.DeepCopy();

            float mean = (float)input.voxels.Where(val => val > backgroundThreshold).Mean();
            float stdDev = (float)input.voxels.Where(val => val > backgroundThreshold).StandardDeviation();
            //float range = input.voxels.Max() - input.voxels.Min();
            // Values from trial and error....
            float minRelevantValue = mean + (minRelevantStd * stdDev); 
            float maxRelevantValue = mean + (maxRelevantStd * stdDev);

            if (input.voxels.Length != reference.voxels.Length) throw new Exception("Input and reference don't match size");

            for (int i = 0; i < input.voxels.Length; ++i)
            {
                output.voxels[i] = input.voxels[i] - reference.voxels[i];
                // We want to ignore changes below the minimum relevant value.
                if (input.voxels[i] < minRelevantValue) output.voxels[i] = 0;
                if (reference.voxels[i] < minRelevantValue) output.voxels[i] = 0;
                // And above the maximum relevant value.
                if (input.voxels[i] > maxRelevantValue) output.voxels[i] = 0;
                if (reference.voxels[i] > maxRelevantValue) output.voxels[i] = 0;
                // If we haven't changed by at least 1 stdDev we're not significant
                if (Math.Abs(output.voxels[i]) < Math.Abs(minChange * stdDev)) output.voxels[i] = 0;
                if (Math.Abs(output.voxels[i]) > Math.Abs(maxChange * stdDev)) output.voxels[i] = 0;
                if (reference.voxels[i] < backgroundThreshold) output.voxels[i] = 0;
                if (input.voxels[i] < backgroundThreshold) output.voxels[i] = 0;
            }

            for (int i = 1; i < output.voxels.Length-1; ++i)
            {
                if (output.voxels[i - 1] == 0 && output.voxels[i + 1] == 0) output.voxels[i] = 0;
                else
                {
                   // Console.WriteLine("Got one!");
                }
            }

            output.RecalcHeaderMinMax(); // Update header range.

            var stdDv = output.voxels.StandardDeviation();
            var mean2 = output.voxels.Where(val => val > 0).Mean();
            System.Console.WriteLine($@"Compared. Mean={mean2}, stdDv={stdDv}, size={output.voxels.Where(val => val > 0).Count()}");

            return output;
        }
    }
}
