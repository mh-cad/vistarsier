namespace CAPI.Agent.Abstractions.Models
{
    public interface ICase
    {
        int Id { get; set; }
        string Accession { get; set; }
        string Status { get; set; }
        AdditionMethod AdditionMethod { get; set; }
    }

    public enum AdditionMethod
    {
        Hl7 = 'H',
        Manually = 'M'
    }
}
