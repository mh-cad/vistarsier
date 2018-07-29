﻿using CAPI.Agent.Abstractions;
using CAPI.DAL;
using CAPI.DAL.Abstraction;
using CAPI.Dicom;
using CAPI.Dicom.Abstraction;
using CAPI.Dicom.Model;
using CAPI.ImageProcessing;
using CAPI.ImageProcessing.Abstraction;
using CAPI.JobManager;
using CAPI.JobManager.Abstraction;
using System.Collections.Generic;
using Unity;
using Unity.log4net;


namespace CAPI.Tests.Helpers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Unity
    {
        public static IUnityContainer CreateContainerCore()
        {
            var container = (UnityContainer)new UnityContainer()
                .AddNewExtension<Log4NetExtension>();

            container.RegisterType<IDicomNode, DicomNode>();
            container.RegisterType<IDicomFactory, DicomFactory>();
            container.RegisterType<IDicomServices, DicomServices>();
            container.RegisterType<IImageConverter, ImageConverter>();
            container.RegisterType<IImageProcessorNew, ImageProcessorNew>();
            container.RegisterType<IJobManagerFactory, JobManagerFactory>();
            container.RegisterType<IRecipe, Recipe>();
            container.RegisterType<IJobNew<IRecipe>, JobNew<IRecipe>>();
            container.RegisterType<IJobNew<IRecipe>, JobNew<IRecipe>>();
            container.RegisterType<IJobBuilderNew, JobBuilderNew>();
            container.RegisterType<ISeriesSelectionCriteria, SeriesSelectionCriteria>();
            container.RegisterType<IIntegratedProcess, IntegratedProcess>();
            container.RegisterType<IDestination, Destination>();
            container.RegisterType<IList<IDestination>, List<IDestination>>();
            container.RegisterType<IRecipeRepositoryInMemory<IRecipe>, RecipeRepositoryInMemory<Recipe>>();
            container.RegisterType<IDicomNodeRepository, DicomNodeRepositoryInMemory>();
            container.RegisterType<IValueComparer, ValueComparer>();
            container.RegisterType<IImageProcessingFactory, ImageProcessingFactory>();
            container.RegisterType<INifti, Nifti>();
            container.RegisterType<ISubtractionLookUpTable, SubtractionLookUpTable>();
            container.RegisterType<IAgent, CAPI.Agent.Agent>();
            container.RegisterType<CAPI.Agent.Abstractions.IImageProcessor, CAPI.Agent.ImageProcessor>();

            return container;
        }
    }
}