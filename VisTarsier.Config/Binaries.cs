using System.IO;

namespace VisTarsier.Config
{
    public class Binaries
    {
        public Binaries() { }

        public Binaries(string basePath)
        {
            N4BiasFieldCorrection = Path.GetFullPath(Path.Combine(basePath, "ants", "N4BiasFieldCorrection.exe"));
            antsRegistration = Path.GetFullPath(Path.Combine(basePath, "ants", "antsRegistration.exe"));
            antsApplyTransforms = Path.GetFullPath(Path.Combine(basePath, "ants", "antsApplyTransforms.exe"));
            bfc = Path.GetFullPath(Path.Combine(basePath, "brain_suite", "bfc.exe"));
            bse = Path.GetFullPath(Path.Combine(basePath, "brain_suite", "bse.exe"));
            reformatx = Path.GetFullPath(Path.Combine(basePath, "cmtk", "reformatx.exe"));
            registration = Path.GetFullPath(Path.Combine(basePath, "cmtk", "registration.exe"));
            dcm2niix = Path.GetFullPath(Path.Combine(basePath, "dicom", "dcm2niix.exe"));
            img2dcm = Path.GetFullPath(Path.Combine(basePath, "dicom", "img2dcm.exe"));
        }

        // ANTS tools.
        public string N4BiasFieldCorrection { set; get; }
        public string antsRegistration { set; get; }
        public string antsApplyTransforms { set; get; }
        // Brain suite tools.
        public string bfc { set; get; }
        public string bse { set; get; }
        // CMTK tools.
        public string reformatx { set; get; }
        public string registration { set; get; }
        // Dicom tools.
        public string dcm2niix { set; get; }
        public string img2dcm { set; get; }
    }
}
