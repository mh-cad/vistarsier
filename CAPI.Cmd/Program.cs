using CAPI.General.Abstractions.Services;
using CAPI.General.Services;
using CAPI.ImageProcessing;
using CAPI.ImageProcessing.Abstraction;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPI.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory("C:/repos/CAPI/CAPI.Cmd");

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
                System.Console.WriteLine("Not enough arguments. Must specify a floating and fixed filename.");
            }


            System.Console.WriteLine("Files read...");

            var timer = new Stopwatch();
            var totaltime = new Stopwatch();
            timer.Start();
            totaltime.Start();

            IFileSystem fs = new FileSystem();
            IImageProcessor ip = new ImageProcessor(fs, null, null, LogManager.GetLogger("logjammin"));
            string[] outputs = { "out1.nii", "out2.nii" };
            ip.ExtractBrainRegisterAndCompare(floatingPath, fixedPath, floatingPath, new string[0], SliceType.Sagittal, true, true, true, outputs, "prior-resliced.nii");

            var out1nifti = new Nifti().ReadNifti("out1.nii");
            out1nifti.GetSlice(114, SliceType.Sagittal).Save("somthing.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

            // BiasCorrection
          //  System.Console.WriteLine("Starting bias correction...");
          //  var bias1 = Task.Run(() => { return BiasCorrection.AntsN4(floatingPath); });
          //  var bias2 = Task.Run(() => { return BiasCorrection.AntsN4(fixedPath); });
          //  bias1.Wait();
          //  bias2.Wait();
          //  floatingPath = bias1.Result;
          //  fixedPath = bias2.Result;
          //  System.Console.WriteLine($@"..done. [{timer.Elapsed}]");
          //  timer.Restart();

          //  // Brain Extraction
          //  System.Console.WriteLine("Starting brain extraction...");
          //  var brain1 = Task.Run(() => { return BrainExtraction.BrainSuiteBSE(floatingPath); });
          //  var brain2 = Task.Run(() => { return BrainExtraction.BrainSuiteBSE(fixedPath); });
          //  brain1.Wait();
          //  brain2.Wait();
          //  floatingPath = brain1.Result;
          //  fixedPath = brain2.Result;
          //  System.Console.WriteLine($@"..done. [{timer.Elapsed}]");
          //  timer.Restart();

          //  // Registration
          //  System.Console.WriteLine("Starting registration...");
          //  floatingPath = Registration.CMTKRegistration(floatingPath, fixedPath);
          //  System.Console.WriteLine($@"..done. [{timer.Elapsed}]");
          //  timer.Restart();

          //  var floatingNifti = new Nifti().ReadNifti(floatingPath);
          //  var fixedNifti = new Nifti().ReadNifti(fixedPath);
          //  //var brainNifti1 = new Nifti().ReadNifti(brain1.Result);
          //  //var brainNifti2 = new Nifti().ReadNifti(brain2.Result);

          //  ////Generate single brain mask.
          //  //for (int i = 0; i < brainNifti1.voxels.Length; ++i)
          //  //{
          //  //    if (brainNifti1.voxels[i] > 0 || brainNifti2.voxels[i] > 0)
          //  //    {
          //  //        brainNifti1.voxels[i] = 1;
          //  //    }
          //  //    else
          //  //    {
          //  //        brainNifti1.voxels[i] = 0;
          //  //    }
          //  //}

          //  //var origFloating = floatingNifti.DeepCopy();

          //  //for (int i = 0; i < brainNifti1.voxels.Length; ++i)
          //  //{
          //  //    floatingNifti.voxels[i] *= brainNifti1.voxels[i];
          //  //    fixedNifti.voxels[i] *= brainNifti1.voxels[i];
          //  //}

          //  // Normalize
          //  System.Console.WriteLine("Starting normalization...");
          //  floatingNifti = Normalization.ZNormalize(floatingNifti, fixedNifti);
          //  System.Console.WriteLine($@"..done. [{timer.Elapsed}]");
          //  timer.Restart();

          //  // Compare 
          //  System.Console.WriteLine("Starting compare...");
          //  var increaseTask = Task.Run(() => { return Compare.CompareMSLesionIncrease(floatingNifti, fixedNifti); });
          //  var decreaseTask = Task.Run(() => { return Compare.CompareMSLesionDecrease(floatingNifti, fixedNifti); });
          //  increaseTask.Wait();
          //  decreaseTask.Wait();
          //  var increaseNifti = increaseTask.Result;
          //  var decreaseNifti = decreaseTask.Result;
          //  System.Console.WriteLine($@"..done. [{timer.Elapsed}]");
          //  timer.Restart();

          //  // Get slices with overlay
          ////  System.Console.WriteLine("Generating overlays...");

          //  //Overlay increase and decrease values:
          //  System.Console.WriteLine("Generating RGB overlays...");
          //  var overlayTask1 = Task.Run(() => { return floatingNifti.AddOverlay(increaseNifti); });
          //  var overlayTask2 = Task.Run(() => { return floatingNifti.AddOverlay(decreaseNifti); });
          //  overlayTask1.Wait();
          //  overlayTask2.Wait();
          //  var increaseNiftiRGB = overlayTask1.Result;
          //  var decreaseNiftiRGB = overlayTask2.Result;

          //  // Write files out to disk.
          //  var writeTask1 = Task.Run(() => { increaseNiftiRGB.WriteNifti("out1.nii"); });
          //  var writeTask2 = Task.Run(() => { decreaseNiftiRGB.WriteNifti("out2.nii"); });
          //  writeTask1.Wait();
          //  writeTask2.Wait();

            

          //  //var sliceTask1 = Task.Run(()=> { return getSlices(floatingNifti, increaseNifti); });
          //  //var sliceTask2 = Task.Run(() => { return getSlices(floatingNifti, decreaseNifti); });
          //  //sliceTask1.Wait();
          //  //sliceTask2.Wait();
          //  //var slicesIncrease = sliceTask1.Result;
          //  //var slicesDecrease = sliceTask2.Result;
          //  System.Console.WriteLine($@"..done. [{timer.Elapsed}]");



          //  //// Output floating with increase overlay
          //  //for (int i = 0; i < slicesIncrease.Length; ++i)
          //  //{
          //  //    slicesIncrease[i].Save($@"{outputPrefix}-increase-{i}.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
          //  //}
          //  //// Output floating with decrease overlay
          //  //for (int i = 0; i < slicesDecrease.Length; ++i)
          //  //{
          //  //    slicesDecrease[i].Save($@"{outputPrefix}-decrease-{i}.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
          //  //}

            Console.WriteLine($@" ALL DONE! [{totaltime.Elapsed}]");
            Console.ReadKey();
        }

        private static Bitmap[] getSlices(INifti mainNifti, INifti overlayNifti)
        {
            mainNifti.GetDimensions(SliceType.Sagittal, out int width, out int height, out int nSlices);
            Bitmap[] output = new Bitmap[nSlices];

            for (int i = 0; i < nSlices; ++i)
            {
                // Draw overlay nifti over main nifti
                Bitmap slice = mainNifti.GetSlice(i, SliceType.Sagittal);
                Bitmap overlay = overlayNifti.GetSlice(i, SliceType.Sagittal);
                Graphics g = Graphics.FromImage((Image)slice);
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                g.DrawImage(overlay, new Point(0, 0));
                g.Save();
                output[i] = slice;
            }
            return output;
        }
    }
}
