using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace DUIP.GUI
{
    /// <summary>
    /// Represents a two-dimensional image in graphics memory. Contains functions related to textures.
    /// </summary>
    public struct Texture : IDisposable
    {
        public Texture(uint ID)
        {
            this._ID = ID;
        }

        public Texture(int ID)
        {
            this._ID = (uint)ID;
        }

        /// <summary>
        /// Gets the ID for this texture.
        /// </summary>
        public uint ID
        {
            get
            {
                return this._ID;
            }
        }

        /// <summary>
        /// Gets if this is the null texture.
        /// </summary>
        public bool IsNull
        {
            get
            {
                return this._ID == 0;
            }
        }

        /// <summary>
        /// Gets the null texture.
        /// </summary>
        public static Texture Null
        {
            get
            {
                return new Texture(0);
            }
        }

        /// <summary>
        /// Sets this as the current texture.
        /// </summary>
        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, (int)this._ID);
        }

        /// <summary>
        /// Sets the technique used for wrapping the currently-bound texture.
        /// </summary>
        public static void SetWrapMode(TextureWrapMode Horizontal, TextureWrapMode Vertical)
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)Horizontal);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)Vertical);
        }

        /// <summary>
        /// Draws a textured quad using the current texture stretched across the destination area.
        /// </summary>
        public static void DrawQuad(Rectangle Destination)
        {
            DrawQuad(new Rectangle(0.0, 0.0, 1.0, 1.0), Destination);
        }

        /// <summary>
        /// Draws a textured quad using the current texture.
        /// </summary>
        public static void DrawQuad(Rectangle Source, Rectangle Destination)
        {
            GL.Begin(BeginMode.Quads);
            OutputQuad(Source, Destination);
            GL.End();
        }

        /// <summary>
        /// Outputs a texture-mapped quad to the current graphics context.
        /// </summary>
        public static void OutputQuad(Rectangle Source, Rectangle Destination)
        {
            double sl = Source.Left;
            double st = Source.Top;
            double sr = Source.Right;
            double sb = Source.Bottom;
            double dl = Destination.Left;
            double dt = Destination.Top;
            double dr = Destination.Right;
            double db = Destination.Bottom;
            GL.TexCoord2(sl, st); GL.Vertex2(dl, dt);
            GL.TexCoord2(sr, st); GL.Vertex2(dr, dt);
            GL.TexCoord2(sr, sb); GL.Vertex2(dr, db);
            GL.TexCoord2(sl, sb); GL.Vertex2(dl, db);
        }


        /// <summary>
        /// Creates a texture of the given size for a rectangular area on a figure.
        /// </summary>
        public static unsafe Texture Create(Figure Figure, Rectangle Area, int Width, int Height)
        {
            using (Bitmap bm = Figure.GetArea(Area, Width, Height))
            {
                return Create(bm);
            }
        }

        /// <summary>
        /// Creates and caches a texture or loads it from a path.
        /// </summary>
        public static Texture CreateOrLoad(Path Path, Func<Figure> Figure, Rectangle Area, int Width, int Height)
        {
            if (Path.FileExists)
            {
                return Load(Path);
            }
            else
            {
                Path.Parent.MakeDirectory();
                Bitmap bm = Figure().GetArea(Area, Width, Height);
                bm.Save(Path);
                return Create(bm);
            }
        }

        /// <summary>
        /// Loads a texture from a file.
        /// </summary>
        public static Texture Load(Path Path)
        {
            using (Bitmap bm = new Bitmap(Path))
            {
                return Create(bm);
            }
        }

        /// <summary>
        /// Creates a texture for the given bitmap.
        /// </summary>
        public static Texture Create(Bitmap Source)
        {
            BitmapData bd = Source.LockBits(
                new System.Drawing.Rectangle(0, 0, Source.Width, Source.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Texture t = Create(bd, Source.Width, Source.Height, true);
            Source.UnlockBits(bd);
            return t;
        }

        /// <summary>
        /// Creates a texture using the given bitmap data. The texture can optionally be mipmapped.
        /// </summary>
        public static Texture Create(BitmapData Data, int Width, int Height, bool Mipmap)
        {
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexEnv(TextureEnvTarget.TextureEnv,
                TextureEnvParameter.TextureEnvMode,
                (float)TextureEnvMode.Modulate);

            GL.TexImage2D(TextureTarget.Texture2D,
                0, PixelInternalFormat.Rgba,
                Width, Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, Data.Scan0);

            if (Mipmap)
            {
                GL.Ext.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
            return new Texture(id);
        }

        public void Dispose()
        {
            GL.DeleteTexture((int)this._ID);
        }

        private uint _ID;
    }

}