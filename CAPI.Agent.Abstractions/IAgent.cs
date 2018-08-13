using CAPI.Common.Config;

namespace CAPI.Agent.Abstractions
{
    public interface IAgent
    {
        void Run();

        CapiConfig Config { get; set; }
        bool IsBusy { get; set; }
    }
}
