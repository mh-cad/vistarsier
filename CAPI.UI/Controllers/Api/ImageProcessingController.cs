using CAPI.BLL.Model;
using CAPI.Common;
using CAPI.ImageProcessing;
using System.IO;
using System.Linq;
using System.Web.Http;

namespace CAPI.UI.Controllers.Api
{
    public class ImageProcessingController : ApiController
    {
        private readonly string _fixed = "fixed"; // TODO3: Hard-coded name
        private readonly string _floating = "floating"; // TODO3: Hard-coded name
        private const string BrainSurfaceSuffix = "_brain_surface"; // TODO3: Hard-coded name
        private const string BrainSurfaceExtSuffix = "_brain_surface_extracted"; // TODO3: Hard-coded name
        private const string ReslicedSuffix = "_resliced"; // TODO3: Hard-coded name
        private readonly string _fixedDcmDir;
        private readonly string _floatingDcmDir;
        private readonly string _outputDir;

        // Constructor
        public ImageProcessingController()
        {
            _outputDir = Config.GetOutputDir();
            _fixedDcmDir = Config.GetFixedDicomDir();
            _floatingDcmDir = Config.GetFloatingDicomDir();
        }

        [HttpGet]
        public string RunAll()
        {
            var imgProcessor = new ImageProcessor();
            ImageProcessor.RunAll();
            return "RunAll process was called...";
        }

        [HttpGet]
        public string Step1()
        {
            var imgProcessor = new ImageConverter();
            imgProcessor.Dicom2Hdr(_fixedDcmDir, _outputDir, _fixed);
            imgProcessor.Dicom2Hdr(_floatingDcmDir, _outputDir, _floating);
            //var resultFixed = imgProcessor.Dicom2Hdr(_fixedDcmDir, _outputDir, _fixed);
            //var resultFloating = imgProcessor.Dicom2Hdr(_floatingDcmDir, _outputDir, _floating);
            //return $"Step 1 completed. [{resultFixed}] and [{resultFloating}] are now created.";
            return "";
        }

        [HttpGet]
        public string Step2()
        {
            var imgProcessor = new ImageProcessor();
            var fixedImagesCount = Directory.GetFiles(_fixedDcmDir).Length;
            var floatingImagesCount = Directory.GetFiles(_floatingDcmDir).Length;

            imgProcessor.ExtractBrainMask(new SeriesHdr(_fixed, $"{_outputDir}\\{_fixed}.hdr", fixedImagesCount), _outputDir, out SeriesHdr fixedBrainMaskRemoved, out SeriesHdr fixedBrainMask);
            imgProcessor.ExtractBrainMask(new SeriesHdr(_floating, $"{_outputDir}\\{_floating}.hdr", floatingImagesCount), _outputDir, out SeriesHdr floatingBrainMaskRemoved, out SeriesHdr floatingBrainMask);
            return "Step2 completed. Brain Mask is now extracted. " +
                   $"[{fixedBrainMaskRemoved.Description}_brain_surface_extracted.hdr/img] - [{fixedBrainMask.Description}_brain_surface.hdr/img] - " +
                   $"[{floatingBrainMaskRemoved.Description}_brain_surface_extracted.hdr/img] - [{floatingBrainMask.Description}_brain_surface.hdr/img]";
        }

        [HttpGet]
        public string Step3()
        {
            var imgProcessor = new ImageProcessor();
            var fixedImagesCount = Directory.GetFiles(_fixedDcmDir).Length;
            //var floatingImagesCount = Directory.GetFiles(_floatingDcmDir).Length;
            //var brainMaskIsRemoved = Directory.GetFiles(_outputDir).Any(f => f.Contains(BrainSurfaceExtSuffix)); // TODO3: Find a better way to check whether brain mask has been removed or not

            var hdrFilesNamesNoExt = Directory.GetFiles(_outputDir)
                .Where(f => f.ToLower().EndsWith(".hdr"))
                .Select(f =>
                    {
                        var lastOrDefault = f.Split('\\').LastOrDefault();
                        return lastOrDefault?.Replace(".hdr", "");
                    }).ToArray();

            foreach (var hdrFileNameNoExt in hdrFilesNamesNoExt)
            {
                var imgConverter = new ImageConverter();
                imgConverter.Hdr2Nii(hdrFileNameNoExt+".hdr", _outputDir, hdrFileNameNoExt+".nii");

                imgProcessor.ConvertHdrToNii(
                    new SeriesHdr(hdrFileNameNoExt, $"{_outputDir}\\{hdrFileNameNoExt}.hdr", fixedImagesCount),
                    new SeriesHdr(_fixed, $"{_outputDir}\\{_fixed}.hdr", fixedImagesCount), hdrFileNameNoExt
                );
            }
            
            return $"Step 3 completed. HDR/IMG pairs converted to NII. {string.Join(", ", hdrFilesNamesNoExt)}";
        }

        [HttpGet]
        public string Step4()
        {
            var imgProcessor = new ImageProcessor();
            var fixedImagesCount = Directory.GetFiles(_fixedDcmDir).Length;
            var floatingImagesCount = Directory.GetFiles(_floatingDcmDir).Length;
            var srcDir = _outputDir;

            var fixedNiiFiles = Directory.GetFiles(srcDir).Where(f => f.EndsWith(".nii") && f.Contains(_fixed)).ToArray();
            var floatingNiiFiles = Directory.GetFiles(srcDir).Where(f => f.EndsWith(".nii") && f.Contains(_floating)).ToArray();

            foreach (var fixedNiiFile in fixedNiiFiles)
                foreach (var floatingNiiFile in floatingNiiFiles)
                {
                    if (floatingNiiFile.Replace(_floating, _fixed) != fixedNiiFile) continue;
                    if (fixedNiiFile.Contains(BrainSurfaceExtSuffix))
                        imgProcessor.Registration(
                            new SeriesNii(fixedNiiFile.Replace(".nii", "").Split('\\').LastOrDefault(), fixedNiiFile, fixedImagesCount),
                            new SeriesNii(floatingNiiFile.Replace(".nii", "").Split('\\').LastOrDefault(), floatingNiiFile, floatingImagesCount),
                            _outputDir
                        );
                }

            return "";
        }

        [HttpGet]
        public string Step5()
        {
            var imgProcessor = new ImageProcessor();
            var fixedImagesCount = Directory.GetFiles(_fixedDcmDir).Length;
            var floatingImagesCount = Directory.GetFiles(_floatingDcmDir).Length;

            imgProcessor.TakeDifference(
                new SeriesHdr(_fixed,$@"{_outputDir}\{_fixed}.hdr",fixedImagesCount),
                new SeriesNii($"{ _floating }{ ReslicedSuffix}", $@"{_outputDir}\{_floating}{ReslicedSuffix}.nii", floatingImagesCount), 
                new SeriesNii($"{_fixed}{BrainSurfaceSuffix}", $@"{_outputDir}\{_fixed}{BrainSurfaceSuffix}.nii", fixedImagesCount), 
                _outputDir, "0");

            return "";
        }

        [HttpGet]
        public string Step6()
        {
            var imgProcessor = new ImageProcessor();
            var floatingImagesCount = Directory.GetFiles(_floatingDcmDir).Length;

            imgProcessor.FlipAndConvertFloatingToDicom(new SeriesNii($"{_floating}{ReslicedSuffix}", $"{_outputDir}\\{_floating}{ReslicedSuffix}.nii",floatingImagesCount));

            return "";
        }

        [HttpGet]
        public string Step7()
        {
            var imgProcessor = new ImageProcessor();

            imgProcessor.ColorMap(_outputDir);

            imgProcessor.ConvertBmpsToDicom(_outputDir);

            return "";
        }

        [HttpGet]
        public string Step8()
        {
            var imgProcessor = new ImageProcessor();

            imgProcessor.CopyDicomHeaders(_outputDir);

            return "";
        }

        [HttpGet]
        public string ExtractBrainMask()
        {

            return "";
        }

        [HttpGet]
        public string Register()
        {

            return "";
        }

        [HttpGet]
        public string CompareTwoSeries()
        {

            return "";
        }
    }
}