using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A block that shows a variable-sized table of blocks seperated by borders.
    /// </summary>
    public class GridBlock : Block
    {
        public GridBlock(int Width, int Height)
        {
            this._Cells = new Block[Width, Height];
        }

        public GridBlock(int Width, int Height, Border Seperator)
            : this(Width, Height)
        {
            this._Seperator = Seperator;
        }

        /// <summary>
        /// Gets or sets the contents of a cell in the grid.
        /// </summary>
        public Block this[int X, int Y]
        {
            get
            {
                return this._Cells[X, Y];
            }
            set
            {
                this._Cells[X, Y] = value;
            }
        }

        /// <summary>
        /// Gets the amount of columns in the grid.
        /// </summary>
        public int Width
        {
            get
            {
                return this._Cells.GetLength(0);
            }
        }

        /// <summary>
        /// Gets the amount of rows in the grid.
        /// </summary>
        public int Height
        {
            get
            {
                return this._Cells.GetLength(1);
            }
        }

        /// <summary>
        /// Gets or sets the border used to seperate cells.
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

        public override Disposable<Control> CreateControl(Rectangle SizeRange)
        {
            return new GridControl(this, SizeRange);
        }

        private Block[,] _Cells;
        private Border _Seperator;
    }

    /// <summary>
    /// A control for a grid block.
    /// </summary>
    public class GridControl : Control, IDisposable
    {
        public GridControl(GridBlock Block, Rectangle SizeRange)
        {
            this._Block = Block;

            int width = Block.Width;
            int height = Block.Height;
            this._Cells = new Disposable<Control>[width, height];
            Rectangle cellsizerange = new Rectangle(Point.Zero, SizeRange.BottomRight);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    this._Cells[x, y] = (this._Block[x, y] ?? SpaceBlock.Singleton).CreateControl(cellsizerange);
                }
            }
            this._XOffsets = new double[width];
            this._YOffsets = new double[height];
            this._LayoutCells(width, height, SizeRange);
        }

        public override Point Size
        {
            get
            {
                return this._Size;
            }
        }

        public override Rectangle SizeRange
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the amount of columns in the grid.
        /// </summary>
        public int Width
        {
            get
            {
                return this._Cells.GetLength(0);
            }
        }

        /// <summary>
        /// Gets the amount of rows in the grid.
        /// </summary>
        public int Height
        {
            get
            {
                return this._Cells.GetLength(1);
            }
        }

        /// <summary>
        /// Arranges the cells on the grid and sets the size of the grid.
        /// </summary>
        private void _LayoutCells(int Width, int Height, Rectangle SizeRange)
        {
            double sep = this._Block.Seperator.Weight;
            double wseps = sep * (Width - 1);
            double hseps = sep * (Height - 1);
            
            // Calculate widths
            double[] widths = new double[Width];
            for (int x = 0; x < Width; x++)
            {
                double minwidth = 0.0;
                for (int y = 0; y < Height; y++)
                {
                    minwidth = Math.Max(minwidth, this._Cells[x, y].Object.Size.X);
                }
                widths[x] = minwidth;
            }
            _AdjustSpan(widths, SizeRange.Left - wseps, SizeRange.Right - wseps);

            // Calculate heights
            double[] heights = new double[Height];
            for (int y = 0; y < Height; y++)
            {
                double minheight = 0.0;
                for (int x = 0; x < Width; x++)
                {
                    minheight = Math.Max(minheight, this._Cells[x, y].Object.Size.Y);
                }
                heights[y] = minheight;
            }
            _AdjustSpan(heights, SizeRange.Top - wseps, SizeRange.Bottom - wseps);

            // Calculate size and offsets
            double xoffset = 0.0;
            double xtotalsize = 0.0;
            for (int x = 0; x < Width; x++)
            {
                this._XOffsets[x] = xoffset;
                xtotalsize = xoffset + widths[x];
                xoffset = xtotalsize + sep;
            }
            double yoffset = 0.0;
            double ytotalsize = 0.0;
            for (int y = 0; y < Height; y++)
            {
                this._YOffsets[y] = yoffset;
                ytotalsize = yoffset + heights[y];
                yoffset = ytotalsize + sep;
            }
            this._Size = new Point(xtotalsize, ytotalsize);

            // Resize controls
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Point size = new Point(widths[x], heights[y]);
                    this._Cells[x, y].Object.SizeRange = new Rectangle(size, size);
                }
            }
        }

        /// <summary>
        /// Adjusts the sizes of items within a span given the minimum and maximum allowable total sizes for the span.
        /// </summary>
        private void _AdjustSpan(double[] Sizes, double MinTotalSize, double MaxTotalSize)
        {
            double total = 0.0;
            for (int t = 0; t < Sizes.Length; t++)
            {
                total += Sizes[t];
            }

            if (total < MinTotalSize)
            {
                double em = (MinTotalSize - total) / Sizes.Length;
                for (int t = 0; t < Size.Length; t++)
                {
                    Sizes[t] += em;
                }
            }

            if (total > MaxTotalSize)
            {
                double em = (total - MaxTotalSize) / Sizes.Length;
                for (int t = 0; t < Size.Length; t++)
                {
                    Sizes[t] -= em;
                }
            }
        }

        public override void Update(Point Offset, IEnumerable<Probe> Probes, double Time)
        {
            for (int x = 0; x < this.Width; x++)
            {
                double xoffset = this._XOffsets[x];
                for (int y = 0; y < this.Height; y++)
                {
                    double yoffset = this._YOffsets[y];
                    this._Cells[x, y].Object.Update(Offset + new Point(xoffset, yoffset), Probes, Time);
                }
            }
        }

        public override void Render(RenderContext Context)
        {
            // Render cells
            for (int x = 0; x < this.Width; x++)
            {
                double xoffset = this._XOffsets[x];
                for (int y = 0; y < this.Height; y++)
                {
                    double yoffset = this._YOffsets[y];
                    using (Context.Translate(new Point(xoffset, yoffset)))
                    {
                        this._Cells[x, y].Object.Render(Context);
                    }
                }
            }

            // Render seperators
            Border seperator = this._Block.Seperator;
            Context.ClearTexture();
            Context.SetColor(seperator.Color);
            double hw = seperator.Weight * 0.5;
            using (Context.DrawLines(seperator.Weight))
            {
                for (int x = 1; x < this._XOffsets.Length; x++)
                {
                    double off = this._XOffsets[x] - hw;
                    Context.OutputLine(new Point(off, 0.0), new Point(off, this._Size.Y));
                }
                for (int y = 1; y < this._YOffsets.Length; y++)
                {
                    double off = this._YOffsets[y] - hw;
                    Context.OutputLine(new Point(0.0, off), new Point(this._Size.X, off));
                }
            }
        }

        public void Dispose()
        {
            foreach (Disposable<Control> con in this._Cells)
            {
                con.Dispose();
            }
        }

        private Disposable<Control>[,] _Cells;
        private double[] _XOffsets;
        private double[] _YOffsets;
        private Point _Size;
        private GridBlock _Block;
    }
}