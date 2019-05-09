using System;
using System.IO;
using CAPI.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CAPI.Tests.NiftiLib
{
    [TestClass]
    public class FilesTest
    {
        [TestMethod]
        public void CheckThirdPartyToolsExist()
        {
            var config = CapiConfigGetter.GetCapiConfig();
            var img2dcm = config.DicomConfig.Img2DcmFilePath;

            var ImgProcBinFolderPath = config.ImgProcConfig.ImgProcBinFolderPath;

            var dcm2nii = Path.Combine(ImgProcBinFolderPath, config.ImgProcConfig.Dcm2NiiExeRelFilePath);
            var bse = Path.Combine(ImgProcBinFolderPath, config.ImgProcConfig.BseExeRelFilePath);
            var registration = Path.Combine(ImgProcBinFolderPath, config.ImgProcConfig.RegistrationRelFilePath);
            var reformatx = Path.Combine(ImgProcBinFolderPath, config.ImgProcConfig.ReformatXRelFilePath);
            var bfc = Path.Combine(ImgProcBinFolderPath, config.ImgProcConfig.BfcExeRelFilePath);
            var defaultRecipe = config.DefaultRecipePath;

            var imageRepoFolder = config.ImgProcConfig.ImageRepositoryPath;
            var manualProcFolder = config.ManualProcessPath;
            var hl7ProcFolder = config.Hl7ProcessPath;

            Assert.IsTrue(CheckIfFileExists(img2dcm));

            Assert.IsTrue(CheckIfFolderExistsAndNotEmpty(ImgProcBinFolderPath, nameof(ImgProcBinFolderPath)));

            Assert.IsTrue(CheckIfFileExists(dcm2nii));
            Assert.IsTrue(CheckIfFileExists(bse));
            Assert.IsTrue(CheckIfFileExists(registration));
            Assert.IsTrue(CheckIfFileExists(reformatx));
            Assert.IsTrue(CheckIfFileExists(bfc));
            Assert.IsTrue(CheckIfFileExists(defaultRecipe));

            Assert.IsTrue(CheckIfFolderExistsOrCanBeCreated(imageRepoFolder, nameof(imageRepoFolder)));

            Assert.IsTrue(CheckIfFolderExistsOrCanBeCreated(manualProcFolder, nameof(manualProcFolder)));

            Assert.IsTrue(CheckIfFolderExistsOrCanBeCreated(hl7ProcFolder, nameof(hl7ProcFolder)));
        }

        private static bool CheckIfFolderExistsAndNotEmpty(string folderPath, string folderName)
        {
            return Directory.Exists(folderPath) && Directory.GetFiles(folderPath).Length > 0;
        }

        private static bool CheckIfFolderExistsOrCanBeCreated(string folderPath, string folderName)
        {
            try
            {
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool CheckIfFileExists(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}
