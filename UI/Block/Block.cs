using System;
using System.Collections.Generic;
using System.Linq;

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
        public Layout CreateLayout(InputContext Context, Point Size)
        {
            return this.CreateLayout(Context, new Rectangle(Size, Size), out Size);
        }

        /// <summary>
        /// Creates a layout for this block with the preferred size within the given size range.
        /// </summary>
        public abstract Layout CreateLayout(InputContext Context, Rectangle SizeRange, out Point Size);

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

        /// <summary>
        /// The allowable difference between sizes and offsets in order for them to be considered equal.
        /// </summary>
        public const double ErrorThreshold = 0.000001;
    }

    /// <summary>
    /// A particular spatial arrangement of elements within a block.
    /// </summary>
    public abstract class Layout
    {
        /// <summary>
        /// Renders the layout to the given render context.
        /// </summary>
        public virtual void Render(RenderContext Context)
        {

        }

        /// <summary>
        /// Registers a callback for when the layout is made invalid due to a change in the associated block.
        /// </summary>
        public virtual RemoveHandler RegisterInvalidate(Action Callback)
        {
            return null;
        }
    }

    /// <summary>
    /// Marks a property of a block as static. This indicates that the property will not change (and may not be modified) once the block is in use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class StaticProperty : Attribute
    {

    }

    /// <summary>
    /// Marks a property of a block as dynamic. This indicates that the property may changed even when the block is in use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DynamicProperty : Attribute
    {

    }
}