using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.GUI
{
    /// <summary>
    /// A figure that creates a tileable randomly-generated cellular pattern.
    /// </summary>
    public class CellularFigure : Figure
    {
        public CellularFigure(Figure Cell, Figure Edge, double Exponent, double Multiplier, IEnumerable<Point> CellCenters)
        {
            this._Centers = new List<Point>();
            this._Cell = Cell;
            this._Edge = Edge;
            this._Exponent = Exponent;
            this._Multiplier = Multiplier;
            foreach (Point cen in CellCenters)
            {
                this._Centers.Add(cen);
            }
        }

        /// <summary>
        /// Creates a random distribution of points.
        /// </summary>
        public static IEnumerable<Point> RandomDistribution(Random Random, int Amount)
        {
            while (Amount > 0)
            {
                yield return new Point(Random.NextDouble(), Random.NextDouble());
                Amount--;
            }
        }

        /// <summary>
        /// Creates a grid-like distribution of points.
        /// </summary>
        /// <param name="Size">The edge-length of the grid.</param>
        /// <param name="Error">The amount each grid point is moved relative to the space between grid points.</param>
        /// <param name="Probability">The probability that a certain grid point will be used.</param>
        public static IEnumerable<Point> GridDistribution(Random Random, int Size, double Error, double Probability)
        {
            double delta = 1.0 / Size;
            Error *= delta;

            double off = delta * 0.5 - Error * 0.5;
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    if (Random.NextDouble() < Probability)
                    {
                        Point p = new Point(x * delta, y * delta);
                        p.X += off + Random.NextDouble() * Error;
                        p.Y += off + Random.NextDouble() * Error;
                        yield return p;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the figure used to draw the interior of cells.
        /// </summary>
        public Figure Cell
        {
            get
            {
                return this._Cell;
            }
        }

        /// <summary>
        /// Gets the figure used to draw the edges between cells.
        /// </summary>
        public Figure Edge
        {
            get
            {
                return this._Edge;
            }
        }

        /// <summary>
        /// Gets the exponent factor used to determine the smoothness falloff from edges
        /// to cells. 
        /// </summary>
        public double Exponent
        {
            get
            {
                return this._Exponent;
            }
        }

        /// <summary>
        /// Gets the multiplier factor used to determine the contrast between edges and
        /// interiors of cells.
        /// </summary>
        public double Multiplier
        {
            get
            {
                return this._Multiplier;
            }
        }

        public override Color GetPoint(Point Point)
        {
            Point.X = ((Point.X % 1.0) + 1.0) % 1.0;
            Point.Y = ((Point.Y % 1.0) + 1.0) % 1.0;
            
            double mdis = double.PositiveInfinity;
            foreach (Point cen in this._Centers)
            {
                double xdis = Math.Abs(Point.X - cen.X);
                double ydis = Math.Abs(Point.Y - cen.Y);
                if (xdis > 0.5)
                {
                    xdis = 1.0 - xdis;
                }
                if (ydis > 0.5)
                {
                    ydis = 1.0 - ydis;
                }
                double dis = xdis * xdis + ydis * ydis;
                mdis = Math.Min(dis, mdis);
            }

            mdis *= this._Multiplier;
            mdis = Math.Pow(mdis, this._Exponent);
            mdis = Math.Min(mdis, 1.0);
            return Color.Mix(this._Cell.GetPoint(Point), this._Edge.GetPoint(Point), mdis);
        }

        private double _Exponent;
        private double _Multiplier;
        private Figure _Cell;
        private Figure _Edge;
        private List<Point> _Centers;
    }
}