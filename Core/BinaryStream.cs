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
        public static void WriteInt(this BinaryWriteStream Stream, int Value)
        {
            byte[] data = BitConverter.GetBytes(Value);
            if (!BitConverter.IsLittleEndian)
            {
                data = _ByteReverse(data);
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
            byte[] data = Stream.AssertRead(sizeof(int));
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt32(data, 0);
            }
            else
            {
                return BitConverter.ToInt32(_ByteReverse(data), 0);
            }
        }

        /// <summary>
        /// Writes the double to the specified stream.
        /// </summary>
        /// <param name="Stream">The stream to write the double to.</param>
        /// <param name="Int">The value to write.</param>
        public static void WriteDouble(this BinaryWriteStream Stream, double Value)
        {
            byte[] data = BitConverter.GetBytes(Value);
            if (!BitConverter.IsLittleEndian)
            {
                data = _ByteReverse(data);
            }
            Stream.Write(data);
        }

        /// <summary>
        /// Reads a double from the specified stream.
        /// </summary>
        /// <param name="Stream">The stream to read from.</param>
        /// <returns>The value of the next bytes as an double.</returns>
        public static double ReadDouble(this BinaryReadStream Stream)
        {
            byte[] data = Stream.AssertRead(sizeof(double));
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToDouble(data, 0);
            }
            else
            {
                return BitConverter.ToDouble(_ByteReverse(data), 0);
            }
        }

        /// <summary>
        /// Writes a character to the stream.
        /// </summary>
        /// <param name="Stream">The stream to write to.</param>
        /// <param name="Value">The character to write.</param>
        public static void WriteChar(this BinaryWriteStream Stream, char Value)
        {
            byte[] data = BitConverter.GetBytes(Value);
            if (!BitConverter.IsLittleEndian)
            {
                data = _ByteReverse(data);
            }
            Stream.Write(data);
        }
        /// <summary>
        /// Reads a character from the stream.
        /// </summary>
        /// <param name="Stream">The stream to read from.</param>
        /// <returns>The value of the next bytes as a character.</returns>
        public static char ReadChar(this BinaryReadStream Stream)
        {
            byte[] data = Stream.AssertRead(sizeof(char));
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToChar(data, 0);
            }
            else
            {
                return BitConverter.ToChar(_ByteReverse(data), 0);
            }
        }

        /// <summary>
        /// Writes a string of any length and encoding to a stream.
        /// </summary>
        /// <param name="Stream">The stream to write to.</param>
        /// <param name="String">The string to write.</param>
        public static void WriteString(this BinaryWriteStream Stream, string String)
        {
            Stream.WriteInt(String.Length);
            foreach (char c in String)
            {
                Stream.WriteChar(c);
            }
        }

        /// <summary>
        /// Reads a string from the stream.
        /// </summary>
        /// <param name="Stream">The stream to read from.</param>
        /// <returns>The value of the next bytes as a string.</returns>
        public static string ReadString(this BinaryReadStream Stream)
        {
            int len = Stream.ReadInt();
            char[] chars = new char[len];
            for (int t = 0; t < len; t++)
            {
                chars[t] = Stream.ReadChar();
            }
            return new string(chars);
        }

        /// <summary>
        /// Reverses the order of bytes in a byte array.
        /// </summary>
        private static byte[] _ByteReverse(byte[] Data)
        {
            byte[] ot = new byte[Data.Length];
            for (int t = 0; t < ot.Length; t++)
            {
                ot[t] = Data[ot.Length - t];
            }
            return ot;
        }
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
