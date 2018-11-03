using System.Diagnostics;

namespace CAPI.General.Abstractions.Services
{
    public interface IProcessBuilder
    {
        Process CallExecutableFile(string fileFullPath, string arguments, string workingDir = "",
            DataReceivedEventHandler outputDataReceived = null,
            DataReceivedEventHandler errorOccuredInProcess = null);

        Process CallJava(string javaFullPath, string arguments, string methodCalled, string workingDir = "",
            DataReceivedEventHandler outputDataReceived = null,
            DataReceivedEventHandler errorOccuredInProcess = null);
    }
}