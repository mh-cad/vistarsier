using CAPI.Dicom.Abstraction;
using CAPI.ImageProcessing.Abstraction;
using CAPI.JobManager.Abstraction;

namespace CAPI.JobManager
{
    public class JobSeriesBundleNew : IJobSeriesBundleNew
    {
        public string Title { get; set; }
        public string DicomFolderPath { get; set; }
        public IDicomStudy ParentDicomStudy { get; set; }
        public string NiiFilePath { get; set; }
        public string Brain { get; set; }
        public string BrainMask { get; set; }
        public string Resliced { get; set; }
        public IFrameOfReference FrameOfReference { get; set; }
    }
}