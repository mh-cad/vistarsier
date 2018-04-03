using System;

namespace CAPI.JobManager
{
    public class DicomStudyNotFoundException : Exception
    {
        public DicomStudyNotFoundException()
        {
        }
        public DicomStudyNotFoundException(string message)
            : base(message)
        {
        }
        public DicomStudyNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}