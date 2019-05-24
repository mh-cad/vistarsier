using System;
using System.Drawing;

namespace VisTarsier.NiftiLib
{
    public class ColorMaps
    {
        /// <summary>
        /// Standard 256 shade greyscale from dark to light.
        /// </summary>
        /// <returns></returns>
        public static Color[] GreyScale()
        {
            Color[] colors = new Color[256];
            for (int i = 0; i < colors.Length; ++i)
            {
                colors[i] = Color.FromArgb(i, i, i);
            }

            return colors;
        }

        /// <summary>
        /// 256 Shade redscale from 0 alpha to bright red.
        /// </summary>
        /// <returns></returns>
        public static Color[] RedScale()
        {
            Color[] colors = new Color[256];

            for (int i = 0; i < colors.Length; ++i)
            {
                double val = i / 16.0;
                val *= val;
                val += 100;
                val = Math.Min(255, val);
                var logval = 255 * (Math.Log(i) / Math.Log(255));
                logval = Math.Min(255, logval);
                if (logval < 0) logval = 0;
                colors[i] = Color.FromArgb((int)logval, 255, (int)val, 0);
            }

            return colors;
        }

        /// <summary>
        /// 256 Shade greenscale with lower alpha value as scale increases.
        /// </summary>
        /// <returns></returns>
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
    }
}
