using CAPI.Common.Config;
using CAPI.Common.Services;
using CAPI.ImageProcessing.Abstraction.ImageProcessor;

namespace CAPI.ImageProcessing.ImageProcessor
{
    public class Resizer : IResizer
    {
        private readonly string _javaClassPath;

        public Resizer()
        {
            var javaUtilsPath = ImgProc.GetJavaUtilsPath();
            _javaClassPath = $".;{javaUtilsPath}/PreprocessJavaUtils.jar;{javaUtilsPath}/lib/NICTA.jar;" +
                             $"{javaUtilsPath}/lib/vecmath.jar;{javaUtilsPath}/lib/ij.jar";
        }

        public string ResizeToDestWidth(string hdrFileFullPath, int destinationWidth)
        {
            var resizedNii = hdrFileFullPath.Replace(".hdr", "_resized.nii");

            try
            {
                const string methodName = "au.com.nicta.preprocess.main.ResizeNii"; // TODO3: Hard-coded method name
                var arguments = $"-classpath \"{_javaClassPath}\" {methodName} " +
                                $"{hdrFileFullPath} {resizedNii} {destinationWidth}";

                ProcessBuilder.CallJava(arguments, methodName);
            }
            catch
            {
                throw; // TODO3: Exception Handling
            }

            return resizedNii;
        }

        public string ResizeNiiToSameSize(string resizedTargetHdr, string originalHdrFileFullPath)
        {
            var resizedBackTargetNii = resizedTargetHdr.Replace("_resized.hdr", ".nii");
            try
            {
                const string methodName = "au.com.nicta.preprocess.main.ResizeNiiToSameSize"; // TODO3: Hard-coded method name
                var arguments = $"-classpath \"{_javaClassPath}\" {methodName} " +
                                $"{resizedTargetHdr} {resizedBackTargetNii} {originalHdrFileFullPath}";

                ProcessBuilder.CallJava(arguments, methodName);
            }
            catch
            {
                throw; // TODO3: Exception Handling
            }

            return resizedBackTargetNii;
        }
    }
}