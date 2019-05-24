using System;
using System.Drawing;

namespace VisTarsier.Extensions
{
    public static class BitmapExtensions
    {
        public static uint ToBgr(this int val)
        {
            var r = (val & (255 << 16)) >> 16;
            var g = (val & (255 << 8)) >> 8;
            var b = val & 255;

            return Convert.ToUInt32(b << 16 | g << 8 | r);
        }

        public static void RgbValToGrayscale(this float[] array)
        {
            for (var i = 0; i < array.Length; i++)
                array[i] = BitConverter.GetBytes((int)array[i])[0];
        }

        public static Color SwapRedBlue(this Color color)
        {
            return Color.FromArgb(color.A, color.B, color.G, color.R);
        }
    }
}
