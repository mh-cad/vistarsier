using CAPI.Dicom.Abstraction;

namespace CAPI.JobManager.Abstraction
{
    public interface IJobBuilder
    {
        IJob Build(IRecipe recipe, IDicomServices dicomServices, IJobManagerFactory jobManagerFactory);
    }
}