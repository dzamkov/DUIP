using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A block that displays a matrix of inner blocks aligned in rows and columns.
    /// </summary>
    public class GridBlock : Block
    {
        public GridBlock(int Columns, int Rows)
        {
            this._Cells = new Block[Columns, Rows];
        }

        /// <summary>
        /// Gets the amount of columns in this grid.
        /// </summary>
        [StaticProperty]
        public int Columns
        {
            get
            {
                return this._Cells.GetLength(0);
            }
        }

        /// <summary>
        /// Gets the amount of rows in this grid.
        /// </summary>
        [StaticProperty]
        public int Rows
        {
            get
            {
                return this._Cells.GetLength(1);
            }
        }

        /// <summary>
        /// Gets or sets the border that acts as the seperator between cells.
        /// </summary>
        [StaticProperty]
        public Border Seperator
        {
            get
            {
                return this._Seperator;
            }
            set
            {
                this._Seperator = value;
            }
        }

        /// <summary>
        /// Gets or sets a cell in this grid.
        /// </summary>
        [StaticProperty]
        public Block this[int Column, int Row]
        {
            get
            {
                return this._Cells[Column, Row];
            }
            set
            {
                this._Cells[Column, Row] = value;
            }
        }

        public override Layout CreateLayout(Rectangle SizeRange, out Point Size)
        {
            int cols = this.Columns;
            int rows = this.Rows;

            double sep = this._Seperator.Weight;
            Rectangle contentsizerange = SizeRange.Translate(-new Point(sep * (cols - 1), sep * (rows - 1)));

            // Create preliminary cell layouts to estimate sizes needed
            Point maxcellsize = contentsizerange.BottomRight;
            double[] widths = new double[cols];
            double[] heights = new double[rows];
            Layout[,] cells = new Layout[cols, rows];
            for (int c = 0; c < cols; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    Point size;
                    cells[c, r] = this._Cells[c, r].CreateLayout(new Rectangle(new Point(widths[c], heights[r]), maxcellsize), out size);
                    widths[c] = Math.Max(widths[c], size.X);
                    heights[r] = Math.Max(heights[r], size.Y);
                }
            }
            _AdjustSizes(widths, contentsizerange.Left, contentsizerange.Right);
            _AdjustSizes(heights, contentsizerange.Top, contentsizerange.Bottom);

            // Adjust cells to have the new sizes
            for (int c = 0; c < cols; c++)
            {
                double width = widths[c];
                for (int r = 0; r < rows; r++)
                {
                    double height = heights[r];
                    this._Cells[c, r].UpdateLayout(ref cells[c, r], new Point(width, height));
                }
            }

            // Determine offsets
            double totalwidth;
            double totalheight;
            double[] coloffsets = _GetOffsets(widths, sep, out totalwidth);
            double[] rowoffsets = _GetOffsets(heights, sep, out totalheight);


            Size = new Point(totalwidth, totalheight);
            return new _Layout
            {
                Block = this,
                Cells = cells,
                ColumnOffsets = coloffsets,
                RowOffsets = rowoffsets,
                Size = Size
            };
        }

        /// <summary>
        /// Adjusts the given array of sizes so that the total is within the range specified.
        /// </summary>
        private static void _AdjustSizes(double[] Sizes, double Min, double Max)
        {
            double total = 0.0;
            for (int t = 0; t < Sizes.Length; t++)
            {
                total += Sizes[t];
            }

            if (total < Min)
            {
                double e = (Min - total) / Sizes.Length;
                for (int t = 0; t < Sizes.Length; t++)
                {
                    Sizes[t] += e;
                }
                return;
            }

            if (total > Max)
            {
                double e = (total - Max) / Sizes.Length;
                for (int t = 0; t < Sizes.Length; t++)
                {
                    Sizes[t] -= e;
                }
                return;
            }
        }

        /// <summary>
        /// Destructively gets the offsets of the spans in one direction, and returns the total size of the grid on that axis.
        /// </summary>
        private static double[] _GetOffsets(double[] Sizes, double SeperatorSize, out double TotalSize)
        {
            double off = 0.0;
            TotalSize = 0.0;
            for (int t = 0; t < Sizes.Length; t++)
            {
                TotalSize = off + Sizes[t];
                Sizes[t] = off;
                off = TotalSize + SeperatorSize;
            }
            return Sizes;
        }

        public override event Action<Block> LayoutUpdate
        {
            add
            {
                foreach (Block block in this._Cells)
                {
                    block.LayoutUpdate += value;
                }
            }
            remove
            {
                foreach (Block block in this._Cells)
                {
                    block.LayoutUpdate -= value;
                }
            }
        }

        private class _Layout : Layout
        {
            public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
            {
                for (int c = 0; c < this.ColumnOffsets.Length; c++)
                {
                    double coff = this.ColumnOffsets[c];
                    for (int r = 0; r < this.RowOffsets.Length; r++)
                    {
                        double roff = this.RowOffsets[r];
                        this.Cells[c, r].Update(Offset + new Point(coff, roff), Probes, Time);
                    }
                }
            }

            public override void Render(RenderContext Context)
            {
                // Render cells
                for (int c = 0; c < this.ColumnOffsets.Length; c++)
                {
                    double coff = this.ColumnOffsets[c];
                    for (int r = 0; r < this.RowOffsets.Length; r++)
                    {
                        double roff = this.RowOffsets[r];
                        using (Context.Translate(new Point(coff, roff)))
                        {
                            this.Cells[c, r].Render(Context);
                        }
                    }
                }

                // Render seperators
                Border seperator = this.Block.Seperator;
                if (seperator.Color.A > 0.0 && seperator.Weight > 0.0)
                {
                    double hw = seperator.Weight * 0.5;
                    Context.ClearTexture();
                    Context.SetColor(seperator.Color);
                    using (Context.DrawLines(seperator.Weight))
                    {
                        for (int c = 1; c < this.ColumnOffsets.Length; c++)
                        {
                            double coff = this.ColumnOffsets[c];
                            Context.OutputLine(new Point(coff - hw, 0.0), new Point(coff - hw, this.Size.Y));
                        }
                        for (int r = 1; r < this.RowOffsets.Length; r++)
                        {
                            double roff = this.RowOffsets[r];
                            Context.OutputLine(new Point(0.0, roff - hw), new Point(this.Size.X, roff - hw));
                        }
                    }
                }
            }

            public GridBlock Block;
            public double[] RowOffsets;
            public double[] ColumnOffsets;
            public Layout[,] Cells;
            public Point Size;
        }

        private Block[,] _Cells;
        private Border _Seperator;
    }
}