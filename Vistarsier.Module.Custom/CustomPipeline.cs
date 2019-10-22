using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using VisTarsier.Common;
using VisTarsier.Config;
using VisTarsier.Module.MS;
using VisTarsier.NiftiLib;
using VisTarsier.NiftiLib.Processing;
using static VisTarsier.NiftiLib.Processing.ResultFile;

namespace VisTarsier.Module.Custom
{
    public class CustomPipeline : Pipeline<Metrics>
    {
        public override Metrics Metrics { get; }

        public override bool IsComplete { get; protected set; }

        private readonly Func<string, DataReceivedEventHandler, string> Nothing = (input, _) => { return input; };

        private readonly Recipe _recipe;
        private string _priorPath;
        private string _currentPath;
        private ILog _log;

        public CustomPipeline(Recipe recipe, string priorPath, string currentPath)
        {
            _log = Log.GetLogger();
           
            Metrics = new Metrics();
            IsComplete = false;
            _log.Info($"Using bias correction: {recipe.BiasFieldCorrection}");
            _log.Info($"Extracting Brain: {recipe.BiasFieldCorrection}");
            BiasCorrect = recipe.BiasFieldCorrection ? BiasCorrection.AntsN4 : Nothing;
            SkullStrip = recipe.ExtractBrain ? BrainExtraction.BrainSuiteBSE : Nothing;

            Register = Registration.ANTSRegistration;
            Reslicer = Registration.ANTSApplyTransforms;

            _priorPath = Path.GetFullPath(priorPath);
            _currentPath = Path.GetFullPath(currentPath);
            _recipe = recipe;
        }

        public override Metrics Process()
        {
            // Bias correction, skull stripping and registration.
            Preprocess();

            // Read in pre-processed files.
            _log.Info("Reading " + _currentPath);
            var currentnii = new NiftiFloat32().ReadNifti(_currentPath);
            _log.Info("Reading " + _priorPath);
            var priornii = new NiftiFloat32().ReadNifti(_priorPath);



            // If we have compare settings then we'll do a compare.
            if (_recipe.CompareSettings != null)
            {
                _log.Info("Comparing current to prior...");
                var hist = DoCompare(currentnii, priornii);
                // We can also output a compare histogram if we so choose. 
                if (_recipe.CompareSettings.GenerateHistogram) Metrics.ResultsSlides = new Bitmap[]{ hist.GenerateSlide() };
            }

            IsComplete = true;
            _log.Info("...done.");
            return Metrics;
        }
        
        private Histogram DoCompare(INifti<float> currentnii, INifti<float> priornii)
        {
            _log.Info("Starting normalization...");
            currentnii = Normalization.ZNormalize(currentnii, priornii);
            _log.Info($@"..done.");

            currentnii.RecalcHeaderMinMax();
            priornii.RecalcHeaderMinMax();

            var tasks = new List<Task>();
            var histogram = new Histogram
            {
                Prior = priornii,
                Current = currentnii
            };

            var cs = _recipe.CompareSettings;
            if (cs.CompareIncrease)
            {
                var t = Task.Run(() =>
                {
                    _log.Info("Comparing increased signal...");
                    var increase = Compare.GatedSubract(currentnii, priornii, cs.BackgroundThreshold, cs.MinRelevantStd, cs.MaxRelevantStd, cs.MinChange, cs.MaxChange);
                    for (int i = 0; i < increase.Voxels.Length; ++i) increase.Voxels[i] = increase.Voxels[i] > 0 ? increase.Voxels[i] : 0;
                    increase.RecalcHeaderMinMax();
                    increase.ColorMap = ColorMaps.RedScale();
                    histogram.Increase = increase;
                    var increaseOut = currentnii.AddOverlay(increase);
                    var outpath = _currentPath + ".increase.nii";
                    increaseOut.WriteNifti(outpath);
                    Metrics.ResultFiles.Add(new ResultFile() { FilePath = outpath, Description = "Increased Signal", Type = ResultType.CURRENT_PROCESSED });
                });
                tasks.Add(t);
            }
            if (cs.CompareDecrease)
            {
                _log.Info("Comparing decreased signal...");
                // I know the code in these two branches looks similar but there's too many inputs to make a function that much simpler...
                var t = Task.Run(() =>
                {
                    var decrease = Compare.GatedSubract(currentnii, priornii, cs.BackgroundThreshold, cs.MinRelevantStd, cs.MaxRelevantStd, cs.MinChange, cs.MaxChange);
                    for (int i = 0; i < decrease.Voxels.Length; ++i) decrease.Voxels[i] = decrease.Voxels[i] < 0 ? decrease.Voxels[i] : 0;
                    decrease.RecalcHeaderMinMax();
                    decrease.ColorMap = ColorMaps.ReverseGreenScale();

                    histogram.Decrease = decrease;
                    var decreaseOut = currentnii.AddOverlay(decrease);
                    var outpath = _currentPath + ".decrease.nii";
                    decreaseOut.WriteNifti(outpath);
                    Metrics.ResultFiles.Add(new ResultFile() { FilePath = outpath, Description = "Decreased Signal", Type = ResultType.CURRENT_PROCESSED });
                });
                tasks.Add(t);
            }
            Task.WaitAll(tasks.ToArray());
            _log.Info("...done.");

            return histogram;
        }

        private void Preprocess()
        {
            _log.Info("Preprocessing:");
            // Bias correction
            _log.Info("-  Bias correction...");
            string priorN4Metric = null;
            string currentN4Metric = null;
            var bcprior = Task.Run(() => BiasCorrect(_priorPath, (s, e) => { if (e?.Data != null && e.Data.StartsWith("  Iteration")) priorN4Metric = e.Data; }));
            var bccurrent = Task.Run(() => BiasCorrect(_currentPath, (s, e) => { if (e?.Data != null && e.Data.StartsWith("  Iteration")) currentN4Metric = e.Data; }));


            // Skull stripping
            _log.Info("-  Skull stripping...");
            string priorSSMetric = null;
            string currentSSMetric = null;
            var ssprior = Task.Run(() => SkullStrip(Path.GetFullPath(bcprior.Result), (s, e) => { if(e?.Data != null && e.Data.StartsWith("lowest cost")) priorSSMetric = e.Data; }));
            var sscurrent = Task.Run(() => SkullStrip(Path.GetFullPath(bccurrent.Result), (s, e) => { if (e?.Data != null && e.Data.StartsWith("lowest cost")) currentSSMetric = e.Data; }));

            _currentPath = Path.GetFullPath(sscurrent.Result);
            _priorPath = Path.GetFullPath(ssprior.Result);

            // Register based on recipe
            _log.Info("-  Registration...");
            string registrationMetric = null;
            if (_recipe.RegisterTo == Recipe.RegisterToOption.CURRENT)
            {
                _priorPath = Register(_priorPath, _currentPath, (s, e) => { if (e?.Data != null && e.Data.StartsWith(" 2DIAGNOSTIC")) registrationMetric = e.Data; }); 
                Metrics.ResultFiles.Add(new ResultFile() { FilePath = _priorPath, Description = ResultFile.PRIOR_RESLICED_DESCRIPTION, Type = ResultType.PRIOR_PROCESSED});
            }
            else if (_recipe.RegisterTo == Recipe.RegisterToOption.PRIOR)
            {
                _currentPath = Register(_currentPath, _priorPath, (s, e) => { if (e?.Data != null && e.Data.StartsWith(" 2DIAGNOSTIC")) registrationMetric = e.Data; });
                Metrics.ResultFiles.Add(new ResultFile() { FilePath = _currentPath, Description = "Current resliced", Type=ResultType.CURRENT_PROCESSED });
            }


            // Handle metrics...
            // For Bias Correction...
            // Iteration 50 (of 50). Current convergence value = 0.000820073 (threshold = 0) 
            if (priorN4Metric != null)
            {
                try
                {
                    priorN4Metric = priorN4Metric.Substring(priorN4Metric.IndexOf("value") + 8);
                    Metrics.Stats.Add("Prior N4 convergence: " + priorN4Metric.Substring(0, priorN4Metric.IndexOf("(threshold")));
                }
                catch { _log.Error("Couldn't parse: " + priorN4Metric); }
            }
            if (currentN4Metric != null)
            {
                try
                {
                    currentN4Metric = currentN4Metric.Substring(currentN4Metric.IndexOf("value") + 8);
                    Metrics.Stats.Add("Current N4 convergence: " + currentN4Metric.Substring(0, currentN4Metric.IndexOf("(threshold")));
                }
                catch { _log.Error("Couldn't parse: " + currentN4Metric); }
            }

            // For skull stripping...
            if (priorSSMetric != null)
            {
                try
                {
                    Metrics.Stats.Add("Prior skull strip cost: " + priorSSMetric.Substring(priorSSMetric.IndexOf("=") + 1));
                }
                catch { _log.Error("Couldn't parse: " + priorSSMetric); }
            }
            if (currentSSMetric != null)
            {
                try
                {
                    Metrics.Stats.Add("Current skull strip cost: " + currentSSMetric.Substring(currentSSMetric.IndexOf("=") + 1));
                }
                catch { _log.Error("Couldn't parse: " + currentSSMetric); }
            }

            // For registration...
            if (registrationMetric != null)
            {
                try
                {
                    var regMet = registrationMetric.Split(',');
                    if (regMet.Length > 3) registrationMetric = regMet[3];
                    Metrics.Stats.Add("Registration convergence: " + registrationMetric);
                }
                catch { _log.Error("Couldn't parse: " + registrationMetric); }
            }

            _log.Info("...done.");
        }

    }
}
