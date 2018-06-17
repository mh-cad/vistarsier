using CAPI.ImageProcessing.Abstraction;
using System.Drawing;

namespace CAPI.ImageProcessing
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SubtractionLookUpTable : ISubtractionLookUpTable
    {
        public int Xmin { get; set; }
        public int Xmax { get; set; }
        public int Ymin { get; set; }
        public int Ymax { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Color[,] Pixels { get; }

        public SubtractionLookUpTable()
        {
            Width = 511; // 255 * 2 + 1
            Height = 511; // 255 * 2 + 1
            Pixels = new Color[Width, Height];
        }

        //public SubtractionLookUpTable(int xmin, int xmax, int ymin, int ymax)
        //{
        //    Width = xmax - xmin + 1;
        //    Height = ymax - ymin + 1;

        //    Pixels = new Color[Width, Height];
        //}

        public void LoadImage(string filepath)
        {
            var img = new Bitmap(filepath);
            Width = img.Width;
            Height = img.Height;
            for (var i = 0; i < Width; i++)
                for (var j = 0; j < Height; j++)
                    Pixels[i, j] = img.GetPixel(i, j);
        }
    }
}