﻿using System;
using System.Collections.Generic;

namespace DUIP.UI
{
    /// <summary>
    /// Represents a color including alpha. Contains methods for color manipulation.
    /// </summary>
    public struct Color
    {

        /// <summary>
        /// Creates a color from its RGBA representation. Values should be between
        /// 0.0 and 1.0.
        /// </summary>
        public static Color RGBA(double R, double G, double B, double A)
        {
            Color c = new Color();
            c.R = R;
            c.G = G;
            c.B = B;
            c.A = A;
            return c;
        }

        /// <summary>
        /// Creates a color from its RGB reprsentation with a completely
        /// opaque alpha.
        /// </summary>
        public static Color RGB(double R, double G, double B)
        {
            return RGBA(R, G, B, 1.0);
        }

        public static Color Alpha(double A)
        {
            return RGBA(1.0, 1.0, 1.0, A);
        }

        /// <summary>
        /// Mixes two colors based on the specified amount. If the amount is 0.0,
        /// the resulting color will be A. If the amount is 1.0, the resulting color
        /// will be B. Values in between will cause the color to be interpolated.
        /// </summary>
        public static Color Mix(Color A, Color B, double Amount)
        {
            double rd = B.R - A.R;
            double gd = B.G - A.G;
            double bd = B.B - A.B;
            double ad = B.A - A.A;
            return RGBA(
                A.R + (rd * Amount),
                A.G + (gd * Amount),
                A.B + (bd * Amount),
                A.A + (ad * Amount));
        }

        /// <summary>
        /// Creates a color from its HLSA representation.
        /// </summary>
        /// <param name="H">Hue in degrees.</param>
        /// <param name="L">Lumination between 0.0 and 1.0.</param>
        /// <param name="S">Saturation between 0.0 and 1.0.</param>
        /// <param name="A">Alpha between 0.0 and 1.0.</param>
        public static Color HLSA(double H, double L, double S, double A)
        {
            // Find color based on hue.
            H = H % 360.0;
            double delta = (H % 60.0) / 60.0;
            Color hue = RGB(1.0, 0.0, 0.0);
            if (H < 60) hue = RGB(1.0, delta, 0.0);
            else if (H < 120) hue = RGB(1.0 - delta, 1.0, 0.0);
            else if (H < 180) hue = RGB(0.0, 1.0, delta);
            else if (H < 240) hue = RGB(0.0, 1.0 - delta, 1.0);
            else if (H < 300) hue = RGB(delta, 0.0, 1.0);
            else if (H < 360) hue = RGB(1.0, 0.0, 1.0 - delta);

            // Saturation
            Color sat = Mix(hue, RGB(0.5, 0.5, 0.5), 1.0 - S);

            // Lumination
            Color lum = sat;
            if (L > 0.5)
            {
                lum = Mix(lum, RGB(1.0, 1.0, 1.0), (L - 0.5) * 2.0);
            }
            else
            {
                lum = Mix(lum, RGB(0.0, 0.0, 0.0), (0.5 - L) * 2.0);
            }

            // Alpha
            lum.A = A;
            return lum;
        }

        public static implicit operator System.Drawing.Color(Color Color)
        {
            return System.Drawing.Color.FromArgb(
                (int)(Color.A * 255.0),
                (int)(Color.R * 255.0),
                (int)(Color.G * 255.0),
                (int)(Color.B * 255.0));
        }

        /// <summary>
        /// Brings all color units to the specified value in the color.
        /// </summary>
        public Color Desaturate(double Min, double Max)
        {
            double dis = Max - Min;
            return Color.RGBA(dis * this.R + Min, dis * this.G + Min, dis * this.B + Min, this.A);
        }

        /// <summary>
        /// Gets a completely transparent color.
        /// </summary>
        public static readonly Color Transparent = RGBA(0.0, 0.0, 0.0, 0.0);

        public double R;
        public double G;
        public double B;
        public double A;
    }
}