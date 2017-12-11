using System.Collections.Generic;

namespace CAPI.ImageProcessing.Abstraction
{
    public interface IImageConverter
    {
        void Dicom2Hdr(string dicomDir, string outputDir, string outputFileNameNoExt);
        void DicomToNii(string dicomDir, string outputDir, string outputFileNameNoExt);
        IEnumerable<string> ConvertDicom2Viewable(string dicomDir, string outputDir = "", string outFileFormat = "png");
        bool Hdr2Nii(string hdrFileFullPath, string outputDir, string niiFileNameNoExt);
    }
}