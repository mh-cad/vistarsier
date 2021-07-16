
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using VisTarsier.Config;
using VisTarsier.Module.MS;
using VisTarsier.NiftiLib;

namespace VisTarsier.CommandLineTool
{
    class Program
    { 

        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();

            if (args.Length < 2)
            {
                System.Console.WriteLine("Open-Vistarsier requires at least 2 NiftiFiles as input.");
                System.Console.WriteLine("Usage: vt [prior nifti] [current nifti] [output-prefix](optional)");
                return;
            }

            var prior = args[0];
            var current = args[1];
            var outputPrefix = "";
            if (args.Length > 2) { outputPrefix = args[2]; }

            if (!Path.IsPathRooted(prior)) prior = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, prior));
            if (!Path.IsPathRooted(current)) current = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, current));

            // Create an MS pipeline.
            var pipeline = new MSPipeline(
                current,
                prior,
                current,
                true, true, true,
                new string[] { outputPrefix + "vt-increase.nii", outputPrefix + "vt-decrease.nii" },  outputPrefix + "vt-prior.nii");

            var metrics = pipeline.Process();

            // Hack(ish), there's all sorts of to-ing and fro-ing with BMP encoding going on under the hood. 
            // If we don't do this step the nifti will blue itself.
            var nii = new NiftiFloat32().ReadNifti(outputPrefix+"vt-increase.nii");
            for (int i = 0; i < nii.Voxels.Length; ++i)
            {
                nii.Voxels[i] = ToBgr((int)nii.Voxels[i]);
            }
            nii.Header.dim[5] = 1; // This is to allow the result to be read by nibabel
            nii.WriteNifti(outputPrefix + "vt-increase.nii");

            System.Console.WriteLine("Complete:");
            System.Console.WriteLine($"Initial brain match  : {metrics.BrainMatch}");
            System.Console.WriteLine($"Union brain match    : {metrics.CorrectedBrainMatch}");
            System.Console.WriteLine($"Successful           : {metrics.Passed}");
            System.Console.WriteLine($"Time to complete     : {sw.Elapsed}");
        }

        public static uint ToBgr(int val)
        {
            var r = (val & (255 << 16)) >> 16;
            var g = (val & (255 << 8)) >> 8;
            var b = val & 255;

            return (uint)(b << 16 | g << 8 | r);
        }
    }
}
