using CAPI.Dicom.Abstraction;

namespace CAPI.JobManager.Abstraction
{
    public interface IJobBuilder
    {
        IJob<IRecipe> Build(IRecipe recipe, IDicomNode localNode, IDicomNode sourceNode);
    }
}