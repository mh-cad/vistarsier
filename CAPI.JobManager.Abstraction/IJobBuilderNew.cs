using CAPI.Dicom.Abstraction;

namespace CAPI.JobManager.Abstraction
{
    public interface IJobBuilderNew
    {
        IJobNew<IRecipe> Build(IRecipe recipe, IDicomNode localNode, IDicomNode sourceNode);
    }
}