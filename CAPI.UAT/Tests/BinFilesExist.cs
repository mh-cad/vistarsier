using CAPI.Agent;
using CAPI.Common.Config;
using System.IO;

namespace CAPI.UAT.Tests
{
    public class BinFilesExist : IUatTest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string SuccessMessage { get; set; }
        public string FailureMessage { get; set; }
        public string TestGroup { get; set; }
        public CapiConfig CapiConfig { get; set; }
        public AgentRepository Context { get; set; }

        public BinFilesExist()
        {
            Name = "Binary files exist";
            Description = "Checks if all third party tools binary files exist";
            SuccessMessage = "All binary files were found in paths specified in config files";
            FailureMessage = "Failed to find all binary files paths specified in config files";
            TestGroup = "Binary Files";
        }

        public bool Run()
        {
            var img2dcm = CapiConfig.DicomConfig.Img2DcmFilePath;

            var ImgProcBinFolderPath = CapiConfig.ImgProcConfig.ImgProcBinFolderPath;

            var dcm2nii = Path.Combine(ImgProcBinFolderPath, CapiConfig.ImgProcConfig.Dcm2NiiExeRelFilePath);
            var bse = Path.Combine(ImgProcBinFolderPath, CapiConfig.ImgProcConfig.BseExeRelFilePath);
            var registration = Path.Combine(ImgProcBinFolderPath, CapiConfig.ImgProcConfig.RegistrationRelFilePath);
            var reformatx = Path.Combine(ImgProcBinFolderPath, CapiConfig.ImgProcConfig.ReformatXRelFilePath);
            var bfc = Path.Combine(ImgProcBinFolderPath, CapiConfig.ImgProcConfig.BfcExeRelFilePath);
            var defaultRecipe = CapiConfig.DefaultRecipePath;

            var imageRepoFolder = CapiConfig.ImgProcConfig.ImageRepositoryPath;
            var manualProcFolder = CapiConfig.ManualProcessPath;
            var hl7ProcFolder = CapiConfig.Hl7ProcessPath;

            if (!CheckIfFileExists(img2dcm)) return false;

            if (!CheckIfFolderExistsAndNotEmpty(ImgProcBinFolderPath, nameof(ImgProcBinFolderPath))) return false;

            if (!CheckIfFileExists(dcm2nii)) return false;
            if (!CheckIfFileExists(bse)) return false;
            if (!CheckIfFileExists(registration)) return false;
            if (!CheckIfFileExists(reformatx)) return false;
            if (!CheckIfFileExists(bfc)) return false;
            if (!CheckIfFileExists(defaultRecipe)) return false;

            if (!CheckIfFolderExistsOrCanBeCreated(imageRepoFolder, nameof(imageRepoFolder))) return false;

            if (!CheckIfFolderExistsOrCanBeCreated(manualProcFolder, nameof(manualProcFolder))) return false;

            if (!CheckIfFolderExistsOrCanBeCreated(hl7ProcFolder, nameof(hl7ProcFolder))) return false;

            return true;
        }

        private static bool CheckIfFolderExistsAndNotEmpty(string folderPath, string folderName)
        {
            if (Directory.Exists(folderPath) && Directory.GetFiles(folderPath).Length > 0)
            {
                Logger.Write(folderName, false, Logger.TextType.Success, true);
                Logger.Write(" exists at [{folderPath}].", true, Logger.TextType.Success, false, 0, 0);
            }
            else
            {
                Logger.Write($"{Path.GetFileName(folderPath)} was not found at [{folderPath} or is empty]", true, Logger.TextType.Fail);
                return false;
            }

            return true;
        }

        private static bool CheckIfFolderExistsOrCanBeCreated(string folderPath, string folderName)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                {
                    Logger.Write(folderName, false, Logger.TextType.Success, true);
                    Logger.Write($" was found at [{folderPath}]", true, Logger.TextType.Success, false, 0, 0);
                }
                return true;
            }
            catch
            {
                Logger.Write($"Failed to create {folderName} folder at [{folderPath}]", true, Logger.TextType.Fail);
                return false;
            }
        }

        private static bool CheckIfFileExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                Logger.Write(Path.GetFileName(filePath), false, Logger.TextType.Success, true);
                Logger.Write($" exists at [{filePath}].", true, Logger.TextType.Success, false, 0, 0);
            }
            else
            {
                Logger.Write($"{Path.GetFileName(filePath)} was not found at [{filePath}]", true, Logger.TextType.Fail);
                return false;
            }

            return true;
        }

        public void FailureResolution()
        {
            Logger.Write("Failed to find files or folders which paths are specified in config.json file. Refer to failure logs above.", true, Logger.TextType.Fail);
        }
    }
}
