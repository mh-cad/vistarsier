using VisTarsier.Config;
using VisTarsier.Dicom;
using log4net;
using VisTarsier.Service.Agent;

namespace VisTarsier.Service.Db
{
    public class Case : ICase
    {
        public long Id { get; set; }
        public string Accession { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public AdditionMethod AdditionMethod { get; set; }

        public static void Process(Recipe recipe, DbBroker context)
        {
            //var remoteNode = if capiConfig.DicomConfig.RemoteNodes.Find((dn) => dn.AeTitle.ToUpper().Equals(recipe.

            var job = new JobBuilder(new ValueComparer(), context).Build(recipe);
            job.Process();
        }
    }
}
