using CAPI.ImageProcessing;
using CAPI.ImageProcessing.Abstraction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPI.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            string floatingPath = "";
            string fixedPath = "";
            string outputPrefix = "capiout";
            if (args.Length >= 2)
            {
                floatingPath = args[0];
                fixedPath = args[1];
                if (args.Length >= 3) outputPrefix = args[2];
            }
            else
            {
                System.Console.WriteLine("Not enough arguments. Must specify a floating and fixed filename.");
            }

            var floatingNifti = new Nifti().ReadNifti(floatingPath);
            var fixedNifti = new Nifti().ReadNifti(fixedPath);
            System.Console.WriteLine("Files read...");

            var timer = new Stopwatch();
            timer.Start();

            // BiasCorrection
            System.Console.WriteLine("Starting bias correction...");
            var bias1 = Task.Run(() => { BiasCorrection.AntsN4(floatingNifti); });
            var bias2 = Task.Run(() => { BiasCorrection.AntsN4(fixedNifti); });
            bias1.Wait();
            bias2.Wait();
            System.Console.WriteLine($@"..done. [{timer.Elapsed}]");
            timer.Restart();

            // Brain Extraction
            System.Console.WriteLine("Starting brain extraction...");
            var brain1 = Task.Run(() => { BrainExtraction.BrainSuiteBSE(floatingNifti); });
            var brain2 = Task.Run(() => { BrainExtraction.BrainSuiteBSE(fixedNifti); });
            brain1.Wait();
            brain2.Wait();
            System.Console.WriteLine($@"..done. [{timer.Elapsed}]");
            timer.Restart();

            // Registration
            System.Console.WriteLine("Starting registration...");
            Registration.CMTKRegistration(floatingNifti, fixedNifti);
            System.Console.WriteLine($@"..done. [{timer.Elapsed}]");
            timer.Restart();

            // Normalize
            System.Console.WriteLine("Starting normalization...");
            Normalization.Normalize(floatingNifti, fixedNifti);
            System.Console.WriteLine($@"..done. [{timer.Elapsed}]");
            timer.Restart();

            // Compare Increase
            System.Console.WriteLine("Starting compare...");
            INifti increaseNifti = floatingNifti.DeepCopy(); // I Hope
            Compare.CompareMSLesionIncrease(increaseNifti, fixedNifti);
            increaseNifti.WriteNifti(outputPrefix + "-increase.nii");
            // Compare Decrease
            INifti decreaseNifti = floatingNifti.DeepCopy(); //maybe
            System.Console.WriteLine($@"..done. [{timer.Elapsed}]");
            timer.Restart();
            // Output fixed with increase overlay

            // Output fixed with decrease overlay


        }
    }
}
