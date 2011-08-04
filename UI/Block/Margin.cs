﻿using System;
using System.Collections.Generic;
using System.Linq;

using DUIP.UI.Graphics;

namespace DUIP.UI
{
    /// <summary>
    /// A block that applies a margin to an inner block.
    /// </summary>
    public class MarginBlock : Block, IDisposable
    {
        public MarginBlock()
        {

        }

        public MarginBlock(Compass<double> Margin, Disposable<Block> Inner)
        {
            this._Margin = Margin;
            this._Inner = Inner;
        }

        /// <summary>
        /// Gets or sets the size of the margin applied by this block.
        /// </summary>
        [StaticProperty]
        public Compass<double> Margin
        {
            get
            {
                return this._Margin;
            }
            set
            {
                this._Margin = value;
            }
        }

        /// <summary>
        /// Gets or sets the inner block for this margin block.
        /// </summary>
        [StaticProperty]
        public Block Inner
        {
            get
            {
                return this._Inner;
            }
            set
            {
                this._Inner = value;
            }
        }

        public override Layout CreateLayout(Context Context, Rectangle SizeRange, out Point Size)
        {
            Compass<double> margin = this._Margin;
            Point sizepadding = new Point(margin.Left + margin.Right, margin.Up + margin.Down);
            Layout inner = this.Inner.CreateLayout(null, SizeRange.Translate(-sizepadding), out Size);
            Size += sizepadding;
            return new _Layout
            {
                Offset = new Point(margin.Left, margin.Up),
                Inner = inner
            };
        }

        private class _Layout : Layout
        {
            public override RemoveHandler Link(Context Context)
            {
                return this.Inner.Link(Context.Translate(-this.Offset));
            }

            public override Figure Figure
            {
                get
                {
                    return this.Inner.Figure.Translate(this.Offset);
                }
            }

            public override RemoveHandler RegisterFigureChange(Action Callback)
            {
                return this.Inner.RegisterFigureChange(Callback);
            }

            public Point Offset;
            public Layout Inner;
        }

        public void Dispose()
        {
            this._Inner.Dispose();
        }

        private Compass<double> _Margin;
        private Disposable<Block> _Inner;
    }
}