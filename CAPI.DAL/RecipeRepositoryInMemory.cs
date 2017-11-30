using CAPI.DAL.Abstraction;
using CAPI.JobManager.Abstraction;
using System.Collections.Generic;
using System.Linq;

namespace CAPI.DAL
{
    public class RecipeRepositoryInMemory : IRecipeRepositoryInMemory
    {
        private readonly IJobManagerFactory _jobManagerFactory;

        public RecipeRepositoryInMemory(IJobManagerFactory jobManagerFactory)
        {
            _jobManagerFactory = jobManagerFactory;
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public bool Add(IRecipe entity)
        {
            throw new System.NotImplementedException();
        }

        public bool Update(IRecipe entity)
        {
            throw new System.NotImplementedException();
        }

        public bool SaveChanges()
        {
            throw new System.NotImplementedException();
        }

        public bool Delete(IRecipe entity)
        {
            throw new System.NotImplementedException();
        }

        public IRecipe Get(int id)
        {
            throw new System.NotImplementedException();
        }

        public IQueryable<IRecipe> GetAll()
        {
            var recipeMs1 = _jobManagerFactory.CreateRecipe();
            var criterion1 = _jobManagerFactory.CreateStudySelectionCriteria();
            criterion1.AccessionNumber = "2017R0168610-1";

            recipeMs1.NewStudyCriteria = new List<IStudySelectionCriteria> { criterion1 };
            recipeMs1.IntegratedProcesses = new List<IIntegratedProcess>
            {
                _jobManagerFactory.CreateIntegratedProcess("1", "1", "-n 3 -d 25 -s 0.64 -r 1 --trim")
            };
            recipeMs1.Destinations = new List<IDestination> {
                _jobManagerFactory.CreateDestination("1", "", "ORTHANC")
            };

            return new List<IRecipe>
            {
                recipeMs1
            }.AsQueryable();
        }
    }
}
