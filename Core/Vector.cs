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
    /// Represents a long distance across sectors. The units, given in integers
    /// are the amount of sectors to the right and down the grid.
    /// </summary>
    public struct LVector : Serializable
    {
        public LVector(int Right, int Down)
        {
            this.Right = Right;
            this.Down = Down;
        }

        public LVector(BinaryReadStream Stream)
        {
            this.Right = Stream.ReadInt();
            this.Down = Stream.ReadInt();
        }

        public void Serialize(BinaryWriteStream Stream)
        {
            Stream.WriteInt(this.Right);
            Stream.WriteInt(this.Down);
        }

        public int Right;
        public int Down;

        public static LVector operator +(LVector A, LVector B)
        {
            return new LVector { Down = A.Down + B.Down, Right = A.Right + B.Right };
        }

        public static LVector operator -(LVector A, LVector B)
        {
            return new LVector { Down = A.Down - B.Down, Right = A.Right - B.Right };
        }
    }

    /// <summary>
    /// Represents a precise distance across sectors. The units are the amount of
    /// sectors to the right and down the grid. Units may be fractional and cover
    /// a part of a sector. When used within a sector, (0,0) points to the top-left
    /// corner of the sector and (1,1) points to the bottom-left corner.
    /// </summary>
    public struct SVector
    {
        public SVector(double Right, double Down)
        {
            this.Right = Right;
            this.Down = Down;
        }

        public SVector(BinaryReadStream Stream)
        {
            this.Right = Stream.ReadDouble();
            this.Down = Stream.ReadDouble();
        }

        public void Serialize(BinaryWriteStream Stream)
        {
            Stream.WriteDouble(this.Right);
            Stream.WriteDouble(this.Down);
        }

        public double Right;
        public double Down;

        /// <summary>
        /// Converts the SVector to an LVector by removing units within a sector.
        /// </summary>
        /// <returns>A less-precise LVector representation of this vector.</returns>
        public LVector ToLVector()
        {
            LVector lv = new LVector();
            lv.Down = (int)(Math.Floor(this.Down));
            lv.Right = (int)(Math.Floor(this.Right));
            return lv;
        }

        public static SVector operator +(SVector A, SVector B)
        {
            return new SVector { Down = A.Down + B.Down, Right = A.Right + B.Right };
        }

        public static SVector operator -(SVector A, SVector B)
        {
            return new SVector { Down = A.Down - B.Down, Right = A.Right - B.Right };
        }

        public static SVector operator +(SVector A, LVector B)
        {
            return new SVector { Down = A.Down + (double)B.Down, Right = A.Right + (double)B.Right };
        }

        public static SVector operator -(SVector A, LVector B)
        {
            return new SVector { Down = A.Down - (double)B.Down, Right = A.Right - (double)B.Right };
        }
    }
}
