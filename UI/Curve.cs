using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace DUIP.UI
{
    /// <summary>
    /// Contains functions related to curves.
    /// </summary>
    public static class Curve
    {
        /// <summary>
        /// Evaluates a cubic bezier curve for the given parameter
        /// </summary>
        public static Point EvaluateBezier(ref Point A, ref Point B, ref Point C, ref Point D, double Parameter)
        {
            double p = Parameter;
            double l = 1.0 - p;
            return 
                A * (l * l * l) + 
                B * (3 * l * l * p) + 
                C * (3 * l * p * p) + 
                D * (p * p * p);
        }

        /// <summary>
        /// Evaluates a quadratic bezier curve for the given parameter
        /// </summary>
        public static Point EvaluateBezier(ref Point A, ref Point B, ref Point C, double Parameter)
        {
            double p = Parameter;
            double l = 1.0 - p;
            return
                A * (l * l) +
                B * (2 * l * p) +
                C * (p * p);
        }
    }
}