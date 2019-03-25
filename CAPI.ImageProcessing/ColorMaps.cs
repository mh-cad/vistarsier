using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPI.ImageProcessing
{
    public class ColorMaps
    {
        public static Color[] GreyScale()
        {
            Color[] colors = new Color[256];
            for (int i = 0; i < colors.Length; ++i)
            {
                colors[i] = Color.FromArgb(i, i, i);
            }

            return colors;
        }

        public static Color[] RedScale()
        {
            Color[] colors = new Color[256];

            for (int i = 0; i < colors.Length; ++i)
            {
                double val = i / 16.0;
                val *= val;
                val = Math.Min(255, val);
                var logval = 255 * (Math.Log(i) / Math.Log(255));
                logval = Math.Min(255, logval);
                if (logval < 0) logval = 0;
                colors[i] = Color.FromArgb((int)logval, 255, (int)val/2, 0);
            }

            return colors;
        }

        public static Color[] GreenScale()
        {
            Color[] colors = new Color[256];

            for (int i = 0; i < colors.Length; ++i)
            {
                colors[i] = Color.FromArgb(0, i, 0);
            }

            return colors;
        }

        public static Color[] ReverseGreenScale()
        {
            Color[] colors = new Color[256];

            for (int i = 0; i < colors.Length; ++i)
            {
                double val = i / 16.0;
                val *= val;
                val = Math.Min(255, val);
                var logval = 255 * (Math.Log(i) / Math.Log(255));
                logval = Math.Min(255, logval);
                if (logval < 0) logval = 0;
                colors[i] = Color.FromArgb(255 - (int)val, 128 - (int)logval/2, 255, 0);
            }

            return colors;
        }

        public static Color[] GreenMask()
        {
            Color[] colors = { Color.Black, Color.FromArgb(55, 255, 0) };
            return colors;
        }
    }
}
