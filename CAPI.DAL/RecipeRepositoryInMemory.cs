using CAPI.DAL.Abstraction;
using CAPI.JobManager.Abstraction;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CAPI.DAL
{
    public class RecipeRepositoryInMemory<TRecipe> : IRecipeRepositoryInMemory<IRecipe>
    {
        private readonly IJobManagerFactory _jobManagerFactory;

        public RecipeRepositoryInMemory(IJobManagerFactory jobManagerFactory)
        {
            _jobManagerFactory = jobManagerFactory;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool Add(IRecipe recipe)
        {
            throw new NotImplementedException();
        }

        public bool Update(IRecipe recipe)
        {
            throw new NotImplementedException();
        }

        public bool SaveChanges()
        {
            throw new NotImplementedException();
        }

        public bool Delete(IRecipe recipe)
        {
            throw new NotImplementedException();
        }

        public IRecipe Get(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<IRecipe> GetAll()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recipes.json");
            var recipesJsonString = File.ReadAllText(path);

            var recipe = JsonConvert.DeserializeObject<IList<TRecipe>>(recipesJsonString);

            //var criterion1 = _jobManagerFactory.CreateStudySelectionCriteria();
            //criterion1.AccessionNumber = "2017R0168610-1";

            //var recipeMs1 = _jobManagerFactory.CreateRecipe();
            //recipeMs1.NewStudyCriteria = new List<IStudySelectionCriteria> { criterion1 };
            //recipeMs1.IntegratedProcesses = new List<IIntegratedProcess>
            //{
            //    _jobManagerFactory.CreateExtractBrinSurfaceIntegratedProcess("1", "-n 3 -d 25 -s 0.64 -r 1 --trim"),
            //    _jobManagerFactory.CreateTakeDifferenceIntegratedProcess("1", ""),
            //    _jobManagerFactory.CreateRegistrationIntegratedProcess("1", ""),
            //    _jobManagerFactory.CreateColorMapIntegratedProcess("1", "")
            //};
            //recipeMs1.Destinations = new List<IDestination> {
            //    _jobManagerFactory.CreateDestination("1", "", "ORTHANC")
            //};

            return (IQueryable<IRecipe>)recipe.AsQueryable();
        }
    }
}