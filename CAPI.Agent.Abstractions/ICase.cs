namespace CAPI.Agent.Abstractions
{
    public interface ICase
    {
        int Id { get; set; }
        string Accession { get; set; }
        string Status { get; set; }
        string AddedBy { get; set; }

        void UpdateStatus(string status);
        void Process();
    }
}
