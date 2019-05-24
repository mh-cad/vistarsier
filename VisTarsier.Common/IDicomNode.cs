namespace VisTarsier.Common
{
    public interface IDicomNode
    {
        string LogicalName { get; set; }
        string AeTitle { get; set; }
        string IpAddress { get; set; }
        int Port { get; set; }
    }
}
