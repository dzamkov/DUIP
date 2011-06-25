﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace DUIP.UI
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
        /// Creates a figure to render a portion of this texture to the given area.
        /// </summary>
        public TextureFigure CreateFigure(Rectangle Source, Rectangle Destination)
        {
            return new TextureFigure(this, Source, Destination);
        }

        /// <summary>
        /// Creates a figure to render this texture.
        /// </summary>
        public TextureFigure CreateFigure()
        {
            return new TextureFigure(this);
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
        /// Creates a texture by sampling an image.
        /// </summary>
        public static Texture Create<TImage>(TImage Image, Rectangle Area, int Width, int Height)
            where TImage : IImage
        {
            Texture tex;
            using (Bitmap bm = new Bitmap(Width, Height))
            {
                BitmapData bmd = bm.LockBits(
                    new System.Drawing.Rectangle(0, 0, Width, Height),
                    ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Write<TImage>(Image, Area, bmd, Width, Height);
                tex = Create(bmd, Width, Height, true);
                bm.UnlockBits(bmd);
            }
            return tex;
        }

        /// <summary>
        /// Creates a texture by sampling an image while using the given path for caching.
        /// </summary>
        public static Texture CacheCreate<TImage>(Path Cache, Func<TImage> Image, Rectangle Area, int Width, int Height)
            where TImage : IImage
        {
            if (Cache.FileExists)
            {
                return Load(Cache);
            }
            else
            {
                Cache.Parent.MakeDirectory();
                Texture tex;
                using (Bitmap bm = new Bitmap(Width, Height))
                {
                    BitmapData bmd = bm.LockBits(
                        new System.Drawing.Rectangle(0, 0, Width, Height),
                        ImageLockMode.WriteOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    Write<TImage>(Image(), Area, bmd, Width, Height);
                    tex = Create(bmd, Width, Height, true);
                    bm.UnlockBits(bmd);
                    bm.Save(Cache);
                }
                return tex;
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
        /// Writes a section of an image to the given bitmap data.
        /// </summary>
        public static unsafe void Write<TImage>(TImage Image, Rectangle Area, BitmapData Data, int Width, int Height)
            where TImage : IImage
        {
            byte* data = (byte*)Data.Scan0.ToPointer();
            Point size = Area.Size;
            Point delta = new Point(size.X / Width, size.Y / Height);
            Point start = delta * 0.5 + Area.TopLeft;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Color col = Image.GetColor(start + new Point(x * delta.X, y * delta.Y));
                    data[3] = (byte)(col.A * 255.0);
                    data[2] = (byte)(col.R * 255.0);
                    data[1] = (byte)(col.G * 255.0);
                    data[0] = (byte)(col.B * 255.0);
                    data += 4;
                }
            }
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

    /// <summary>
    /// A figure that displays a section of a texture.
    /// </summary>
    public class TextureFigure : Figure
    {
        public TextureFigure(Texture Texture, Rectangle Source, Rectangle Destination)
        {
            this._Texture = Texture;
            this._Source = Source;
            this._Destination = Destination;
        }

        public TextureFigure(Texture Texture)
            : this(Texture, Rectangle.UnitSquare, Rectangle.UnitSquare)
        {

        }

        /// <summary>
        /// Gets the displayed texture.
        /// </summary>
        public Texture Texture
        {
            get
            {
                return this._Texture;
            }
        }

        /// <summary>
        /// Gets the rectangular area in the texture from which to take the image.
        /// </summary>
        public Rectangle Source
        {
            get
            {
                return this._Source;
            }
        }

        /// <summary>
        /// Gets the rectangular area to display the image in.
        /// </summary>
        public Rectangle Destination
        {
            get
            {
                return this._Destination;
            }
        }

        public override void Render(RenderContext Context)
        {
            Context.SetTexture(this._Texture);
            Context.SetColor(Color.White);
            Context.DrawTexturedQuad(this._Source, this._Destination);
        }

        public override Rectangle Bounds
        {
            get
            {
                return this._Destination;
            }
        }

        private Rectangle _Source;
        private Rectangle _Destination;
        private Texture _Texture;
    }
}