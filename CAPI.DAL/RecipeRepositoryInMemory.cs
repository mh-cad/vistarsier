using CAPI.DAL.Abstraction;
using CAPI.JobManager.Abstraction;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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
            var path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                    ?? throw new InvalidOperationException()
                , "Recipes.json");
            var recipesJsonString = File.ReadAllText(path);

            var recipe = JsonConvert.DeserializeObject<IList<TRecipe>>(recipesJsonString);

            return (IQueryable<IRecipe>)recipe.AsQueryable();
        }
    }
}