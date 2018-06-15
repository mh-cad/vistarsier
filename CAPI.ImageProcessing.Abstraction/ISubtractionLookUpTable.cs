using System.Drawing;

namespace CAPI.ImageProcessing.Abstraction
{
    public interface ISubtractionLookUpTable
    {
        int Xmax { get; set; }
        int Ymin { get; set; }
        int Xmin { get; set; }
        int Ymax { get; set; }
        int Width { get; }
        int Height { get; }
        Color[,] Pixels { get; }

        void LoadImage(string filepath);
    }
}