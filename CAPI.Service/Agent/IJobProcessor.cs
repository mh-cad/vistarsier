﻿using CAPI.NiftiLib;
using CAPI.Service.Db;

namespace CAPI.Service.Agent.Abstractions
{
    public interface IJobProcessor
    {
        IJobResult[] CompareAndSaveLocally(
            string currentDicomFolder, string priorDicomFolder, string referenceDicomFolder,
            SliceType sliceType,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string outPriorReslicedDicom,
            string resultsDicomSeriesDescription, string priorReslicedDicomSeriesDescription);

        IJobResult[] CompareAndSaveLocally(IJob job, IRecipe recipe, SliceType sliceType);

        void AddOverlayToImage(string bmpFilePath, string overlayText);
    }
}