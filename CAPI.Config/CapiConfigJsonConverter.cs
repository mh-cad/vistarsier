using VisTarsier.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace VisTarsier.Config
{

    public class CapiConfigJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type type)
        {
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonToken = JToken.Load(reader);

            var conf = CapiConfig.GenerateDefault();
            if (jsonToken["AgentDbConnectionString"] != null) conf.AgentDbConnectionString = jsonToken["AgentDbConnectionString"].ToString();
            if (jsonToken["Binaries"] != null) conf.Binaries = jsonToken["Binaries"].ToObject<Binaries>();
            if (jsonToken["DefaultRecipePath"] != null) conf.DefaultRecipePath = jsonToken["DefaultRecipePath"].ToString();
            if (jsonToken["DicomConfig"] != null) HandleDicomConf(conf, jsonToken["DicomConfig"]);
            if (jsonToken["Hl7ProcessPath"] != null) conf.Hl7ProcessPath = jsonToken["Hl7ProcessPath"].ToString();
            if (jsonToken["ProcessCasesAddedByHL7"] != null) conf.ProcessCasesAddedByHL7 = jsonToken["ProcessCasesAddedByHL7"].ToObject<bool>();
            if (jsonToken["ManualProcessPath"] != null) conf.ManualProcessPath = jsonToken["ManualProcessPath"].ToString();
            if (jsonToken["ProcessCasesAddedManually"] != null) conf.ProcessCasesAddedManually = jsonToken["ProcessCasesAddedManually"].ToObject<bool>();
            if (jsonToken["RunInterval"] != null) conf.RunInterval = jsonToken["RunInterval"].ToString();
            if (jsonToken["ImagePaths"] != null) conf.ImagePaths = jsonToken["ImagePaths"].ToObject<ImagePaths>();

            return conf;
        }

        private void HandleDicomConf(CapiConfig conf, JToken jtoken)
        {
            // This needs extra attention because we're using a list of abstracts.
            conf.DicomConfig.LocalNode = JsonConvert.DeserializeObject<DicomConfig.DicomConfigNode>(jtoken["LocalNode"].ToString());
            var remoteNodes = JsonConvert.DeserializeObject<List<DicomConfig.DicomConfigNode>>(jtoken["RemoteNodes"].ToString());
            conf.DicomConfig.RemoteNodes = new List<IDicomNode>();
            foreach (var remoteNode in remoteNodes) conf.DicomConfig.RemoteNodes.Add(remoteNode);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType().Equals(typeof(CapiConfig)))
            {
                var conf = (CapiConfig)value;
                serializer.Formatting = Formatting.Indented;
                writer.Formatting = Formatting.Indented;
                writer.WriteStartObject();

                writer.WriteComment("String used to connect to the database.");
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WritePropertyName("AgentDbConnectionString");
                serializer.Serialize(writer, conf.AgentDbConnectionString);
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WriteWhitespace(System.Environment.NewLine);

                writer.WriteComment("These are the paths to binary tools used by CAPI. They shouldn't need changing unless you have your own versions.");
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WritePropertyName("Binaries");
                serializer.Serialize(writer, conf.Binaries);
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WriteWhitespace(System.Environment.NewLine);

                writer.WriteComment("The path to the default recipe file. If the file doesn't exist, one will be created for you.");
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WritePropertyName("DefaultRecipePath");
                serializer.Serialize(writer, conf.DefaultRecipePath);
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WriteWhitespace(System.Environment.NewLine);

                writer.WriteComment("This is the DICOM service configuration, which allows CAPI to connect to remote PACS systems.");
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WritePropertyName("DicomConfig");
                serializer.Serialize(writer, conf.DicomConfig);
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WriteWhitespace(System.Environment.NewLine);

                writer.WriteComment("The folder where new HL7 accession numbers are stored.");
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WritePropertyName("Hl7ProcessPath");
                serializer.Serialize(writer, conf.Hl7ProcessPath);
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WriteWhitespace(System.Environment.NewLine);

                writer.WriteComment("Flag to for HL7 processing. If false, these cases will be ignored.");
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WritePropertyName("ProcessCasesAddedByHL7");
                serializer.Serialize(writer, conf.ProcessCasesAddedByHL7);
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WriteWhitespace(System.Environment.NewLine);

                writer.WriteComment("The folder where new maually added accession numbers are stored.");
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WritePropertyName("ManualProcessPath");
                serializer.Serialize(writer, conf.ManualProcessPath);
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WriteWhitespace(System.Environment.NewLine);

                writer.WriteComment("Flag to for processing manually added cases. If false, these cases will be ignored.");
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WritePropertyName("ProcessCasesAddedManually");
                serializer.Serialize(writer, conf.ProcessCasesAddedManually);
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WriteWhitespace(System.Environment.NewLine);

                writer.WriteComment("How often the agent checks for new cases (in seconds).");
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WritePropertyName("RunInterval");
                serializer.Serialize(writer, conf.RunInterval);
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WriteWhitespace(System.Environment.NewLine);

                writer.WriteComment("Path for temporary DICOM / NIfTI storage and descriptions for output files.");
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WritePropertyName("ImagePaths");
                serializer.Serialize(writer, conf.ImagePaths);
                writer.WriteWhitespace(System.Environment.NewLine);
                writer.WriteWhitespace(System.Environment.NewLine);

                writer.WriteEndObject();
            }
            
        }
    }
}
