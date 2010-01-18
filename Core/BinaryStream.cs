//***********************************
// Copyright (c) 2010, Dmitry Zamkov 
// Open source under the BSD License 
//***********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DUIP.Core
{
    /// <summary>
    /// Stream for reading binary data. Note that all streams must be in
    /// little endian.
    /// </summary>
    public interface BinaryReadStream
    {
        /// <summary>
        /// Reads the next some of data from the stream. Can be blocking if
        /// the stream source is blocking.
        /// </summary>
        /// <param name="Amount">The amount of bytes to read.</param>
        /// <returns>A byte array with the next bytes in it. If the array's size
        /// is less than the amount asked for, the stream is over.</returns>
        byte[] Read(int Amount);
    }

    /// <summary>
    /// Extension methods for binary streams.
    /// </summary>
    public static class BinaryStreamExtensions
    {
        static BinaryStreamExtensions()
        {
            LittleEndian = BitConverter.IsLittleEndian;
        }

        /// <summary>
        /// Reads byte data from a binary stream and raises an exception if the specified
        /// amount of data is not available.
        /// </summary>
        /// <param name="Stream">The stream to read from.</param>
        /// <param name="Amount">The amount of bytes to read.</param>
        /// <returns>A byte array with the next bytes in it.</returns>
        public static byte[] AssertRead(this BinaryReadStream Stream, int Amount)
        {
            byte[] data = Stream.Read(Amount);
            if (data.Length != Amount)
            {
                throw new Exception("AssertRead failed");
            }
            else
            {
                return data;
            }
        }

        /// <summary>
        /// Writes the integer to the specified stream.
        /// </summary>
        /// <param name="Stream">The stream to write the int to.</param>
        /// <param name="Int">The value to write.</param>
        public static void WriteInt(this BinaryWriteStream Stream, int Int)
        {
            byte[] data = new byte[4];
            if (LittleEndian)
            {
                data[0] = (byte)((Int & 0x000000FF) >> 0);
                data[1] = (byte)((Int & 0x0000FF00) >> 8);
                data[2] = (byte)((Int & 0x00FF0000) >> 16);
                data[3] = (byte)((Int & 0xFF000000) >> 24);
            }
            else
            {
                data[0] = (byte)((Int & 0xFF000000) >> 24);
                data[1] = (byte)((Int & 0x00FF0000) >> 16);
                data[2] = (byte)((Int & 0x0000FF00) >> 8);
                data[3] = (byte)((Int & 0x000000FF) >> 0);
            }
            Stream.Write(data);
        }

        /// <summary>
        /// Reads an integer from the specified stream.
        /// </summary>
        /// <param name="Stream">The stream to read from.</param>
        /// <returns>The value of the next bytes as an int.</returns>
        public static int ReadInt(this BinaryReadStream Stream)
        {
            byte[] data = Stream.AssertRead(4);
            if (LittleEndian)
            {
                return
                    (((int)data[0] << 0) +
                    ((int)data[1] << 8) +
                    ((int)data[2] << 16) +
                    ((int)data[3] << 24));
            }
            else
            {
                return
                    (((int)data[0] << 24) +
                    ((int)data[1] << 16) +
                    ((int)data[2] << 8) +
                    ((int)data[3] << 0));
            }
        }

        private static bool LittleEndian;
    }

    /// <summary>
    /// Stream for writing binary data.
    /// </summary>
    public interface BinaryWriteStream
    {
        /// <summary>
        /// Writes an some of data to the stream.
        /// </summary>
        /// <param name="Data">The data to write to the stream. The length of the
        /// byte array is the amount of data to write.</param>
        void Write(byte[] Data);
    }

    /// <summary>
    /// A binary write stream that writes to a byte array.
    /// </summary>
    public class ByteArrayWriter : BinaryWriteStream
    {
        public ByteArrayWriter()
        {
            this._Data = new List<byte[]>();
        }

        public void Write(byte[] Data)
        {
            this._Data.Add(Data);
            this._Bytes += Data.Length;
        }

        /// <summary>
        /// Gets the data written so far to this writer.
        /// </summary>
        public byte[] Data
        {
            get
            {
                byte[] data = new byte[this._Bytes];
                int cur = 0;
                foreach (byte[] m in this._Data)
                {
                    for (int t = 0; t < m.Length; t++)
                    {
                        data[cur + t] = m[t];
                    }
                    cur += m.Length;
                }
                return data;
            }
        }

        private List<byte[]> _Data;
        private int _Bytes;
    }

    /// <summary>
    /// A binary stream reader that reads from a byte array.
    /// </summary>
    public class ByteArrayReader : BinaryReadStream
    {
        public ByteArrayReader(byte[] Data)
        {
            this._Data = Data;
            this._Cur = 0;
        }

        public byte[] Read(int Amount)
        {
            int nsize = this._Data.Length - this._Cur;
            nsize = nsize < Amount ? nsize : Amount;

            byte[] data = new byte[nsize];
            for (int t = 0; t < nsize; t++)
            {
                data[t] = this._Data[this._Cur + t];
            }
            this._Cur += nsize;
            return data;
        }

        private byte[] _Data;
        private int _Cur;
    }
}
