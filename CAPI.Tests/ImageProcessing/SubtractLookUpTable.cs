using CAPI.Common.Config;
using CAPI.ImageProcessing.Abstraction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;

namespace CAPI.Tests.ImageProcessing
{
    [TestClass]
    public class SubtractLookUpTable
    {
        private IUnityContainer _unity;
        private string _testResourcesPath;
        private string _lookupTableFile;
        //private string _outfile;

        [TestInitialize]
        public void TestInit()
        {
            _unity = Helpers.Unity.CreateContainerCore();
            _testResourcesPath = $@"{Helper.GetTestResourcesPath()}/nifti";
            _lookupTableFile = $@"{_testResourcesPath}/lut.bmp";
        }

        [TestMethod]
        public void ReadLookUpTable()
        {
            var lut = _unity.Resolve<ISubtractionLookUpTable>();
            lut.LoadImage(_lookupTableFile);
        }
    }
}