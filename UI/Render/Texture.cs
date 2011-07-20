using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using DUIP.UI.Graphics;
using Color = DUIP.UI.Graphics.Color;

namespace DUIP.UI.Render
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
        /// Sets the filter mode for the currently-bound texture.
        /// </summary>
        public static void SetFilterMode(TextureMinFilter Min, TextureMagFilter Mag)
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)Min);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)Mag);
        }

        /// <summary>
        /// Creates a mipmap for the currently-bound texture based on its current contents.
        /// </summary>
        public static void GenerateMipmap()
        {
            GL.Ext.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        /// <summary>
        /// Loads a texture from a file on the filesystem.
        /// </summary>
        public static Texture Load(Path Path)
        {
            using (Bitmap bm = new Bitmap(Path))
            {
                return Create(bm);
            }
        }

        /// <summary>
        /// Creates a texture that contains the data from the given sampled figure using the given view.
        /// </summary>
        public static unsafe Texture Create(SampledFigure Figure, View View, Format Format, int Width, int Height)
        {
            Texture tex;
            const System.Drawing.Imaging.PixelFormat bmpformat = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
            using (Bitmap bmp = new Bitmap(Width, Height, bmpformat))
            {
                BitmapData bd = bmp.LockBits(new System.Drawing.Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, bmpformat);
                byte* data = (byte*)bd.Scan0.ToPointer();

                Point xdelta = View.Right / Width;
                Point ydelta = View.Down / Height;
                Point offset = View.Offset + xdelta * 0.5 + ydelta * 0.5;
                for (int x = 0; x < Width; x++)
                {
                    Point tpos = offset + xdelta * x;
                    for (int y = 0; y < Height; y++)
                    {
                        Point pos = tpos + ydelta * y;
                        Color col = Figure.GetColor(pos);
                        data[3] = (byte)(col.A * 255.0);
                        data[2] = (byte)(col.R * 255.0);
                        data[1] = (byte)(col.G * 255.0);
                        data[0] = (byte)(col.B * 255.0);
                        data += 4;
                    }
                }

                tex = Create(bd, Format, true);
            }

            return tex;
        }

        /// <summary>
        /// Creates a texture for the given bitmap.
        /// </summary>
        public static Texture Create(Bitmap Source)
        {
            BitmapData bd = Source.LockBits(
                new System.Drawing.Rectangle(0, 0, Source.Width, Source.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Texture t = Create(bd, Format.BGRA32, true);
            Source.UnlockBits(bd);
            return t;
        }

        /// <summary>
        /// Creates a blank texture.
        /// </summary>
        public static Texture Create(Format Format, int Width, int Height)
        {
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexEnv(TextureEnvTarget.TextureEnv,
                TextureEnvParameter.TextureEnvMode,
                (float)TextureEnvMode.Modulate);
            GL.TexImage2D(TextureTarget.Texture2D,
                0, Format.PixelInternalFormat,
                Width, Height, 0,
                Format.PixelFormat, Format.PixelType, IntPtr.Zero);
            return new Texture(id);
        }

        /// <summary>
        /// A possible pixel format for a texture.
        /// </summary>
        public struct Format
        {
            public PixelInternalFormat PixelInternalFormat;
            public OpenTK.Graphics.OpenGL.PixelFormat PixelFormat;
            public PixelType PixelType;

            public static readonly Format BGRA32 = new Format()
            {
                PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelInternalFormat = PixelInternalFormat.Rgba,
                PixelType = PixelType.UnsignedByte
            };

            public static readonly Format A8 = new Format()
            {
                PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Alpha,
                PixelInternalFormat = PixelInternalFormat.Alpha,
                PixelType = PixelType.UnsignedByte
            };
        }

        /// <summary>
        /// Creates a texture using the given bitmap data. The texture can optionally be mipmapped.
        /// </summary>
        public static Texture Create(BitmapData Data, Format Format, bool Mipmap)
        {
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexEnv(TextureEnvTarget.TextureEnv,
                TextureEnvParameter.TextureEnvMode,
                (float)TextureEnvMode.Modulate);

            SetImage(Format, 0, Data.Width, Data.Height, Data.Scan0);

            Texture tex = new Texture(id);
            if (Mipmap)
            {
                GenerateMipmap();
                SetFilterMode(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
            }
            else
            {
                SetFilterMode(TextureMinFilter.Linear, TextureMagFilter.Linear);
            }
            return new Texture(id);
        }

        /// <summary>
        /// Sets the image of a certain mipmap level of the current texture from a pointer to image data.
        /// </summary>
        public static void SetImage(Format Format, int Level, int Width, int Height, IntPtr Data)
        {
            GL.TexImage2D(TextureTarget.Texture2D,
                Level, Format.PixelInternalFormat, Width, Height, 0,
                Format.PixelFormat, Format.PixelType, Data);
        }

        public void Dispose()
        {
            GL.DeleteTexture((int)this._ID);
        }

        private uint _ID;
    }
}