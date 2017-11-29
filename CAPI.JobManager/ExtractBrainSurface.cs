using CAPI.Dicom.Abstraction;
using CAPI.JobManager.Abstraction;

namespace CAPI.JobManager
{
    public class ExtractBrainSurface: IExtractBrainSurface
    {
        private string[] _parameters;
        
        public string Id { get; set; }
        public string Version { get; set; }
        public string[] Parameters { get; set; }
        public ISeries Series { get; set; }

        public void Init()
        {
            throw new System.NotImplementedException();
        }

        public void Init(params string[] parameters)
        {
            _parameters = parameters;
        }

        public void Run()
        {
            throw new System.NotImplementedException();
        }

        // Constructor
        public ExtractBrainSurface(string[] parameters)
        {
            Init(parameters);
        }
    }
}
