using System;
using System.Collections.Generic;

namespace CAPI.JobManager.Abstraction
{
    public interface IJobNew<T>
    {
        string OutputFolderPath { get; set; }

        IJobSeriesBundleNew Fixed { get; set; }
        IJobSeriesBundleNew Floating { get; set; }
        string ChangesDarkInFloating2BrightInFixed { get; set; }
        string ChangesBrightInFloating2DarkInFixed { get; set; }
        string ChangesBrainMask { get; set; }
        string PositiveOverlayImageFolder { get; set; }
        string PositiveOverlayDicomFolder { get; set; }
        string NegativeOverlayImageFolder { get; set; }
        string NegativeOverlayDicomFolder { get; set; }

        IList<IIntegratedProcess> IntegratedProcesses { get; set; }
        IList<IDestination> Destinations { get; set; }

        void Run();
        void ConvertDicomToNii();

        event EventHandler<IProcessEventArgument> OnEachProcessStarted;
        event EventHandler<IProcessEventArgument> OnEachProcessCompleted;
        event EventHandler<ILogEventArgument> OnLogContentReady;
    }
}