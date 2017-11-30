using System.Collections.Generic;
using CAPI.JobManager.Abstraction;
using CAPI.Dicom.Abstraction;

namespace CAPI.JobManager
{
    public class Job : IJob
    {
        public IDicomStudy DicomStudyUnderFocus { get; set; }
        public IDicomStudy DicomStudyBeingComparedTo { get; set; }
        public IList<IIntegratedProcess> IntegratedProcesses { get; set; }
        public IList<IDestination> Destinations { get; set; }

        public void Run()
        {
            throw new System.NotImplementedException();
        }
    }
}