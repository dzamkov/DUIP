using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// An unbounded, two-dimensional dynamic object a user can interaction with.
    /// </summary>
    public abstract class Surface
    {
        /// <summary>
        /// Updates the state of the surface with and for user interaction.
        /// </summary>
        public abstract void Update(Input Input, Output Output);
    }

    /// <summary>
    /// A surface bounded to a variable-sized rectangular area whose top-left point is at the origin, 
    /// with no interactions outside the area.
    /// </summary>
    public abstract class Control : Surface
    {
        /// <summary>
        /// Gets or sets the size of the rectangle bounding the control.
        /// </summary>
        public abstract Point Size { get; set; }
    }
}