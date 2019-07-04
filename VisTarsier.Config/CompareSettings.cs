using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisTarsier.Config
{
    public enum ChangeType { MS_LESION, ALL }
    public class CompareSettings
    {
        public bool CompareIncrease { get; set; }
        public bool CompareDecrease { get; set; }
        public float BackgroundThreshold { get; set; }
        public float MinRelevantStd { get; set; }
        public float MaxRelevantStd { get; set; }
        public float MinChange { get; set; }
        public float MaxChange { get; set; }
        public bool GenerateHistogram { get; set; }
    }
}
