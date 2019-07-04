using System;
using System.Diagnostics;

namespace VisTarsier.NiftiLib.Processing
{
    public abstract class Pipeline<T> where T : Metrics
    {
        // Setup what pre-processing toolchain we are using...
        public Func<string, DataReceivedEventHandler, string> BiasCorrect;
        public Func<string, DataReceivedEventHandler, string> SkullStrip;
        // Note, these should be the same tool since they're going to use the correct temp files for the transform matrix.
        public Func<string, string, DataReceivedEventHandler, string> Register;
        public Func<string, string, DataReceivedEventHandler, string> Reslicer;

        public abstract T Metrics { get; }

        public abstract bool IsComplete { get; protected set; }

        public abstract T Process();
    }
}