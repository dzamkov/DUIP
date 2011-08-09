using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI.Graphics
{
    /// <summary>
    /// A sampled figure that displays a tiled cellular pattern. The pattern can be made to resemble water,
    /// ripples, leaves and some other nature-looking-thingies.
    /// </summary>
    public class CellularFigure : SampledFigure
    {
        public CellularFigure(Color Cell, Color Edge, double Exponent, double Multiplier, IEnumerable<Point> CellCenters)
            : base(Rectangle.Unbound, true)
        {
            this.Cell = Cell;
            this.Edge = Edge;
            this.Exponent = Exponent;
            this.Multiplier = Multiplier;

            this._Centers = new List<Point>();
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
                yield return new Point(Random.Sample(), Random.Sample());
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
                    if (Random.Sample() < Probability)
                    {
                        Point p = new Point(x * delta, y * delta);
                        p.X += off + Random.Sample() * Error;
                        p.Y += off + Random.Sample() * Error;
                        yield return p;
                    }
                }
            }
        }

        /// <summary>
        /// The color used to draw the interior of cells.
        /// </summary>
        public readonly Color Cell;

        /// <summary>
        /// The color used to draw the edges between cells.
        /// </summary>
        public readonly Color Edge;

        /// <summary>
        /// The exponent factor used to determine the smoothness falloff from edges
        /// to cells. 
        /// </summary>
        public readonly double Exponent;

        /// <summary>
        /// The multiplier factor used to determine the contrast between edges and
        /// interiors of cells.
        /// </summary>
        public readonly double Multiplier;

        public override Color  GetColor(Point Point)
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

            mdis *= this.Multiplier;
            mdis = Math.Pow(mdis, this.Exponent);
            mdis = Math.Min(mdis, 1.0);
            return Color.Mix(this.Cell, this.Edge, mdis);
        }

        private List<Point> _Centers;
    }
}