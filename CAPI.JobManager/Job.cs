using System.Collections.Generic;
using CAPI.JobManager.Abstraction;
using CAPI.Dicom.Abstraction;

namespace CAPI.JobManager
{
    public class Job : IJob
    {
        public IStudy StudyUnderFocus { get; set; }
        public IStudy StudyBeingComparedTo { get; set; }
        public IList<IIntegratedProcess> IntegratedProcesses { get; set; }
        public IList<IDestination> Destinations { get; set; }

        public void Run()
        {
            throw new System.NotImplementedException();
        }
    }
}