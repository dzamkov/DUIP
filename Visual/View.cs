using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DUIP.Core;

namespace DUIP.Visual
{
    /// <summary>
    /// A view of the world.
    /// </summary>
    public struct View
    {
        /// <summary>
        /// The point the view is looking at.
        /// </summary>
        public Location Location;

        /// <summary>
        /// The zoom level in relation to the parent sector. 1.0 indicates
        /// a normal zoom level. 2.0 indicates twice normal zoom of the sector.
        /// </summary>
        public double ZoomLevel;

        /// <summary>
        /// Zooms by a certain amount into the view.
        /// </summary>
        /// <param name="Amount">An amount to multiply the zoom level by.</param>
        public void Zoom(double Amount)
        {
            this.ZoomLevel *= Amount;
            this.Normalize();
        }

        /// <summary>
        /// Pans the view based on the zoomlevel and the specified amount.
        /// </summary>
        /// <param name="X">The x amount to pan.</param>
        /// <param name="Y">The y amount to pan.</param>
        public void Pan(double X, double Y)
        {
            this.Location.Offset.Right += X / this.ZoomLevel;
            this.Location.Offset.Down += Y / this.ZoomLevel;
            this.Normalize();
        }

        /// <summary>
        /// Brings the location to the correct sector.
        /// </summary>
        public void Normalize()
        {
            this.Location.Normalize();
            while (this.ZoomLevel <= 0.5)
            {
                this.Location.Up();
                this.ZoomLevel *= 2.0;
            }
            while (this.ZoomLevel > 2.0)
            {
                this.Location.Down();
                this.ZoomLevel /= 2.0;
            }
        }
    }
}
