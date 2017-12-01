using CAPI.Dicom.Abstraction;
using System;
using System.Collections.Generic;

namespace CAPI.JobManager.Abstraction
{
    public interface IJob
    {
        IDicomStudy DicomStudyUnderFocus { get; set; }
        IDicomStudy DicomStudyBeingComparedTo { get; set; }
        IList<IIntegratedProcess> IntegratedProcesses { get; set; }
        IList<IDestination> Destinations { get; set; }
        event EventHandler<ProcessEventArgument> OnEachProcessCompleted;

        void Run();
    }
}