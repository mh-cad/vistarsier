using CAPI.Config;
using CAPI.Dicom;
using log4net;
using CAPI.Service.Agent;

namespace CAPI.Service.Db
{
    public class Case : ICase
    {
        public long Id { get; set; }
        public string Accession { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public AdditionMethod AdditionMethod { get; set; }

        public static void Process(Recipe recipe,
                                   CapiConfig capiConfig, ILog log, DbBroker context)
        {
            var dicomConfig = GetDicomConfigFromCapiConfig(capiConfig);
            var dicomServices = new DicomServices(dicomConfig);
            var job = new JobBuilder(dicomServices,
                                     new ValueComparer(),
                                     capiConfig, log, context)
                      .Build(recipe);
            job.Process();
        }

        private static CAPI.Dicom.Abstractions.IDicomConfig GetDicomConfigFromCapiConfig(CapiConfig capiConfig)
        {
            var dicomConfig = new Dicom.DicomConfig();
            dicomConfig.Img2DcmFilePath = capiConfig.DicomConfig.Img2DcmFilePath;
            return dicomConfig;
        }
    }
}
