using CAPI.Common.Abstractions.Config;

namespace CAPI.Agent.Abstractions
{
    public interface IAgent
    {
        void Run();

        ICapiConfig Config { get; set; }
        bool IsBusy { get; set; }
    }
}
