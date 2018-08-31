using CAPI.DAL.Abstraction;
using CAPI.Dicom.Abstraction;
using CAPI.JobManager.Abstraction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Unity;

namespace CAPI.Tests.Helpers
{
    public static class JobBuilder
    {
        private const string FixedAccession = "2018R0021135-1";
        private const string FloatingAccession = "2016R0176578-1";
        private const string Destination = @"D:\temp\Capi-Tests-Output";

        public static IJobNew<IRecipe> GetTestJob()
        {
            var container = Unity.CreateContainerCore();

            var dicomNodeRepo = container.Resolve<IDicomNodeRepository>();
            var jobBuilder = container.Resolve<IJobBuilderNew>();
            var recipeRepositoryInMemory = container.Resolve<IRecipeRepositoryInMemory<IRecipe>>();
            var jobManagerFactory = container.Resolve<IJobManagerFactory>();

            var recipe = recipeRepositoryInMemory.GetAll().FirstOrDefault();
            if (recipe == null) Assert.Fail("Failed to retreive recipe from recipe repository!");
            recipe.NewStudyAccession = FixedAccession;
            recipe.PriorStudyAccession = FloatingAccession;

            var localDicomNode = GetLocalNode(dicomNodeRepo);
            var sourceNode = dicomNodeRepo.GetAll()
                .FirstOrDefault(n => n.AeTitle == recipe.SourceAet);

            // Replace Recipe Destinations with Test OutputPath
            var destination = jobManagerFactory.CreateDestination("1", Destination, "");
            recipe.Destinations.Clear();
            recipe.Destinations.Add(destination);

            return jobBuilder.Build(recipe, localDicomNode, sourceNode);
        }

        private static IDicomNode GetLocalNode(IDicomNodeRepositoryReadOnly dicomNodeRepo)
        {
            return dicomNodeRepo.GetAll()
                .FirstOrDefault(n => string.Equals(n.AeTitle,
                    Environment.GetEnvironmentVariable("DcmNodeAET_Local", EnvironmentVariableTarget.User),
                    StringComparison.CurrentCultureIgnoreCase));
        }
    }
}