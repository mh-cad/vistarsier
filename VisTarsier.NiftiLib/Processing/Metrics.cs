using System.Collections.Generic;
using System.Drawing;

namespace VisTarsier.NiftiLib.Processing
{
    public class Metrics
    {
        public Metrics()
        {
            ResultsSlides = new Bitmap[0];
            ResultFiles = new List<ResultFile>();
            Stats = new List<string>();
        }
        public bool Passed { get; set; } = true;
        public string Notes { get; set; }
        public Bitmap[] ResultsSlides { get; set; }
        public List<ResultFile> ResultFiles{ get; set; }
        public List<string> Stats { get; set; }
    }
}
