using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A control that displays a matrix of inner controls aligned in rows and columns.
    /// </summary>
    public class GridControl : Control
    {
        public GridControl(int Rows, int Columns)
        {
            this._Cells = new Control[Rows, Columns];
        }

        /// <summary>
        /// Gets the amount of rows in this grid.
        /// </summary>
        public int Rows
        {
            get
            {
                return this._Cells.GetLength(0);
            }
        }

        /// <summary>
        /// Gets the amount of columns in this grid.
        /// </summary>
        public int Columns
        {
            get
            {
                return this._Cells.GetLength(1);
            }
        }

        /// <summary>
        /// Gets or sets the border that acts as the seperator between cells.
        /// </summary>
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
        public Control this[int Row, int Column]
        {
            get
            {
                return this._Cells[Row, Column];
            }
            set
            {
                this._Cells[Row, Column] = value;
            }
        }

        public override Layout CreateLayout(Rectangle SizeRange, out Point Size)
        {
            int rows = this.Rows;
            int cols = this.Columns;

            double sep = this._Seperator.Weight;
            Rectangle contentsizerange = SizeRange.Translate(-new Point(sep * (rows - 1), sep * (cols - 1)));

            // Create preliminary cell layouts to estimate sizes needed
            Point maxcellsize = contentsizerange.BottomRight;
            double[] widths = new double[rows];
            double[] heights = new double[cols];
            Layout[,] cells = new Layout[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Point size;
                    cells[r, c] = this._Cells[r, c].CreateLayout(new Rectangle(new Point(widths[r], heights[c]), maxcellsize), out size);
                    widths[r] = Math.Max(widths[r], size.X);
                    heights[c] = Math.Max(heights[c], size.Y);
                }
            }
            _AdjustSizes(widths, contentsizerange.Left, contentsizerange.Right);
            _AdjustSizes(heights, contentsizerange.Top, contentsizerange.Bottom);

            // Adjust cells to have the new sizes
            for (int r = 0; r < rows; r++)
            {
                double width = widths[r];
                for (int c = 0; c < cols; c++)
                {
                    double height = heights[c];
                    this._Cells[r, c].UpdateLayout(ref cells[r, c], new Point(width, height));
                }
            }

            // Determine offsets
            double totalwidth;
            double totalheight;
            double[] rowoffsets = _GetOffsets(widths, sep, out totalwidth);
            double[] coloffsets = _GetOffsets(heights, sep, out totalheight);


            Size = new Point(totalwidth, totalheight);
            return new _Layout
            {
                Control = this,
                Cells = cells,
                RowOffsets = rowoffsets,
                ColumnOffsets = coloffsets,
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

        private class _Layout : Layout
        {
            public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
            {
                for (int r = 0; r < this.RowOffsets.Length; r++)
                {
                    double roff = this.RowOffsets[r];
                    for (int c = 0; c < this.ColumnOffsets.Length; c++)
                    {
                        double coff = this.ColumnOffsets[c];
                        this.Cells[r, c].Update(Offset + new Point(roff, coff), Probes, Time);
                    }
                }
            }

            public override void Render(RenderContext Context)
            {
                // Render cells
                for (int r = 0; r < this.RowOffsets.Length; r++)
                {
                    double roff = this.RowOffsets[r];
                    for (int c = 0; c < this.ColumnOffsets.Length; c++)
                    {
                        double coff = this.ColumnOffsets[c];
                        using (Context.Translate(new Point(roff, coff)))
                        {
                            this.Cells[r, c].Render(Context);
                        }
                    }
                }

                // Render seperators
                Border seperator = this.Control.Seperator;
                if (seperator.Color.A > 0.0 && seperator.Weight > 0.0)
                {
                    double hw = seperator.Weight * 0.5;
                    Context.ClearTexture();
                    Context.SetColor(seperator.Color);
                    using (Context.DrawLines(seperator.Weight))
                    {
                        for (int r = 1; r < this.RowOffsets.Length; r++)
                        {
                            double roff = this.RowOffsets[r];
                            Context.OutputLine(new Point(roff - hw, 0.0), new Point(roff - hw, this.Size.Y));
                        }
                        for (int c = 1; c < this.ColumnOffsets.Length; c++)
                        {
                            double coff = this.ColumnOffsets[c];
                            Context.OutputLine(new Point(0.0, coff - hw), new Point(this.Size.X, coff - hw));
                        }
                    }
                }
            }

            public GridControl Control;
            public double[] RowOffsets;
            public double[] ColumnOffsets;
            public Layout[,] Cells;
            public Point Size;
        }

        private Control[,] _Cells;
        private Border _Seperator;
    }
}