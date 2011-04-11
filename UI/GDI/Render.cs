using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading;

namespace DUIP.UI.GDI
{
    /// <summary>
    /// Contains functions and methods related to .Net's GDI+ library and the UI system.
    /// </summary>
    public static class GDI
    {
        /// <summary>
        /// Renders an image to the given graphics device.
        /// </summary>
        public static void Render(Graphics Graphics, Image Image)
        {
            SolidImage si = Image as SolidImage;
            if (si != null)
            {
                Graphics.Clear(si.Color);
                return;
            }

            OverDrawImage odi = Image as OverDrawImage;
            if (odi != null)
            {
                Render(Graphics, odi.Under);
                RenderOver(Graphics, odi.Over);
                return;
            }

            Graphics.Clear(System.Drawing.Color.Transparent);
            RenderOver(Graphics, Image);
        }

        /// <summary>
        /// Renders an image over the current image in the given graphics device.
        /// </summary>
        public static void RenderOver(Graphics Graphics, Image Image)
        {
            PathImage pi = Image as PathImage;
            if (pi != null)
            {
                RenderPathOver(Graphics, pi.Path, pi.Style);
                return;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Renders a path to the given graphics context.
        /// </summary>
        public static void RenderPathOver(Graphics Graphics, Path Path, StrokeStyle Stroke)
        {
            LinePath lp = Path as LinePath;
            if (lp != null)
            {
                using (Pen p = MakePen(Stroke))
                {
                    Point s = lp.Start;
                    Point e = lp.End;
                    Graphics.DrawLine(p,
                        new PointF((float)s.X, (float)s.Y),
                        new PointF((float)e.X, (float)e.Y));
                }
                return;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a pen for the given stroke style. This pen must later be disposed.
        /// </summary>
        public static Pen MakePen(StrokeStyle Stroke)
        {
            SolidStrokeStyle sss = Stroke as SolidStrokeStyle;
            if (sss != null)
            {
                return new Pen(sss.Color, (float)sss.Thickness);
            }

            throw new NotImplementedException();
        }
    }
}