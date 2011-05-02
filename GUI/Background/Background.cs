using System;
using System.Collections.Generic;

using OpenTK.Graphics;

namespace DUIP.GUI
{
    /// <summary>
    /// A dynamic object responsible for rendering the background for a world. The main
    /// purpose of backgrounds is to allow the user to see when and how the camera moves when there are no 
    /// foreground objects.
    /// </summary>
    public abstract class Background
    {
        /// <summary>
        /// Renders the background to the current graphics context when the given view is used.
        /// </summary>
        public virtual void Render(World World, View View)
        {

        }

        /// <summary>
        /// Updates the state of the background by the given amount of time in seconds.
        /// </summary>
        public virtual void Update(World World, double Time)
        {

        }
    }
}