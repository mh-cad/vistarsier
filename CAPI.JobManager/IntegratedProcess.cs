using CAPI.JobManager.Abstraction;

namespace CAPI.JobManager
{
    public class IntegratedProcess : IIntegratedProcess
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string[] Parameters { get; set; }

        public IntegratedProcess(string id, string version, params string[] parameters)
        {
            Id = id;
            Version = version;
            Parameters = parameters;
        }

        public void Init()
        {
            throw new System.NotImplementedException();
        }

        public void Init(string[] parameters)
        {
            throw new System.NotImplementedException();
        }

        public void Run()
        {
            throw new System.NotImplementedException();
        }
    }
}
