using VisTarsier.NiftiLib.Processing;

namespace VisTarsier.MS
{
    public class MSMetrics : Metrics
    {
        public double VoxelVolPrior { get; set; }
        public double VoxelVolCurrent { get; set; }
        public double VoxelVolUsed { get; set; }
        public double BrainMatch { get; set; }
        public double CorrectedBrainMatch { get; set; }
        public double EdgeRatioIncrease { get; set; }
        public double EdgeRatioDecrease { get; set; }
    }
}
