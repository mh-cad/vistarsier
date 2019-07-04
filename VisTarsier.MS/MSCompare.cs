using VisTarsier.NiftiLib;
using VisTarsier.NiftiLib.Processing;

namespace VisTarsier.Module.MS
{
    public static class MSCompare
    {
        /// <summary>
        /// Compares the increase in values from the reference Nifti (prior) to the input Nifti (current).
        /// The inputs should be pre-registered and normalised.
        /// </summary>
        /// <param name="input">Current example</param>
        /// <param name="reference">Prior example</param>
        /// <returns>Nifti who's values are the meaningful increase between prior and current.</returns>
        public static INifti<float> CompareMSLesionIncrease(INifti<float> input, INifti<float> reference)
        {
            INifti<float> output = Compare.GatedSubract(input, reference, backgroundThreshold:10, minRelevantStd:-1, maxRelevantStd:5, minChange:0.8f, maxChange:5);
            for (int i = 0; i < output.Voxels.Length; ++i) if (output.Voxels[i] < 0) output.Voxels[i] = 0;
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
        public static INifti<float> CompareMSLesionDecrease(INifti<float> input, INifti<float> reference)
        {
            INifti<float> output = Compare.GatedSubract(input, reference, backgroundThreshold: 10, minRelevantStd: -1, maxRelevantStd: 5, minChange: 0.8f, maxChange: 5);
            for (int i = 0; i < output.Voxels.Length; ++i) if (output.Voxels[i] > 0) output.Voxels[i] = 0;
            output.RecalcHeaderMinMax(); // This will update the header range.
            output.ColorMap = ColorMaps.ReverseGreenScale();

            return output;
        }
    }
}
