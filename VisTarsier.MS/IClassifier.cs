using VisTarsier.NiftiLib;

namespace VisTarsier.Module.MS
{
    interface IClassifier<T>
    {
        /// <summary>
        /// The classifier will produce a mask/labelset based on one or more inputs.
        /// </summary>
        /// <param name="inputs">Input niftis</param>
        /// <returns>The output nifti</returns>
        INifti<T> Classify(INifti<T>[] inputs);
    }
}
