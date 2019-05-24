﻿
using System.Diagnostics;
using VisTarsier.MS;
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
                System.Console.WriteLine("Usage: vt [prior nifti] [current nifti]");
                return;
            }

            // Create an MS pipeline.
            var pipeline = new MSPipeline(
                args[1],
                args[0],
                args[1],
                true, true, true,
                new string[] { "vt-increase.nii", "vt-decrease.nii" }, "vt-prior.nii");

            var metrics = pipeline.Process();

            // Hack(ish), there's all sorts of to-ing and fro-ing with BMP encoding going on under the hood. 
            // If we don't do this step the nifti will blue itself.
            var nii = new NiftiFloat32().ReadNifti("vt-increase.nii");
            for (int i = 0; i < nii.Voxels.Length; ++i)
            {
                nii.Voxels[i] = ToBgr((int)nii.Voxels[i]);
            }
            nii.WriteNifti("vt-increase.nii");

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