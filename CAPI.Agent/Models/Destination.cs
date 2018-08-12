using CAPI.Agent.Abstractions.Models;

namespace CAPI.Agent.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Destination : IDestination
    {
        public string Id { get; set; }
        public string FolderPath { get; set; }
        public string AeTitle { get; set; }
        public string IpAddress { get; set; }
        public string Port { get; set; }
        public string DisplayName { get; set; }
    }
}
