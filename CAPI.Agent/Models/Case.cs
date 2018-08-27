using CAPI.Agent.Abstractions.Models;
using CAPI.Common.Abstractions.Services;
using CAPI.Common.Config;
using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing.Abstraction;
using log4net;

namespace CAPI.Agent.Models
{
    public class Case : ICase
    {
        public long Id { get; set; }
        public string Accession { get; set; }
        public string Status { get; set; }
        public AdditionMethod AdditionMethod { get; set; }

        public void Process(Recipe recipe, IDicomFactory dicomFactory, IImageProcessingFactory imgProcFactory, CapiConfig capiConfig,
                            IFileSystem fileSystem, IProcessBuilder processBuilder, ILog log)
        {
            var dicomConfig = GetDicomConfigFromCapiConfig(capiConfig, dicomFactory);
            var dicomServices = dicomFactory.CreateDicomServices(dicomConfig, fileSystem, processBuilder, log);
            var job = new JobBuilder(dicomServices,
                                     imgProcFactory,
                                     new ValueComparer(),
                                     fileSystem, processBuilder,
                                     capiConfig, log)
                      .Build(recipe);
            job.Process();
        }

        private static IDicomConfig GetDicomConfigFromCapiConfig(
            CapiConfig capiConfig, IDicomFactory dicomFactory)
        {
            var dicomConfig = dicomFactory.CreateDicomConfig();
            dicomConfig.ExecutablesPath = capiConfig.DicomConfig.DicomServicesExecutablesPath;
            return dicomConfig;
        }
    }
}
