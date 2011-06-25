﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// Contains a value of a certain type for each cardinal direction.
    /// </summary>
    public struct Compass<T>
    {
        public Compass(T Left, T Up, T Right, T Down)
        {
            this.Left = Left;
            this.Up = Up;
            this.Right = Right;
            this.Down = Down;
        }

        public Compass(T Value)
            : this(Value, Value, Value, Value)
        {
             
        }

        /// <summary>
        /// Gets or sets the value of this compass for the given direction.
        /// </summary>
        public T this[Direction Direction]
        {
            get
            {
                switch (Direction)
                {
                    case Direction.Left: return this.Left;
                    case Direction.Up: return this.Up;
                    case Direction.Right: return this.Right;
                    default: return this.Down;
                }
            }
            set
            {
                switch (Direction)
                {
                    case Direction.Left: this.Left = value; break;
                    case Direction.Up: this.Up = value; break;
                    case Direction.Right: this.Right = value; break;
                    default: this.Down = value; break;
                }
            }
        }

        /// <summary>
        /// Sets the value of all directions of the compass uniformly.
        /// </summary>
        public T Value
        {
            set
            {
                this.Left = this.Up = this.Right = this.Down = value;
            }
        }

        /// <summary>
        /// Maps every item in this compass to another value based using another compass and
        /// a mapping function.
        /// </summary>
        public Compass<T> Map<TO>(Compass<TO> Other, Func<T, TO, T> Map)
        {
            return new Compass<T>(
                Map(this.Left, Other.Left),
                Map(this.Up, Other.Up),
                Map(this.Right, Other.Right),
                Map(this.Down, Other.Down));
        }

        public T Left;
        public T Up;
        public T Right;
        public T Down;
    }

    /// <summary>
    /// One of the four cardinal directions.
    /// </summary>
    public enum Direction
    {
        Left,
        Up,
        Right,
        Down
    }
}