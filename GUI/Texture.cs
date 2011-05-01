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
    public class Texture : IDisposable
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
        /// Sets this as the current texture.
        /// </summary>
        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, (int)this._ID);
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