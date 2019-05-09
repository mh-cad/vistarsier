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
            var dicomServices = new DicomServices();
            var job = new JobBuilder(dicomServices,
                                     new ValueComparer(),
                                     capiConfig, log, context)
                      .Build(recipe);
            job.Process();
        }
    }
}
