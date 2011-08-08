using System;
using System.Collections.Generic;
using System.Linq;

using DUIP.UI.Graphics;

namespace DUIP.UI
{
    /// <summary>
    /// A block that displays a matrix of inner blocks aligned in rows and columns.
    /// </summary>
    public class GridBlock : Block
    {
        public GridBlock(Block[,] Cells, Border Seperator)
        {
            this.Cells = Cells;
            this.Seperator = Seperator;
        }

        /// <summary>
        /// Gets the amount of columns in this grid.
        /// </summary>
        public int Columns
        {
            get
            {
                return this.Cells.GetLength(0);
            }
        }

        /// <summary>
        /// Gets the amount of rows in this grid.
        /// </summary>
        public int Rows
        {
            get
            {
                return this.Cells.GetLength(1);
            }
        }

        /// <summary>
        /// The (immutable) contents of the cells for this grid block.
        /// </summary>
        public readonly Block[,] Cells;

        /// <summary>
        /// The border to use as the seperator between cells.
        /// </summary>
        public readonly Border Seperator;

        /// <summary>
        /// Gets a cell in this grid.
        /// </summary>
        public Block this[int Column, int Row]
        {
            get
            {
                return this.Cells[Column, Row];
            }
        }

        public override Layout CreateLayout(Context Context, Rectangle SizeRange, out Point Size)
        {
            int cols = this.Columns;
            int rows = this.Rows;

            double sep = this.Seperator.Weight;
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
                    cells[c, r] = this.Cells[c, r].CreateLayout(null, new Rectangle(new Point(widths[c], heights[r]), maxcellsize), out size);
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
                    cells[c, r] = this.Cells[c, r].CreateLayout(null, new Point(width, height));
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

        private class _Layout : Layout
        {
            public override RemoveHandler Link(Context Context)
            {
                RemoveHandler rh = null;
                for (int c = 0; c < this.ColumnOffsets.Length; c++)
                {
                    double coff = this.ColumnOffsets[c];
                    for (int r = 0; r < this.RowOffsets.Length; r++)
                    {
                        double roff = this.RowOffsets[r];
                        rh += this.Cells[c, r].Link(Context.Translate(new Point(-coff, -roff)));
                    }
                }
                return rh;
            }

            public override Figure Figure
            {
                get
                {
                    // Cells
                    List<Figure> cellfigures = new List<Figure>();
                    for (int c = 0; c < this.ColumnOffsets.Length; c++)
                    {
                        double coff = this.ColumnOffsets[c];
                        for (int r = 0; r < this.RowOffsets.Length; r++)
                        {
                            double roff = this.RowOffsets[r];
                            cellfigures.Add(Figure.Translate(this.Cells[c, r].Figure, new Point(coff, roff)));
                        }
                    }
                    Figure fig = new CompoundFigure(cellfigures);

                    // Seperators
                    Border seperator = this.Block.Seperator;
                    if (seperator.Color.A > 0.0 && seperator.Weight > 0.0)
                    {
                        double w = seperator.Weight;
                        double hw = seperator.Weight * 0.5;
                        SolidFigure mask = new SolidFigure(seperator.Color);
                        for (int c = 1; c < this.ColumnOffsets.Length; c++)
                        {
                            double coff = this.ColumnOffsets[c];
                            fig += new ShapeFigure(
                                new PathShape(w, new SegmentPath(
                                    new Point(coff - hw, 0.0),
                                    new Point(coff - hw, this.Size.Y))),
                                mask);
                        }
                        for (int r = 1; r < this.RowOffsets.Length; r++)
                        {
                            double roff = this.RowOffsets[r];
                            fig += new ShapeFigure(
                                new PathShape(w, new SegmentPath(
                                    new Point(0.0, roff - hw),
                                    new Point(this.Size.X, roff - hw))),
                                mask);
                        }
                    }

                    return fig;
                }
            }

            public override RemoveHandler RegisterFigureChange(Action Callback)
            {
                RemoveHandler rh = null;
                foreach (Layout l in this.Cells)
                {
                    rh += l.RegisterFigureChange(Callback);
                }
                return rh;
            }

            public GridBlock Block;
            public double[] RowOffsets;
            public double[] ColumnOffsets;
            public Layout[,] Cells;
            public Point Size;
        }
    }
}