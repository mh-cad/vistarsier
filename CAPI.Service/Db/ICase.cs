namespace VisTarsier.Service.Db
{
    public interface ICase
    {
        long Id { get; set; }
        string Accession { get; set; }
        string Status { get; set; }
        string Comment { get; set; }
        AdditionMethod AdditionMethod { get; set; }
    }

    public enum AdditionMethod
    {
        Hl7,
        Manually
    }
}
