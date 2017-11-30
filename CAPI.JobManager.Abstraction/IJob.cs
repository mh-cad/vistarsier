using System.Collections.Generic;
using CAPI.Dicom.Abstraction;

namespace CAPI.JobManager.Abstraction
{
    public interface IJob
    {
        IDicomStudy DicomStudyUnderFocus { get; set; }
        IDicomStudy DicomStudyBeingComparedTo { get; set; }
        IList<IIntegratedProcess> IntegratedProcesses { get; set; }
        IList<IDestination> Destinations { get; set; }

        void Run();
    }
}