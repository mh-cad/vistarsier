using CAPI.Service.Db;

namespace CAPI.Service.Agent.Abstractions
{
    public interface IJobBuilder
    {
        IJob Build(IRecipe recipe);
    }
}
