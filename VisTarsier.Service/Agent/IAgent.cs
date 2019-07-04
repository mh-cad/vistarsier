using VisTarsier.Config;

namespace VisTarsier.Service
{
    public interface IAgent
    {
        /// <summary>
        /// Start the Agent running. Agent will run on a loop with an interval defined in the config.
        /// </summary>
        void Start();
        /// <summary>
        /// Returns the configuration for the application.
        /// </summary>
        CapiConfig Config { get; set; }
        /// <summary>
        /// Will return true if there are no errors with the Agent (e.g. a bad or missing configuration).
        /// </summary>
        bool IsHealthy { get; set; }
    }
}
