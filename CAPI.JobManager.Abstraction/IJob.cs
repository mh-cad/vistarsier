using CAPI.Dicom.Abstraction;
using System;
using System.Collections.Generic;

namespace CAPI.JobManager.Abstraction
{
    public interface IJob<T>
    {
        IDicomStudy DicomStudyFixed { get; set; }
        IDicomStudy DicomStudyFloating { get; set; }
        IList<IIntegratedProcess> IntegratedProcesses { get; set; }
        IList<IDestination> Destinations { get; set; }

        void Run();

        event EventHandler<ProcessEventArgument> OnEachProcessCompleted;
    }
}