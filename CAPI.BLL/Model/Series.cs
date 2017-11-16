using System;
using System.IO;
using CAPI.Common;
using CAPI.Common.Services;

namespace CAPI.BLL.Model
{
    public class Series
    {
        private readonly string _outputPath;
        private readonly string _executablesPath;
        public string OriginalDicomFolderPath { get; set; }
        public string Name { get; set; }
        public string HdrFullPath { get; set; }
        public string HdrSmoothBrainMaskFullPath { get; set; }
        public string HdrBrainMaskRemovedFullPath { get; set; }
        public string NiiFullPath { get; set; }
        public string BmpFolderPath { get; set; }
        public bool IsBrainMaskRemoved { get; set; }

        public Series(string dicomFolderPath)
        {
            _outputPath = Config.GetOutputDir();
            _executablesPath = Config.GetExecutablesPath();

            OriginalDicomFolderPath = dicomFolderPath;
            Name = string.Empty;
            HdrFullPath = string.Empty;
            HdrSmoothBrainMaskFullPath = string.Empty;
            HdrBrainMaskRemovedFullPath = string.Empty;
            NiiFullPath = string.Empty;
            BmpFolderPath = string.Empty;
            IsBrainMaskRemoved = false;
        }
    
        public void ExtractBrainMask()
        {
            // Make sure hdr files exist | if not convert dicom files to hdr/img pairs and takle note of hdr file path
            OriginalDicomToHdr();

            if (string.IsNullOrEmpty(HdrFullPath)) throw new FileNotFoundException();

            try
            {
                var brainMaskProc = ProcessBuilder.Build(_executablesPath, "bse09e.exe", // TODO3 Hard-coded filename
                    string.Format("-i {0} --mask {1}\\{2}_brain_surface -o {1}\\{2}_brain_surface_extracted {3}", HdrFullPath, _outputPath, Name, "-n 3 -d 25 -s 0.64 -r 1 --trim")); // TODO3 Hard-coded parameters
                brainMaskProc.Start();
                Logger.ProcessErrorLogWrite(brainMaskProc, "brainMaskProc");
                brainMaskProc.WaitForExit();

                HdrSmoothBrainMaskFullPath = _outputPath + '\\' + Name + "_brain_surface.hdr";
                HdrBrainMaskRemovedFullPath = _outputPath + '\\' + Name + "_brain_surface_extracted.hdr";
                IsBrainMaskRemoved = true;

                HdrToNii();
            }
            catch (Exception ex) // TODO3: Exception Handling
            {}
        }
    
        public void Do3DRegistration()
        {
            var imageFormatConvertor = new ImageProcessor(_executablesPath);

            if (string.IsNullOrEmpty(NiiFullPath)) return;

            // TODO To complete
       
        }

        public void CompareTo(Series series)
        {
        
        }

        private void OriginalDicomToHdr()
        {
            if (string.IsNullOrEmpty(OriginalDicomFolderPath)) throw new ArgumentNullException();
            if (!Directory.Exists(OriginalDicomFolderPath)) throw new DirectoryNotFoundException("Dicom folder path does not exist in file system: " + OriginalDicomFolderPath);
            if (Directory.GetFiles(OriginalDicomFolderPath).Length == 0) throw new FileNotFoundException("Dicom path contains no files: " + OriginalDicomFolderPath);

            var imageFormatConvertor = new ImageProcessor(_executablesPath);
            imageFormatConvertor.ConvertDicomToHdr(OriginalDicomFolderPath, _outputPath, Name);
        }

        private void HdrToNii()
        {
        
        }
    }
}
