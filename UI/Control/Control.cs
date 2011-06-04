using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A dynamic, visual object users can interact with.
    /// </summary>
    /// <remarks>Controls are not displayed and updated directly, but rather must use a layout,
    /// which is a spatial arrangement of elements within a control.</remarks>
    public abstract class Control
    {
        /// <summary>
        /// Creates a layout with the given size.
        /// </summary>
        public virtual Layout CreateLayout(Point Size)
        {
            return this.CreateLayout(new Rectangle(Size, Size), out Size);
        }

        /// <summary>
        /// Creates a layout for this control with the preferred size within the given size range.
        /// </summary>
        public abstract Layout CreateLayout(Rectangle SizeRange, out Point Size);

        /// <summary>
        /// Destructively updates the given layout (of this control) with the given size. The new layout will be equivalent 
        /// to the one produced with "CreateLayout".
        /// </summary>
        public virtual void UpdateLayout(ref Layout Layout, Point Size)
        {
            Layout = CreateLayout(Size);
        }

        /// <summary>
        /// Destructively updates the given layout (of this control) with the given size range. The new layout will be equivalent 
        /// to the one produced with "CreateLayout".
        /// </summary>
        public virtual void UpdateLayout(ref Layout Layout, Rectangle SizeRange, out Point Size)
        {
            Layout = CreateLayout(SizeRange, out Size);
        }

        /// <summary>
        /// A particular spatial arrangement of elements within a control.
        /// </summary>
        public abstract class Layout
        {
            /// <summary>
            /// Updates the state of the control this layout is for by the given amount of time while receiving input from probes. Note that this
            /// does not change the state of the layout itself.
            /// </summary>
            /// <param name="Offset">The offset of the control from the probes.</param>
            /// <param name="Probes">The probes that affect the control.</param>
            public virtual void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
            {

            }

            /// <summary>
            /// Renders the control (using the layout) to the given render context.
            /// </summary>
            public virtual void Render(RenderContext Context)
            {

            }
        }

        /// <summary>
        /// Creates a control that applies a border to this control.
        /// </summary>
        public BorderControl WithBorder(Border Border)
        {
            return new BorderControl(Border, this);
        }

        /// <summary>
        /// Creates a control that applies a background to this control.
        /// </summary>
        public BackgroundControl WithBackground(Color Background)
        {
            return new BackgroundControl(Background, this);
        }

        /// <summary>
        /// Creates a control that limits the size range available to this control.
        /// </summary>
        public SizeControl WithSize(Rectangle LimitSizeRange)
        {
            return new SizeControl(LimitSizeRange, this);
        }

        /// <summary>
        /// Creates a control that limits the size range available to this control.
        /// </summary>
        public SizeControl WithSize(Point Size)
        {
            return new SizeControl(new Rectangle(Size, Size), this);
        }

        /// <summary>
        /// Creates a control that limits the size range available to this control.
        /// </summary>
        public SizeControl WithSize(double Width, double Height)
        {
            return this.WithSize(new Point(Width, Height));
        }

        /// <summary>
        /// Creates a control that limits the width range available to this control.
        /// </summary>
        public SizeControl WithWidth(double Min, double Max)
        {
            return this.WithSize(new Rectangle(Min, 0.0, Max, double.PositiveInfinity));
        }

        /// <summary>
        /// Creates a control that limits the width range available to this control.
        /// </summary>
        public SizeControl WithWidth(double Width)
        {
            return this.WithWidth(Width, Width);
        }

        /// <summary>
        /// Creates a control that limits the height range available to this control.
        /// </summary>
        public SizeControl WithHeight(double Min, double Max)
        {
            return this.WithSize(new Rectangle(0.0, Min, double.PositiveInfinity, Max));
        }

        /// <summary>
        /// Creates a control that limits the height range available to this control.
        /// </summary>
        public SizeControl WithHeight(double Height)
        {
            return this.WithHeight(Height, Height);
        }

        /// <summary>
        /// Creates a control that applies padding to this control.
        /// </summary>
        public PadControl WithPad(Compass<double> Padding)
        {
            return new PadControl(Padding, this);
        }
    }
}