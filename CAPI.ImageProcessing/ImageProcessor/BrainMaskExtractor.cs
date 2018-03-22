using CAPI.Common.Config;
using CAPI.Common.Services;
using CAPI.ImageProcessing.Abstraction.ImageProcessor;
using System.IO;

namespace CAPI.ImageProcessing.ImageProcessor
{
    public class BrainMaskExtractor : IBrainMaskExtractor
    {
        private readonly string _executablesPath;
        private const string BseExe = "bse09e.exe";
        private const string BrainMaskRemovedSuffix = "_brain_surface_extracted";

        public BrainMaskExtractor()
        {
            _executablesPath = ImgProc.GetExecutablesPath();
        }

        public void ExtractBrainMask(string inputFileFullPath, string outputPath, string bseParams,
            out string brainMaskRemoved, out string brainMask)
        {
            var inputFileName = Path.GetFileNameWithoutExtension(inputFileFullPath);

            var arguments = $"-i {inputFileFullPath} " +
                            $"--mask {outputPath}\\{inputFileName}.mask.hdr " +
                            $"-o {outputPath}\\{inputFileName}{BrainMaskRemovedSuffix}.hdr {bseParams}";

            ProcessBuilder.CallExecutableFile($@"{_executablesPath}\{BseExe}", arguments);

            brainMaskRemoved = Path.Combine(outputPath, inputFileName + BrainMaskRemovedSuffix + ".hdr");
            brainMask = Path.Combine(outputPath, inputFileName + ".mask.hdr");
        }
    }
}
