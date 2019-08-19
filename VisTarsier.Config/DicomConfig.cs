using VisTarsier.Common;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace VisTarsier.Config
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
                IpAddress = GetFQDN(),
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

        /// <summary>
        /// Returns the fully qualified domain name for the machine
        /// </summary>
        /// <returns></returns>
        private static string GetFQDN()
        {
            string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName();

            domainName = "." + domainName;
            if (!hostName.EndsWith(domainName))  // if hostname does not already include domain name
            {
                hostName += domainName;   // add the domain name part
            }

            return hostName;                    // return the fully qualified name
        }
    }
}