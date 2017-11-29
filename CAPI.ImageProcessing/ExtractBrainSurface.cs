using CAPI.ImageProcessing.Abstraction;

namespace CAPI.ImageProcessing
{
    public class ExtractBrainSurface : IExtractBrainSurface
    {
        public string[] Parameters { get; set; }

        public ExtractBrainSurface(string[] parameters)
        {
            Parameters = parameters;
        }

        public void Run()
        {
            throw new System.NotImplementedException();
        }
    }
}
