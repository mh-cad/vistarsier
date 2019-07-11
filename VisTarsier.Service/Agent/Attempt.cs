using VisTarsier.Config;
using VisTarsier.Dicom;
using log4net;
using VisTarsier.Service;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisTarsier.Service
{
    public class Attempt
    {
        public enum AdditionMethod
        {
            Hl7,
            Manually
        }

        public long Id { get; set; }
        public string CurrentAccession { get; set; }
        public string SourceAet { get; set; }
        public string DestinationAet { get; set; }
        public string PatientId { get; set; }
        public string PatientFullName { get; set; }
        public string PatientBirthDate { get; set; }
        public string CurrentSeriesUID { get; set; }
        public string PriorAccession { get; set; }
        public string PriorSeriesUID { get; set; }
        public string CustomRecipe { get; set; }
        public string ReferenceSeries { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public string DbExt { get; set; }

        public AdditionMethod Method { get; set; }

        public Attempt()
        {

        }
    }
}
