using Newtonsoft.Json;
using System;

namespace CAPI.JobManager.Abstraction
{
    public interface IRecipeJsonConverter
    {
        void WriteJson(JsonWriter writer, object value, JsonSerializer serializer);
        object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer);
        bool CanConvert(Type objectType);
    }
}
