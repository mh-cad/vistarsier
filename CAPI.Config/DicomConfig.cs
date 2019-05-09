using CAPI.Common;
using System.Collections.Generic;
using System.Net;

namespace CAPI.Config
{
    public class DicomConfig
    {
        public class DicomConfigNode : IDicomNode
        {
            public string LogicalName { get; set; }
            public string AeTitle { get; set; }
            public string IpAddress { get; set; }
            public int Port { get; set; }
        }

        public IDicomNode LocalNode { get; set; }
        public List<IDicomNode> RemoteNodes { get; set; }

        public DicomConfig()
        {
            LocalNode = new DicomConfigNode
            {
                AeTitle = "CAPI",
                IpAddress = Dns.GetHostName(),
                LogicalName = "CAPI Local",
                Port = 4104
            };

            RemoteNodes = new List<IDicomNode>
            {
                new DicomConfigNode
                {
                    AeTitle = "DEFAULT-PACS",
                    IpAddress = "127.0.0.1",
                    LogicalName = "Default PACS service",
                    Port = 404
                }
            };
        }
    }
}