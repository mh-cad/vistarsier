using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisTarsier.Common;
using VisTarsier.NiftiLib;
using VisTarsier.NiftiLib.Processing;

namespace VisTarsier.Module.MS
{
    public class Histogram
    {
        public INifti<float> Prior { get; set; }
        public INifti<float> Current { get; set; }
        public INifti<float> Increase { get; set; }
        public INifti<float> Decrease { get; set; }

        public Bitmap GenerateSlide()
        {
            // Init 2-D histo matrix
            double[][] diffMatrix = new double[1024][];
            for (int i = 0; i < 1024; ++i) diffMatrix[i] = new double[1024];
            double[][] increaseMatrix = new double[1024][];
            for (int i = 0; i < 1024; ++i) increaseMatrix[i] = new double[1024];
            double[][] decreaseMatrix = new double[1024][];
            for (int i = 0; i < 1024; ++i) decreaseMatrix[i] = new double[1024];

            var max = double.MinValue;
            var min = double.MaxValue;

            var rangeStart = Math.Min(Prior.Voxels.Min(), Current.Voxels.Min());
            var rangeEnd = Math.Max(Prior.Voxels.Max(), Current.Voxels.Max());
            var range = rangeEnd - rangeStart;
            var diff = Subtract(Prior.Voxels, Current.Voxels);

            for (int i = 0; i < diff.Length; ++i) diff[i] = Math.Abs(diff[i]);

            for (int i = 0; i < diff.Length; ++i)
            {
                int x = Math.Min(1023, (int)((Prior.Voxels[i] - rangeStart) / range  * 1024));
                int y = Math.Min(1023, (int)((Current.Voxels[i] - rangeStart) / range * 1024));
                diffMatrix[x][y] += 1;
                if (Increase != null && Increase.Voxels[i] != 0) increaseMatrix[x][y] += 1;//(double)Increase.Voxels[i];
                if (Decrease != null && Decrease.Voxels[i] != 0) decreaseMatrix[x][y] += 1;//(double)Decrease.Voxels[i];
                if (diffMatrix[x][y] > max && x > 10 && y > 10) max = diffMatrix[x][y];
                if (diffMatrix[x][y] < min) min = diffMatrix[x][y];
            }

            // We could normalise to the given range, but a flat multiplier will
            // help us compare between scans.
            var bmp = new DirectBitmap(1024, 1024);
            Log.GetLogger().Info($"Min {min}, Max {max}, Range = { max - min}, ideal multi={255.0 / (max - min)}");
            var multiplier = 3;//255.0 / (max - min);

            for (int x = 0; x < 1024; ++x)
            {
                for (int y = 0; y < 1024; ++y)
                {
                    var grey = Math.Min((int)((diffMatrix[x][y] - min) * multiplier), 255);
                    grey = Math.Max(grey, 0);
                    bmp.SetPixel(x, y, Color.FromArgb(grey, grey, grey));
                    if (increaseMatrix[x][y] != 0) bmp.SetPixel(x, y, Color.FromArgb(Math.Min(255, (int)(grey * 1.5)), (int)(grey / 1.5), 0));
                    if (decreaseMatrix[x][y] != 0) bmp.SetPixel(x, y, Color.FromArgb(0, Math.Min(255, (int)(grey * 1.5)), 0));
                    if (x == y) bmp.SetPixel(x, y, Color.FromArgb(128, grey, grey));
                }
            }

            return bmp.Bitmap;
        }

        private static float[] Subtract(float[] prior, float[] current)
        {
            var output = new float[current.Length];
            for (var i = 0; i < current.Length; ++i)
            {
                output[i] = current[i] - prior[i];
            }

            return output;
        }
    }
}
