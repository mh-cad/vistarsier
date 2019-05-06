using CAPI.NiftiLib;
using System;
using System.Collections.Generic;

namespace CAPI.ImageProcessing.Abstraction
{
    public interface IImageProcessor
    {
        Metrics MSLesionCompare(
            string currentNii, string priorNii, string referenceNii,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string[] resultNiis, string outPriorReslicedNii);
    }
}