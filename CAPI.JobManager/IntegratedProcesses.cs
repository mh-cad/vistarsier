using CAPI.ImageProcessing.Abstraction;
using CAPI.JobManager.Abstraction;
using System.Collections;
using System.Collections.Generic;

namespace CAPI.JobManager
{
    public class IntegratedProcesses : IEnumerable<IIntegratedProcess>
    {
        private readonly IImageProcessor _imageProcessor;

        public IntegratedProcesses(IImageProcessor imageProcessor)
        {
            _imageProcessor = imageProcessor;
        }

        public IEnumerator<IIntegratedProcess> GetEnumerator()
        {
            return new List<IIntegratedProcess>
            {
                new ExtractBrainSurface(_imageProcessor),
                new Registration(),
                new TakeDifference(),
                new ColorMap()
            }.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}