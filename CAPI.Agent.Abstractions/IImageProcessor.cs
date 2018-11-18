﻿using CAPI.Agent.Abstractions.Models;
using CAPI.ImageProcessing.Abstraction;

namespace CAPI.Agent.Abstractions
{
    public interface IImageProcessor
    {
        IJobResult[] CompareAndSaveLocally(
            string currentDicomFolder, string priorDicomFolder, string referenceDicomFolder,
            string[] lookupTablePaths, SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string outPriorReslicedDicom,
            string resultsDicomSeriesDescription, string priorReslicedDicomSeriesDescription);

        IJobResult[] CompareAndSaveLocally(IJob job, IRecipe recipe, SliceType sliceType);

        void AddOverlayToImage(string bmpFilePath, string overlayText);
    }
}
