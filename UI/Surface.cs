using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// An two-dimensional dynamic object a user can interaction with.
    /// </summary>
    public abstract class Surface
    {
        /// <summary>
        /// Updates the state of the surface for user interaction.
        /// </summary>
        public abstract void Update(UpdateInterface Interface);

        /// <summary>
        /// Gets an image that can be used to display the surface to the user.
        /// </summary>
        public abstract Image Image { get; }

        /// <summary>
        /// Gets the area this surface occupies. The surface will not receive any input from outside this rectangle and can not draw
        /// outside the rectangle (the image outside this rectangle should be transparent).
        /// </summary>
        public virtual Rectangle Bounds
        {
            get
            {
                return Rectangle.Unbound;
            }
        }
    }

    /// <summary>
    /// Collection of methods that allow a surface to receive input from the user over a time interval.
    /// </summary>
    public abstract class UpdateInterface
    {
        /// <summary>
        /// Gets the amount of time that has elapsed during the scope of the interface.
        /// </summary>
        public abstract double Time { get; }

        /// <summary>
        /// Informs the interface that the visual contents of the given area have changed during the update and
        /// need to be redrawn. This may be called at any time for any area but doing so may result in a performance loss.
        /// </summary>
        public abstract void Invalidate(Rectangle Area);
    }

    /// <summary>
    /// Collection of methods that allows a surface to be displayed.
    /// </summary>
    public abstract class RenderInterface
    {
        /// <summary>
        /// Clears the entirety of the renderable area to a single color. 
        /// </summary>
        public abstract void Clear(Color Color);

        /// <summary>
        /// Draws a solid line between two points.
        /// </summary>
        public abstract void Line(Color Color, Point A, Point B, double Thickness);
    }

    /// <summary>
    /// A surface bounded to a variable-sized rectangular area whose top-left point is at the origin, 
    /// with no interactions outside the area.
    /// </summary>
    public abstract class Control : Surface
    {
        /// <summary>
        /// The size of the rectangle bounding the control.
        /// </summary>
        public Point Size;

        public override Rectangle Bounds
        {
            get
            {
                return new Rectangle(new Point(0.0, 0.0), this.Size);
            }
        }
    }

    /// <summary>
    /// A control that displays as a solid color.
    /// </summary>
    public class TestControl : Control
    {
        public override void Update(UpdateInterface Interface)
        {
            
        }

        public override Image Image
        {
            get
            {
                return Image.Solid(Color.RGB(1.0, 0.0, 0.0))
                    .OverDraw(Image.Path(Path.Line(new Point(0.0, 0.0), this.Size), Color.RGB(0.0, 1.0, 1.0), 5.0));
            }
        }
    }
}