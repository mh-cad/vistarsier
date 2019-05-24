using VisTarsier.Config;
using VisTarsier.Service.Db;

namespace VisTarsier.Service.Agent.Abstractions
{
    public interface IJobBuilder
    {
        IJob Build(IRecipe recipe);
    }
}
