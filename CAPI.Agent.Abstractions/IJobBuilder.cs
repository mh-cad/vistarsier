using CAPI.Agent.Abstractions.Models;

namespace CAPI.Agent.Abstractions
{
    public interface IJobBuilder
    {
        IJob Build(IRecipe recipe);
    }
}
