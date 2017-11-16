namespace CAPI.Dicom.Model
{
    public class DicomNode
    {
        public string AeTitle { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }

        public DicomNode()
        {
            AeTitle = string.Empty;
            IpAddress = string.Empty;
            Port = 0;
        }

        public DicomNode(string aeTitle, string ipAddress, int port)
        {
            AeTitle = aeTitle;
            IpAddress = ipAddress;
            Port = port;
        }
    }
}
