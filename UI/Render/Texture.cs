using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using DUIP.Memory;
using DUIP.UI.Graphics;
using MPath = DUIP.Memory.Path;
using GColor = DUIP.UI.Graphics.Color;

namespace DUIP.UI.Render
{
    /// <summary>
    /// Represents a two-dimensional image in graphics memory. Contains functions related to textures.
    /// </summary>
    public struct Texture : IDisposable
    {
        public Texture(uint ID)
        {
            this.ID = ID;
        }

        public Texture(int ID)
        {
            this.ID = (uint)ID;
        }

        /// <summary>
        /// The ID for this texture.
        /// </summary>
        public readonly uint ID;

        /// <summary>
        /// Gets if this is the null texture.
        /// </summary>
        public bool IsNull
        {
            get
            {
                return this.ID == 0;
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
            GL.BindTexture(TextureTarget.Texture2D, (int)this.ID);
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
        public static Texture Load(MPath Path)
        {
            using (Bitmap bm = new Bitmap(Path))
            {
                return Create(bm);
            }
        }

        /// <summary>
        /// Writes a 32bpp ARGB image for the given view of a sampled figure.
        /// </summary>
        public static unsafe void WriteImageARGB(SampledFigure Figure, View View, int Width, int Height, byte* Data)
        {
            Point xdelta = View.Right / Width;
            Point ydelta = View.Down / Height;
            Point offset = View.Offset + xdelta * 0.5 + ydelta * 0.5;
            for (int x = 0; x < Width; x++)
            {
                Point tpos = offset + xdelta * x;
                for (int y = 0; y < Height; y++)
                {
                    Point pos = tpos + ydelta * y;
                    GColor col = Figure.GetColor(pos);
                    Data[3] = (byte)(col.A * 255.0);
                    Data[2] = (byte)(col.R * 255.0);
                    Data[1] = (byte)(col.G * 255.0);
                    Data[0] = (byte)(col.B * 255.0);
                    Data += 4;
                }
            }
        }

        /// <summary>
        /// Creates a texture that contains the data from the given sampled figure using the given view.
        /// </summary>
        public static unsafe Texture Create(SampledFigure Figure, View View, Format Format, int Width, int Height)
        {
            Texture tex;
            byte[] data = new byte[Width * Height * 4];
            fixed (byte* ptr = data)
            {
                WriteImageARGB(Figure, View, Width, Height, ptr);
                tex = Create();
                SetImage(Format, 0, Width, Height, ptr);
                GenerateMipmap();
                SetFilterMode(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
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
            Texture t = Create();

            SetImage(Format.BGRA32, 0, bd.Width, bd.Height, bd.Scan0);
            GenerateMipmap();
            SetFilterMode(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);

            Source.UnlockBits(bd);
            return t;
        }


        /// <summary>
        /// Creates and binds a texture with no associated image data.
        /// </summary>
        public static Texture Create()
        {
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexEnv(TextureEnvTarget.TextureEnv,
                TextureEnvParameter.TextureEnvMode,
                (float)TextureEnvMode.Modulate);
            return new Texture(id);
        }

        /// <summary>
        /// Creates a texture with undefined image data.
        /// </summary>
        public static Texture CreateBlank(Format Format, int Width, int Height)
        {
            Texture tex = Create();
            GL.TexImage2D(TextureTarget.Texture2D,
                0, Format.PixelInternalFormat,
                Width, Height, 0,
                Format.PixelFormat, Format.PixelType, IntPtr.Zero);
            return tex;
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

        /// <summary>
        /// Sets the image of a certain mipmap level of the current texture from a pointer to image data.
        /// </summary>
        public static unsafe void SetImage(Format Format, int Level, int Width, int Height, void* Data)
        {
            SetImage(Format, Level, Width, Height, (IntPtr)Data);
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

        public void Dispose()
        {
            GL.DeleteTexture((int)this.ID);
        }
    }
}