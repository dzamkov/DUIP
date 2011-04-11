using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A description of a static, unbound, and arbitrarily detailed two-dimensional image with an alpha channel.
    /// </summary>
    public abstract class Image
    {
        /// <summary>
        /// Gets an image with a solid color.
        /// </summary>
        public static SolidImage Solid(Color Color)
        {
            return new SolidImage(Color);
        }

        /// <summary>
        /// Gets an image that displays a traced path.
        /// </summary>
        public static PathImage Path(Path Path, StrokeStyle Style)
        {
            return new PathImage(Path, Style);
        }

        /// <summary>
        /// Gets an image that displays a path traced with the given color and thickness.
        /// </summary>
        public static PathImage Path(Path Path, Color Color, double Thickness)
        {
            return new PathImage(Path, StrokeStyle.Solid(Color, Thickness));
        }

        /// <summary>
        /// Creates an image that displays another image drawn over this image.
        /// </summary>
        public OverDrawImage OverDraw(Image Over)
        {
            return new OverDrawImage(Over, this);
        }

        /// <summary>
        /// Creates an image that displays another image drawn under this image.
        /// </summary>
        public OverDrawImage UnderDraw(Image Under)
        {
            return new OverDrawImage(this, Under);
        }
    }

    /// <summary>
    /// An image created by drawing one image over another using the alpha channel in the "Over" image to determine how 
    /// much of each image to include.
    /// </summary>
    public class OverDrawImage : Image
    {
        public OverDrawImage(Image Over, Image Under)
        {
            this._Over = Over;
            this._Under = Under;
        }

        /// <summary>
        /// Gets the image that is being drawn over. The affect this image has on the final image is (1 - The alpha of the "Over" image) for any given point.
        /// </summary>
        public Image Under
        {
            get
            {
                return this._Under;
            }
        }

        /// <summary>
        /// Gets the image that is drawning over. The affect this image has on the final image is determined by its alpha value for any given point.
        /// </summary>
        public Image Over
        {
            get
            {
                return this._Over;
            }
        }

        private Image _Under;
        private Image _Over;
    }

    /// <summary>
    /// An image with a solid color at all points.
    /// </summary>
    public class SolidImage : Image
    {
        public SolidImage(Color Color)
        {
            this._Color = Color;
        }

        /// <summary>
        /// Gets the uniform color of the image.
        /// </summary>
        public Color Color
        {
            get
            {
                return this._Color;
            }
        }

        private Color _Color;
    }

    /// <summary>
    /// An image that displays a path filled with a stroke style with all points not filled being completely transparent.
    /// </summary>
    public class PathImage : Image
    {
        public PathImage(Path Path, StrokeStyle Style)
        {
            this._Path = Path;
            this._Style = Style;
        }

        /// <summary>
        /// Gets the path that is traced across.
        /// </summary>
        public Path Path
        {
            get
            {
                return this._Path;
            }
        }

        /// <summary>
        /// Gets the stroke style used to trace the path.
        /// </summary>
        public StrokeStyle Style
        {
            get
            {
                return this._Style;
            }
        }

        private Path _Path;
        private StrokeStyle _Style;
    }
}