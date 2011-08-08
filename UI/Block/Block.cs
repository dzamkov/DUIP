using System;
using System.Collections.Generic;
using System.Linq;

using DUIP.UI.Graphics;

namespace DUIP.UI
{
    /// <summary>
    /// A dynamic, visual object users can interact with.
    /// </summary>
    /// <remarks>Blocks are not displayed and updated directly, but rather must use a layout,
    /// which is a spatial arrangement of elements within a block.</remarks>
    public abstract class Block
    {
        /// <summary>
        /// Creates a layout for this block with the given size.
        /// </summary>
        public Layout CreateLayout(Context Context, Point Size)
        {
            return this.CreateLayout(Context, new Rectangle(Size, Size), out Size);
        }

        /// <summary>
        /// Creates a layout for this block with the preferred size within the given size range.
        /// </summary>
        public abstract Layout CreateLayout(Context Context, Rectangle SizeRange, out Point Size);

        /// <summary>
        /// Creates a block that applies a border to this block.
        /// </summary>
        public BorderBlock WithBorder(Border Border)
        {
            return new BorderBlock(Border, this);
        }

        /// <summary>
        /// Creates a block that applies a background to this block.
        /// </summary>
        public BackgroundBlock WithBackground(Color Background)
        {
            return new BackgroundBlock(Background, this);
        }

        /// <summary>
        /// Creates a block that limits the size range available to this block.
        /// </summary>
        public SizeBlock WithSize(Rectangle LimitSizeRange)
        {
            return new SizeBlock(LimitSizeRange, this);
        }

        /// <summary>
        /// Creates a block that limits the size range available to this block.
        /// </summary>
        public SizeBlock WithSize(Point Size)
        {
            return new SizeBlock(new Rectangle(Size, Size), this);
        }

        /// <summary>
        /// Creates a block that limits the size range available to this block.
        /// </summary>
        public SizeBlock WithSize(double Width, double Height)
        {
            return this.WithSize(new Point(Width, Height));
        }

        /// <summary>
        /// Creates a block that limits the width range available to this block.
        /// </summary>
        public SizeBlock WithWidth(double Min, double Max)
        {
            return this.WithSize(new Rectangle(Min, 0.0, Max, double.PositiveInfinity));
        }

        /// <summary>
        /// Creates a block that limits the width range available to this block.
        /// </summary>
        public SizeBlock WithWidth(double Width)
        {
            return this.WithWidth(Width, Width);
        }

        /// <summary>
        /// Creates a block that limits the height range available to this block.
        /// </summary>
        public SizeBlock WithHeight(double Min, double Max)
        {
            return this.WithSize(new Rectangle(0.0, Min, double.PositiveInfinity, Max));
        }

        /// <summary>
        /// Creates a block that limits the height range available to this block.
        /// </summary>
        public SizeBlock WithHeight(double Height)
        {
            return this.WithHeight(Height, Height);
        }

        /// <summary>
        /// Creates a block that applies a margin to this block.
        /// </summary>
        public MarginBlock WithMargin(Compass<double> Margin)
        {
            return new MarginBlock(Margin, this);
        }
    }

    /// <summary>
    /// A particular spatial arrangement of elements within a block.
    /// </summary>
    public abstract class Layout
    {
        /// <summary>
        /// Links this layout to an input context and returns a remove handler to later unlink it. Only one layout for each block may be linked
        /// at one time.
        /// </summary>
        /// <remarks>The input context given is not restricted to the area of the layout, and may reference probes that are outside the layout.
        /// The input context will give positions relative to the layout with (0.0, 0.0) being the top-left corner with ascending
        /// positions going towards the bottom-right.</remarks>
        public virtual RemoveHandler Link(Context Context)
        {
            return null;
        }

        /// <summary>
        /// Gets a figure to draw this layout in its current state.
        /// </summary>
        public virtual Figure Figure
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Registers a callback to be called when the figure for this layout has changed.
        /// </summary>
        public virtual RemoveHandler RegisterFigureChange(Action Callback)
        {
            return null;
        }

        /// <summary>
        /// The allowable difference between sizes and offsets in order for them to be considered equal.
        /// </summary>
        public const double ErrorThreshold = 0.000001;
    }
}