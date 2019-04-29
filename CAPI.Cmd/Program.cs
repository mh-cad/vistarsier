using CAPI.General.Abstractions.Services;
using CAPI.General.Services;
using CAPI.ImageProcessing;
using CAPI.ImageProcessing.Abstraction;
using CAPI.NiftiLib;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

            IImageProcessor ip = new ImageProcessor(null, null);
            string[] outputs = { "out1.nii", "out2.nii" };
            ip.MSLesionCompare(fixedPath, floatingPath, fixedPath, true, true, true, outputs, "prior-resliced.nii");

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
