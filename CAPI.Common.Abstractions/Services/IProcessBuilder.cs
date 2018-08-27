using System.Diagnostics;

namespace CAPI.Common.Abstractions.Services
{
    public interface IProcessBuilder
    {
        Process CallExecutableFile(string fileFullPath, string arguments, string workingDir = "");
        void CallJava(string arguments, string methodCalled, string workingDir = "");
    }
}