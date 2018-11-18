using System;
using System.IO;
using System.Text;

namespace CAPI.Common
{
    public class RegistrationData : IRegistrationData
    {
        public string BrainRegistration { get; set; }
        public string BrainSettings { get; set; }
        public string BrainStatistics { get; set; }
        public string BrainStudyList { get; set; }

        public string MaskRegistration { get; set; }
        public string MaskSettings { get; set; }
        public string MaskStatistics { get; set; }
        public string MaskStudyList { get; set; }

        public RegistrationData()
        {
            BrainRegistration = string.Empty;
            BrainSettings = string.Empty;
            BrainStatistics = string.Empty;
            BrainStudyList = string.Empty;
            MaskRegistration = string.Empty;
            MaskSettings = string.Empty;
            MaskStatistics = string.Empty;
            MaskStudyList = string.Empty;
        }

        public void GetDataFromCmtkFiles(string brainCmtkFolderPath, string maskCmtkFolderPath)
        {
            if (!Directory.Exists(brainCmtkFolderPath)) throw new DirectoryNotFoundException($"Failed to find CMTK brain registration folder: [{brainCmtkFolderPath}]");
            if (!Directory.Exists(maskCmtkFolderPath)) throw new DirectoryNotFoundException($"Failed to find CMTK mask registration folder: [{maskCmtkFolderPath}]");
            if (Directory.GetFiles(brainCmtkFolderPath).Length != 4) throw new Exception($"Four files \"Registration\" \"Settings\" \"Statistics\" \"StudyList\" should exist in cmtk brain folder: [{brainCmtkFolderPath}]");
            if (Directory.GetFiles(maskCmtkFolderPath).Length != 4) throw new Exception($"Four files \"Registration\" \"Settings\" \"Statistics\" \"StudyList\" should exist in cmtk mask folder: [{maskCmtkFolderPath}]");

            BrainRegistration = File.ReadAllText(Path.Combine(brainCmtkFolderPath, "registration"));
            BrainSettings = File.ReadAllText(Path.Combine(brainCmtkFolderPath, "settings"));
            BrainStatistics = File.ReadAllText(Path.Combine(brainCmtkFolderPath, "statistics"));
            BrainStudyList = File.ReadAllText(Path.Combine(brainCmtkFolderPath, "studylist"));

            MaskRegistration = File.ReadAllText(Path.Combine(maskCmtkFolderPath, "registration"));
            MaskSettings = File.ReadAllText(Path.Combine(maskCmtkFolderPath, "settings"));
            MaskStatistics = File.ReadAllText(Path.Combine(maskCmtkFolderPath, "statistics"));
            MaskStudyList = File.ReadAllText(Path.Combine(maskCmtkFolderPath, "studylist"));
        }

        public string ToBase64()
        {
            var content = string.Join("|", BrainRegistration, BrainSettings, BrainStatistics, BrainStudyList
                                         , MaskRegistration, MaskSettings, MaskStatistics, MaskStudyList);

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(content));
        }

        public void FromBase64(string base64Text)
        {
            var regDataBytes = Convert.FromBase64String(base64Text);
            var regDataText = System.Text.Encoding.UTF8.GetString(regDataBytes);
            var filesSeparatedText = regDataText.Split('|');
            if (filesSeparatedText.Length != 8)
                throw new Exception("Registration data not in correct format i.e. eight text files should exist - four for brain and four for mask. " +
                                    "Please refer to CMTK registration.exe output format.");
            BrainRegistration = filesSeparatedText[0];
            BrainSettings = filesSeparatedText[1];
            BrainStatistics = filesSeparatedText[2];
            BrainStudyList = filesSeparatedText[3];
            MaskRegistration = filesSeparatedText[4];
            MaskSettings = filesSeparatedText[5];
            MaskStatistics = filesSeparatedText[6];
            MaskStudyList = filesSeparatedText[7];
        }

        public bool Exists()
        {

            if (string.IsNullOrEmpty(BrainRegistration)) return false;
            if (string.IsNullOrEmpty(BrainSettings)) return false;
            if (string.IsNullOrEmpty(BrainStatistics)) return false;
            if (string.IsNullOrEmpty(BrainStudyList)) return false;
            if (string.IsNullOrEmpty(MaskRegistration)) return false;
            if (string.IsNullOrEmpty(MaskSettings)) return false;
            if (string.IsNullOrEmpty(MaskStatistics)) return false;
            if (string.IsNullOrEmpty(MaskStudyList)) return false;
            return true;
        }

        public void ExportToFolder(string cmtkOutputDir, string seriesType)
        {
            if (Directory.Exists(cmtkOutputDir)) Directory.Delete(cmtkOutputDir, true);
            Directory.CreateDirectory(cmtkOutputDir);

            var isBrain = seriesType.Equals("brain", StringComparison.CurrentCultureIgnoreCase);

            var filePath = Path.Combine(cmtkOutputDir, "registration");
            File.WriteAllText(filePath, isBrain ? BrainRegistration : MaskRegistration);

            filePath = Path.Combine(cmtkOutputDir, "settings");
            File.WriteAllText(filePath, isBrain ? BrainSettings : MaskSettings);

            filePath = Path.Combine(cmtkOutputDir, "statistics");
            File.WriteAllText(filePath, isBrain ? BrainStatistics : MaskStatistics);

            filePath = Path.Combine(cmtkOutputDir, "studylist");
            File.WriteAllText(filePath, isBrain ? BrainStudyList : MaskStudyList);
        }
    }

    public interface IRegistrationData
    {
        string BrainRegistration { get; set; }
        string BrainSettings { get; set; }
        string BrainStatistics { get; set; }
        string BrainStudyList { get; set; }

        string MaskRegistration { get; set; }
        string MaskSettings { get; set; }
        string MaskStatistics { get; set; }
        string MaskStudyList { get; set; }

        void GetDataFromCmtkFiles(string brainCmtkFolderPath, string maskCmtkFolderPath);
        string ToBase64();
        void FromBase64(string base64Text);
        bool Exists();
        void ExportToFolder(string cmtkOutputDir, string seriesType);
    }
}
