using CAPI.Agent.Abstractions.Models;
using CAPI.Common.Abstractions.Config;
using CAPI.Common.Abstractions.Services;
using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing.Abstraction;
using log4net;

namespace CAPI.Agent.Models
{
    public class Case : ICase
    {
        public int Id { get; set; }
        public string Accession { get; set; }
        public string Status { get; set; }
        public AdditionMethod AdditionMethod { get; set; }

        public void Process(Recipe recipe, IDicomFactory dicomFactory, IImageProcessingFactory imgProcFactory, ICapiConfig capiConfig,
                            IFileSystem fileSystem, IProcessBuilder processBuilder, ILog log)
        {
            var dicomConfig = GetDicomConfigFromCapiConfig(capiConfig, dicomFactory);
            var dicomServices = dicomFactory.CreateDicomServices(dicomConfig, fileSystem, processBuilder);
            var job = new JobBuilder(dicomServices,
                                     imgProcFactory,
                                     new ValueComparer(),
                                     fileSystem, processBuilder,
                                     capiConfig, log)
                      .Build(recipe);
            job.Process();
        }

        private static Dicom.Abstraction.IDicomConfig GetDicomConfigFromCapiConfig(
            ICapiConfig capiConfig, IDicomFactory dicomFactory)
        {
            var dicomConfig = dicomFactory.CreateDicomConfig();
            dicomConfig.ExecutablesPath = capiConfig.DicomConfig.DicomServicesExecutablesPath;
            return dicomConfig;
        }
    }
}
