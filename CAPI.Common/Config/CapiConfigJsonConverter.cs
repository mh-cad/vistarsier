using CAPI.Dicom.Abstractions;
using CAPI.Dicom.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CAPI.Common.Config
{

    public class CapiConfigJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type type)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonToken = JToken.Load(reader);

            var capiConfig = new CapiConfig
            {
                DicomConfig = new DicomConfig
                {
                    LocalNode = new DicomNode(),
                    RemoteNodes = new List<IDicomNode>()
                },

                DefaultRecipePath = jsonToken["DefaultRecipePath"].ToString(),
                ManualProcessPath = jsonToken["ManualProcessPath"].ToString(),
                ProcessCasesAddedManually = (bool)jsonToken["ProcessCasesAddedManually"],
                ProcessCasesAddedByHL7 = (bool)jsonToken["ProcessCasesAddedByHL7"],
                AgentDbConnectionString = jsonToken["AgentDbConnectionString"].ToString(),
                Hl7ProcessPath = jsonToken["Hl7ProcessPath"].ToString(),
                RunInterval = jsonToken["RunInterval"].ToString(),
                ImgProcConfig = JsonConvert.DeserializeObject<ImgProcConfig>(jsonToken["ImgProcConfig"].ToString()),
                TestsConfig = JsonConvert.DeserializeObject<TestsConfig>(jsonToken["TestsConfig"].ToString())
            };

            // Deserialize DicomConfig
            dynamic dicomConfig = jsonToken["DicomConfig"];

            capiConfig.DicomConfig.DicomServicesExecutablesPath = dicomConfig.DicomServicesExecutablesPath.Value;

            capiConfig.DicomConfig.LocalNode = JsonConvert.DeserializeObject<DicomNode>(dicomConfig.LocalNode.ToString());

            var remoteNodes = JsonConvert.DeserializeObject<List<dynamic>>(dicomConfig.RemoteNodes.ToString());
            foreach (var remoteNode in remoteNodes)
                capiConfig.DicomConfig.RemoteNodes.Add(JsonConvert.DeserializeObject<DicomNode>(remoteNode.ToString()));

            return capiConfig;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
