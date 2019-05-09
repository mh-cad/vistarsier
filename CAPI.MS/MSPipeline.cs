﻿using CAPI.NiftiLib;
using CAPI.NiftiLib.Processing;
using log4net;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CAPI.Common;

namespace CAPI.MS
{
    public class IThinkSomethingWentWrongException : Exception
    {
        public IThinkSomethingWentWrongException(string message) : base(message)
        {
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class MSPipeline : Pipeline<MSMetrics>
    {
        public override MSMetrics Metrics { get; }
        public override bool IsComplete { get; }

        private readonly ILog _log;
        private string _currentNii;
        private string _priorNii;
        private string _referenceNii;
        private bool _extractBrain;
        private bool _register;
        private bool _biasFieldCorrect;
        private string[] _resultNiis;
        private string _outPriorReslicedNii;

        public MSPipeline(
            string currentNii, string priorNii, string referenceNii,
            bool extractBrain, bool register, bool biasFieldCorrect,
            string[] resultNiis, string outPriorReslicedNii)
        {
            _log = Log.GetLogger();
            IsComplete = false;
            Metrics = new MSMetrics();
            BiasCorrect = BiasCorrection.AntsN4;
            SkullStrip = BrainExtraction.BrainSuiteBSE;
            Register = Registration.ANTSRegistration;
            Reslicer = Registration.ANTSApplyTransforms;

            _extractBrain = extractBrain;
            _register = register;
            _biasFieldCorrect = biasFieldCorrect;
            _resultNiis = resultNiis;
            _outPriorReslicedNii = outPriorReslicedNii;
        }

        /// <summary>
        /// Main method responsible for calling other methods to Extract Brain from current and prior series, Register the two, BFC and Normalize and then compare using a lookup table
        /// </summary>
        /// <param name="currentNii">current series nifti file path</param>
        /// <param name="priorNii">prior series nifti file path</param>
        /// <param name="referenceNii">reference series nifti file path (If exists, used for universal frame of reference)</param>
        /// <param name="extractBrain">to do skull stripping or not</param>
        /// <param name="register">to register or not</param>
        /// <param name="biasFieldCorrect">to perform bias field correction or not</param>
        /// <param name="resultNiis">end result output nifti files path</param>
        /// <param name="outPriorReslicedNii">resliced prior series nifti file path</param>
        public override MSMetrics Process()
        {
            // We consider this pipeline one-shot.
            if (IsComplete) throw new IThinkSomethingWentWrongException("You've already processed this instance. Make a new one or check the metrics.");
            // Quick sanity check that files exist.
            FileSystem.FilesExist(new[] { _currentNii, _priorNii });

            // Setup our things...
            DataReceivedEventHandler dataout = null;//(s, e) => { _log.Debug(e.Data); System.Console.WriteLine(e.Data); };
            var stopwatch1 = new Stopwatch();

            // BiasCorrection
            if (_biasFieldCorrect)
            {
                _log.Info("Starting bias correction...");
                stopwatch1.Start();
                var bias1 = Task.Run(() => { return BiasCorrect(_currentNii, dataout); });
                var bias2 = Task.Run(() => { return BiasCorrect(_priorNii, dataout); });
                //bias1.Wait();
                //bias2.Wait();
                _currentNii = bias1.Result;
                _priorNii = bias2.Result;
                _log.Info($@"..done. [{stopwatch1.Elapsed}]");
                stopwatch1.Restart();
            }

            var currentWithSkull = _currentNii;
            var priorWithSkull = _priorNii;

            // Brain Extraction
            _log.Info("Starting brain extraction...");
            var brain1 = Task.Run(() => { return SkullStrip(_currentNii, dataout); });
            var brain2 = Task.Run(() => { return SkullStrip(_priorNii, dataout); });
            var brain3 = Task.Run(() => { return SkullStrip(_referenceNii, dataout); }); // TODO: This can be null.
            brain1.Wait();
            brain2.Wait();
            brain3.Wait();
            _currentNii = brain1.Result;
            _priorNii = brain2.Result;
            _referenceNii = brain3.Result;
            _log.Info($@"..done. [{stopwatch1.Elapsed}]");
            stopwatch1.Restart();

            // Registration
            if (_register)
            {
                var refBrain = string.IsNullOrEmpty(_referenceNii) && File.Exists(_referenceNii) ? _referenceNii : _currentNii;

                // Registration
                _log.Info("Starting registration...");
                _priorNii = Register(_priorNii, refBrain, dataout);
                if (!refBrain.Equals(_currentNii)) // If we're using a third file for registration...
                {
                    _priorNii = Register(_currentNii, refBrain, dataout);
                }
                _log.Info($@"..done. [{stopwatch1.Elapsed}]");
                stopwatch1.Restart();

                //TODO fix this...
                priorWithSkull = Reslicer(priorWithSkull, refBrain, dataout);
            }

            // Convert files to INifti, now that we're done with pre-processing.
            INifti currentNifti = new Nifti().ReadNifti(_currentNii);
            INifti priorNifti = new Nifti().ReadNifti(_priorNii);
            INifti currentNiftiWithSkull = new Nifti().ReadNifti(currentWithSkull);
            INifti priorNiftiWithSkull = new Nifti().ReadNifti(priorWithSkull);

            // Check brain extraction match...
            _log.Info($@"Checking brain extraction...");
            stopwatch1.Restart();
            CheckBrainExtractionMatch(currentNifti, priorNifti, currentNiftiWithSkull, priorNiftiWithSkull, Metrics);
            _log.Info($@"...done [{stopwatch1.Elapsed}]");

            // Normalize
            stopwatch1.Restart();
            _log.Info("Starting normalization...");
            currentNifti = Normalization.ZNormalize(currentNifti, priorNifti);
            _log.Info($@"..done. [{stopwatch1.Elapsed}]");
            stopwatch1.Restart();

            // Compare 
            _log.Info("Starting compare...");
            var increaseTask = Task.Run(() => { return Compare.CompareMSLesionIncrease(currentNifti, priorNifti); });
            var decreaseTask = Task.Run(() => { return Compare.CompareMSLesionDecrease(currentNifti, priorNifti); });
            increaseTask.Wait();
            decreaseTask.Wait();
            var increaseNifti = increaseTask.Result;
            var decreaseNifti = decreaseTask.Result;
            _log.Info($@"..done. [{stopwatch1.Elapsed}]");
            stopwatch1.Restart();

            // Estimate edge ratio. The edge ratio is the ratio of change which is an edge vs. the change which is bounded.
            // A lower ratio implies that there are larger connected areas of change, which may be more clinically significant.
            // This is an estimate as the check is only over one dimension (to save time).
            _log.Info($@"Estimating edge ratio (lower implies more meaningful change)");
            EstimateEdgeRatio(increaseNifti, decreaseNifti, Metrics);
            _log.Info($@"..done. [{stopwatch1.Elapsed}]");

            stopwatch1.Reset();

            // Write the prior resliced file.
            var priorToExport = _extractBrain ? priorNifti : priorNiftiWithSkull;
            priorToExport.WriteNifti(_outPriorReslicedNii);

            //Overlay increase and decrease values:
            _log.Info("Generating RGB overlays...");
            var currentToExport = _extractBrain ? currentNifti : currentNiftiWithSkull;
            var overlayTask1 = Task.Run(() => { return currentToExport.AddOverlay(increaseNifti); });
            var overlayTask2 = Task.Run(() => { return currentToExport.AddOverlay(decreaseNifti); });
            overlayTask1.Wait();
            overlayTask2.Wait();
            var increaseNiftiRGB = overlayTask1.Result;
            var decreaseNiftiRGB = overlayTask2.Result;

            // Write files out to disk.
            var writeTask1 = Task.Run(() => { increaseNiftiRGB.WriteNifti(_resultNiis[0]); });
            var writeTask2 = Task.Run(() => { decreaseNiftiRGB.WriteNifti(_resultNiis[1]); });
            writeTask1.Wait();
            writeTask2.Wait();

            _log.Info($@"..done. [{stopwatch1.Elapsed}]");

            return Metrics;
        }

        private void EstimateEdgeRatio(INifti increaseNifti, INifti decreaseNifti, MSMetrics qaResults)
        {
            var varianceIncrease = 0d;
            var totalIncrease = 0d;
            var varianceDecrease = 0d;
            var totalDecrease = 0d;
            for (int i = 0; i < increaseNifti.voxels.Length - 1; ++i)
            {
                if ((increaseNifti.voxels[i] > 0 != increaseNifti.voxels[i + 1] > 0)) varianceIncrease++;
                if (increaseNifti.voxels[i] > 0) totalIncrease++; // Note, we'll miss the last voxel but it's just a rough estimate.
                if ((decreaseNifti.voxels[i] < 0 != decreaseNifti.voxels[i + 1] < 0)) varianceDecrease++;
                if (decreaseNifti.voxels[i] < 0) totalDecrease++;

                if (i == increaseNifti.voxels.Length - 2)
                {
                    if (increaseNifti.voxels[i + 1] > 0) totalIncrease++;
                    if (decreaseNifti.voxels[i + 1] < 0) totalDecrease++;
                }
            }

            _log.Info($@"Edge ratio for increase: {varianceIncrease / totalIncrease}");
            _log.Info($@"Edge ratio for decrease: {varianceDecrease / totalDecrease}");

            if (totalIncrease == 0 || totalDecrease == 0 || double.IsNaN(varianceIncrease / totalIncrease))
            {
                qaResults.Passed = false;
            }
        }

        private void CheckBrainExtractionMatch(
            INifti currentNifti, INifti priorNifti,
            INifti currentNiftiWithSkull, INifti priorNiftiWithSkull, MSMetrics qaResults)
        {
            var volCurrent = 0d;
            var volPrior = 0d;
            var volWSkullCurrent = 0;
            var volWSkullPrior = 0;
            for (int i = 0; i < currentNifti.voxels.Length; ++i)
            {
                if (currentNifti.voxels[i] > 0) volCurrent++;
                if (currentNiftiWithSkull.voxels[i] > 30) volWSkullCurrent++;
                if (priorNifti.voxels[i] > 0) volPrior++;
                if (priorNiftiWithSkull.voxels[i] > 30) volWSkullPrior++;
            }
            var match = Math.Min(volPrior, volCurrent) / Math.Max(volPrior, volCurrent);
            // Add results to QA list
            qaResults.VoxelVolPrior = volPrior;
            qaResults.VoxelVolCurrent = volCurrent;
            qaResults.BrainMatch = match;

            _log.Info($@"Percentage of current volume that's brain: {(int)(volCurrent / volWSkullCurrent * 100d)}%");
            _log.Info($@"Percentage of prior volume that's brain: {(int)(volPrior / volWSkullPrior * 100d)}%");
            _log.Info($@"Brain extraction match: {(int)(match * 100)}%");

            // If the match is sub-80% it's probably because one of the brains didn't extract so 
            // the compare operation will automatically use the intersection of the two. On the 
            // other hand if we're above 80% but less than 95% one of the extractions probably cut
            // out a chunk of brain. So we'll make a mask of the union and apply it to both sides.
            if (match > 0.7 && match < 0.95)
            {
                _log.Info($@"Brain extraction match not good enough, taking the union...");
                // Let's try to make the brain mask an OR of the two.
                var mask = currentNifti.DeepCopy();
                for (int i = 0; i < mask.voxels.Length; ++i)
                {
                    if (currentNifti.voxels[i] != 0 || priorNifti.voxels[i] != 0) mask.voxels[i] = 1;
                    else mask.voxels[i] = 0;
                }

                currentNifti = currentNiftiWithSkull.DeepCopy();
                priorNifti = priorNiftiWithSkull.DeepCopy();

                for (int i = 0; i < currentNifti.voxels.Length; ++i)
                {
                    currentNifti.voxels[i] = currentNifti.voxels[i] * mask.voxels[i];
                    priorNifti.voxels[i] = priorNifti.voxels[i] * mask.voxels[i];
                }

                currentNifti.RecalcHeaderMinMax();
                priorNifti.RecalcHeaderMinMax();

                // Check brain extraction match again...
                volCurrent = 0d;
                volPrior = 0d;
                for (int i = 0; i < currentNifti.voxels.Length; ++i)
                {
                    if (currentNifti.voxels[i] > 0) volCurrent++;
                    if (priorNifti.voxels[i] > 0) volPrior++;
                }
                match = Math.Min(volPrior, volCurrent) / Math.Max(volPrior, volCurrent);
                _log.Info($@"Brain extraction match after mask: {(int)(match * 100)}%");
            }
            else if (match < 0.7)
            {
                qaResults.Passed = false;
            }

            // In theory this should stop the skull highlights darkening the output images...
            priorNifti.RecalcHeaderMinMax();
            currentNifti.RecalcHeaderMinMax();
            priorNiftiWithSkull.Header.cal_max = priorNifti.Header.cal_max;
            currentNiftiWithSkull.Header.cal_max = currentNifti.Header.cal_max;
        }
    }
}