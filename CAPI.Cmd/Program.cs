using CAPI.ImageProcessing;
using CAPI.ImageProcessing.Abstraction;
using CAPI.NiftiLib;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace CAPI.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory + "../../");

            string floatingPath = "floating.nii";
            string fixedPath = "fixed.nii";
            string outputPrefix = "capiout";


            if (args.Length >= 2)
            {
                floatingPath = args[0];
                fixedPath = args[1];
                if (args.Length >= 3) outputPrefix = args[2];
            }
            else
            {
                System.Console.WriteLine("Usage:");
                System.Console.WriteLine("capi [floating file] [fixed file] [optional: output prefix]");
            }


            System.Console.WriteLine("Files read...");

            var timer = new Stopwatch();
            var totaltime = new Stopwatch();
            timer.Start();
            totaltime.Start();

            IImageProcessor ip = new ImageProcessor(null);
            string[] outputs = { outputPrefix + "-increase.nii", outputPrefix + "-decrease.nii" };
            ip.MSLesionCompare(fixedPath, floatingPath, fixedPath, true, true, true, outputs, "prior-resliced.nii");

            Console.WriteLine($@" ALL DONE! [{totaltime.Elapsed}]");
            Console.ReadKey();
        }
    }
}
