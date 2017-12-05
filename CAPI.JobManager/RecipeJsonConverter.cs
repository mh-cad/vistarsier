using CAPI.JobManager.Abstraction;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace CAPI.JobManager
{
    public class RecipeJsonConverter<TRecipe, TDestination, TIntegratedProcess, TStudySelectionCriteria>
        : JsonConverter, IRecipeJsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var recipe = default(TRecipe);

            serializer.Populate(jsonObject.CreateReader(), recipe);
            return recipe;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}