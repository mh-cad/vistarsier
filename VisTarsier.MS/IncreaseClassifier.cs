using System;
using VisTarsier.NiftiLib;
using VisTarsier.NiftiLib.Processing;

namespace VisTarsier.Module.MS
{
    class CapiClassifier : IClassifier<float>
    {
        public INifti<float> Classify(INifti<float> prior, INifti<float> current)
        {
            var output = MSCompare.CompareMSLesionIncrease(prior, current);
            return output;
        }

        public INifti<float> Classify(INifti<float>[] inputs)
        {
            if (inputs.Length != 2) throw new ArgumentException("This classifier needs a prior and current INifti");

            var prior = inputs[0];
            var current = inputs[1];

            return Classify(prior, current);
        }
    }
}
