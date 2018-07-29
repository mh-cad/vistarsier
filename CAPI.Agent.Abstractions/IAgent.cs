namespace CAPI.Agent.Abstractions
{
    public interface IAgent
    {
        void Run();

        Common.Config.CapiConfig Config { get; set; }
    }
}
