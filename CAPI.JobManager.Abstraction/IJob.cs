using System;
using System.Collections.Generic;

namespace CAPI.JobManager.Abstraction
{
    public interface IJob<T>
    {
        string OutputFolderPath { get; set; }
        IJobSeriesBundle DicomSeriesFixed { get; set; }
        IJobSeriesBundle DicomSeriesFloating { get; set; }
        IList<IIntegratedProcess> IntegratedProcesses { get; set; }
        IList<IDestination> Destinations { get; set; }

        void Run();

        event EventHandler<IProcessEventArgument> OnEachProcessCompleted;
        event EventHandler<ILogEventArgument> OnLogContentReady;
    }
}