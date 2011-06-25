﻿using System;
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
        /// Creates a layout for this control with the preferred size within the given size range.
        /// </summary>
        public abstract Layout CreateLayout(Rectangle SizeRange, out Point Size);

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
        /// Creates a control that applies padding to this control.
        /// </summary>
        public PadControl WithPad(Compass<double> Padding)
        {
            return new PadControl(Padding, this);
        }
    }
}