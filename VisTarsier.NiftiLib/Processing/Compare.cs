using MathNet.Numerics.Statistics;
using System;
using System.Linq;

namespace VisTarsier.NiftiLib.Processing
{
    public class Compare
    {
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
        public static INifti<float> GatedSubract(INifti<float> input, INifti<float> reference, float backgroundThreshold = 10, float minRelevantStd = -1, float maxRelevantStd = 5, float minChange = 0.8f, float maxChange = 5)
        {
            INifti<float> output = input.DeepCopy();

            //var mean = (float)input.Voxels.Where(val => val > backgroundThreshold).MeanStandardDeviation();
            var meanstddev = input.Voxels.Where(val => val > backgroundThreshold).MeanStandardDeviation();
            var mean = meanstddev.Item1;
            var stdDev = meanstddev.Item2; //(Not sure why decompose stopped working here).
            //float range = input.voxels.Max() - input.voxels.Min();
            // Values from trial and error....
            float minRelevantValue = (float)(mean + (minRelevantStd * stdDev));
            float maxRelevantValue = (float)(mean + (maxRelevantStd * stdDev));

            if (input.Voxels.Length != reference.Voxels.Length) throw new Exception("Input and reference don't match size");

            for (int i = 0; i < input.Voxels.Length; ++i)
            {
                output.Voxels[i] = input.Voxels[i] - reference.Voxels[i];


                // We want to ignore changes below the minimum relevant value.
                if (input.Voxels[i] < minRelevantValue) output.Voxels[i] = 0;
                if (reference.Voxels[i] < minRelevantValue) output.Voxels[i] = 0;

                // And above the maximum relevant value.
                if (input.Voxels[i] > maxRelevantValue) output.Voxels[i] = 0;
                if (reference.Voxels[i] > maxRelevantValue) output.Voxels[i] = 0;

                // If we haven't changed by at least 1 stdDev we're not significant
                if (Math.Abs(output.Voxels[i]) < Math.Abs(minChange * stdDev)) output.Voxels[i] = 0;
                if (Math.Abs(output.Voxels[i]) > Math.Abs(maxChange * stdDev)) output.Voxels[i] = 0;
                if (reference.Voxels[i] < backgroundThreshold) output.Voxels[i] = 0;
                if (input.Voxels[i] < backgroundThreshold) output.Voxels[i] = 0;
            }

            for (int i = 1; i < output.Voxels.Length - 1; ++i)
            {
                if (output.Voxels[i - 1] == 0 && output.Voxels[i + 1] == 0) output.Voxels[i] = 0;
            }

            output.RecalcHeaderMinMax(); // Update header range.

            var stdDv = output.Voxels.StandardDeviation();
            var mean2 = output.Voxels.Where(val => val > 0).Mean();
            System.Console.WriteLine($"Compared. Mean={mean2}, stdDv={stdDv}, size={output.Voxels.Where(val => val > 0).Count()}");

            return output;
        }
    }
}
