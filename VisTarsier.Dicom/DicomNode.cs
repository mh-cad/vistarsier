using VisTarsier.Common;

namespace VisTarsier.Dicom
{
    public class DicomNode : IDicomNode
    {
        public string LogicalName { get; set; }
        public string AeTitle { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }

        public DicomNode()
        {
            LogicalName = string.Empty;
            AeTitle = string.Empty;
            IpAddress = string.Empty;
            Port = 0;
        }

        public DicomNode(string logicalName,string aeTitle, string ipAddress, int port)
        {
            LogicalName = logicalName;
            AeTitle = aeTitle;
            IpAddress = ipAddress;
            Port = port;
        }
    }
}
