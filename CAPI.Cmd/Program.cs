using CAPI.ImageProcessing;
using CAPI.ImageProcessing.Abstraction;
using CAPI.NiftiLib;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace CAPI.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory + "../../");

            string repo = "D:/Capi-Files/ImageRepository";

            foreach(var arg in args)
            {
                if (arg.StartsWith("-r")) repo = arg.Substring(3).Trim(' ');
            }
            

            string currentDicomFolder = repo + "/Current/Dicom";
            string priorDicomFolder = repo + "/Prior/Dicom";
            string referenceDicomFolder = repo + "/Reference/Dicom";

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

            // Generate Nifti file from Dicom and pass to ProcessNifti Method for current series.
            System.Console.WriteLine($@"Start converting series dicom files to Nii");

            var currentNifti = ip.DicomToNifti(currentDicomFolder, "current.nii");
            var priorNifti = ip.DicomToNifti(priorDicomFolder, "prior.nii");
            var referenceNifti = ip.DicomToNifti(referenceDicomFolder, "reference.nii");

            System.Console.WriteLine($@"Finished converting series dicom files to Nii");

            string[] outputs = { outputPrefix + "-increase.nii", outputPrefix + "-decrease.nii" };
           
            // Process Nifti files.
            ip.MSLesionCompare(currentNifti, priorNifti, referenceNifti, true, true, true, outputs, "prior-resliced.nii");

            foreach(var output in outputs)
            {
                Console.WriteLine($@"Reading: {output}");
                new Nifti().ReadNifti(output);
            }

            Console.WriteLine($@" ALL DONE! [{totaltime.Elapsed}]");
            Console.ReadKey();
        }
    }
}
