﻿using System.Drawing;

namespace VisTarsier.NiftiLib.Processing
{
    public class Metrics
    {
        public bool Passed { get; set; } = true;
        public string Notes { get; set; }
        public Bitmap[] ResultsSlides { get; set; }
    }
}
