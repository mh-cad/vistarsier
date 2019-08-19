using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisTarsier.NiftiLib.Processing
{
    public class ResultFile
    {
        public enum ResultType { METADATA, CURRENT_PROCESSED, PRIOR_PROCESSED }

        public const string PRIOR_RESLICED_DESCRIPTION = "Prior Resliced";
        public string FilePath { get; set; }
        public string Description { get; set; }
        public ResultType Type;
    }
}
