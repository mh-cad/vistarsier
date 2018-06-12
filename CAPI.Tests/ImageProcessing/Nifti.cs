using CAPI.ImageProcessing.Abstraction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;

namespace CAPI.Tests.ImageProcessing
{
    [TestClass]
    public class Nifti
    {
        private IUnityContainer _unity;
        private string _testResourcesPath;
        private string _infile;
        private string _outfile;

        [TestInitialize]
        public void TestInit()
        {
            _unity = Helpers.Unity.CreateContainerCore();
            _testResourcesPath = Common.Config.Helper.GetTestResourcesPath();
            _infile = $@"{_testResourcesPath}\Fixed\fixed.nii";
            _outfile = $@"{_testResourcesPath}\Fixed\fixed.noro.nii";
        }

        [TestMethod]
        public void Read()
        {
            var nifti = _unity.Resolve<INifti>();
            nifti.ReadNifti(_infile);
        }

        [TestMethod]
        public void Write()
        {
            var nifti = _unity.Resolve<INifti>();
            nifti.ReadNifti(_infile);
            //nifti.Reorient(256, 256, 160);
            nifti.WriteNifti(_outfile);
        }

        [TestMethod]
        public void Reorient()
        {
            var nifti = _unity.Resolve<INifti>();
            nifti.ReadNifti(_infile);
            nifti.Reorient(256, 256, 160);
        }
    }
}