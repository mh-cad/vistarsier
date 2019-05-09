namespace CAPI.Service.Db
{
    public interface IDestination
    {
        string Id { get; set; }
        string FolderPath { get; set; }
        string AeTitle { get; set; }
        string IpAddress { get; set; }
        string Port { get; set; }
        string DisplayName { get; set; }
    }
}