namespace CAPI.Common.Abstractions.Services
{
    public interface IProcessBuilder
    {
        string CallExecutableFile(string fileFullPath, string arguments, string workingDir = "");
        void CallJava(string arguments, string methodCalled, string workingDir = "");
    }
}