using CAPI.Agent.Abstractions.Models;
using CAPI.Config;
using CAPI.Dicom.Abstractions;
using CAPI.ImageProcessing.Abstraction;
using log4net;

namespace CAPI.Agent.Models
{
    public class Case : ICase
    {
        public long Id { get; set; }
        public string Accession { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public AdditionMethod AdditionMethod { get; set; }

        public static void Process(Recipe recipe, IDicomFactory dicomFactory, IImageProcessingFactory imgProcFactory,
                                   CapiConfig capiConfig, ILog log, AgentRepository context)
        {
            var dicomConfig = GetDicomConfigFromCapiConfig(capiConfig, dicomFactory);
            var dicomServices = dicomFactory.CreateDicomServices(dicomConfig);
            var job = new JobBuilder(dicomServices,
                                     imgProcFactory,
                                     new ValueComparer(),
                                     capiConfig, log, context)
                      .Build(recipe);
            job.Process();
        }

        private static CAPI.Dicom.Abstractions.IDicomConfig GetDicomConfigFromCapiConfig(CapiConfig capiConfig, IDicomFactory dicomFactory)
        {
            var dicomConfig = dicomFactory.CreateDicomConfig();
            dicomConfig.Img2DcmFilePath = capiConfig.DicomConfig.Img2DcmFilePath;
            return dicomConfig;
        }
    }
}
