using CAPI.ImageProcessing.Abstraction;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CAPI.JobManager
{
    public class JsonConverterRecipe : JsonConverter
    {
        private readonly IImageProcessor _imageProcessor;

        public JsonConverterRecipe(IImageProcessor imageProcessor1)
        {
            _imageProcessor = imageProcessor1;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonToken = JToken.Load(reader);
            var recipe = new Recipe();

            var destinations = JsonConvert.DeserializeObject<List<Destination>>(jsonToken["Destinations"].ToString());
            foreach (var destination in destinations) recipe.Destinations.Add(destination);

            var integratedProcesses = JsonConvert.DeserializeObject<List<IntegratedProcess>>(jsonToken["IntegratedProcesses"].ToString());
            foreach (var integratedProcess in integratedProcesses)
            {
                var processToAdd = new IntegratedProcesses(_imageProcessor).FirstOrDefault(p => p.Id == integratedProcess.Id);
                if (processToAdd == null) continue;
                processToAdd.Parameters = integratedProcess.Parameters;
                recipe.IntegratedProcesses.Add(processToAdd);
            }

            var newStudyCriteria = JsonConvert.DeserializeObject<List<StudySelectionCriteria>>(jsonToken["NewStudyCriteria"].ToString());
            foreach (var newStudyCriterion in newStudyCriteria) recipe.NewStudyCriteria.Add(newStudyCriterion);

            var priorStudyCriteria = JsonConvert.DeserializeObject<List<StudySelectionCriteria>>(jsonToken["PriorStudyCriteria"].ToString());
            foreach (var priorStudyCriterion in priorStudyCriteria) recipe.PriorStudyCriteria.Add(priorStudyCriterion);

            return recipe;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}