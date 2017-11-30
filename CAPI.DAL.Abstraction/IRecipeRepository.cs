using CAPI.JobManager.Abstraction;
using System.Linq;

namespace CAPI.DAL.Abstraction
{
    public interface IRecipeRepository : IRepository<IRecipe>
    {
        IRecipe Get(int id);
        IQueryable<IRecipe> GetAll();
    }
}