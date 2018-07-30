using System;
using CAPI.Agent.Abstractions;

namespace CAPI.Agent.Models
{
    public class Case : ICase
    {
        public int Id { get; set; }
        public string Accession { get; set; }
        public string Status { get; set; }
        public string AddedBy { get; set; }
        public void UpdateStatus(string status)
        {
            throw new NotImplementedException();
        }

        public void Process()
        {
            throw new NotImplementedException();
        }
    }
}
