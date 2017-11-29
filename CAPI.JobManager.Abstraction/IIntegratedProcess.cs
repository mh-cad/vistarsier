namespace CAPI.JobManager.Abstraction
{
    public interface IIntegratedProcess
    {
        string Id { get; set; }
        string Version { get; set; }
        string[] Parameters { get; set; }
        void Init();
        void Init(params string[] parameters);
        void Run();
    }
}