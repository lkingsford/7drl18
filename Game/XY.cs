﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public class XY
    {
        /// <summary>
        /// Construct pair with given X, Y
        /// </summary>
        /// <param name="X">X Coordinate</param>
        /// <param name="Y">Y Coordinate</param>
        public XY(int X, int Y)
        {
            this._X = X;
            this._Y = Y;
        }

        /// <summary>
        /// Underlying value for X
        /// </summary>
        private int _X;

        /// <summary>
        /// X coordinate
        /// </summary>
        public int X
        {
            get
            {
                return _X;
            }
        }

        /// <summary>
        /// Underlying value for Y
        /// </summary>
        private int _Y;

        /// <summary>
        /// Y coordinate
        /// </summary>
        public int Y
        {
            get
            {
                return _Y;
            }
        }

        /// <summary>
        /// Determine if XY is inclusively within given bounds
        /// </summary>
        /// <param name="MinX">Minimum allowed x</param>
        /// <param name="MinY">Minimum allowed y</param>
        /// <param name="MaxX">Maximum allowed x</param>
        /// <param name="MaxY">Maximum allowed y</param>
        /// <returns>True, if XY within the given bounds</returns>
        public bool ContainedBy(int MinX, int MinY, int MaxX, int MaxY)
        {
            if ((MinX > MaxX) || (MinY > MaxY))
            {
                throw new ArgumentException("Min must be greater than max");
            }

            return _X >= MinX && MaxX >= _X && _Y >= MinY && MaxY >= _Y;
        }

        public bool Adjacent(XY second)
        {
            return Math.Abs(X - second.X) <= 1 && Math.Abs(Y - second.Y) <= 1;
        }

        /// <summary>
        /// Return coordinates as a string
        /// </summary>
        /// <returns>(X, Y)</returns>
        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        /// <summary>
        /// Addition operator for XY. Adds X to X and Y to Y - cartesian addition.
        /// </summary>
        /// <param name="first">First operand</param>
        /// <param name="second">Second operand</param>
        /// <returns></returns>
        public static XY operator +(XY first, XY second)
        {
            return new XY(first.X + second.X, first.Y + second.Y);
        }

        /// <summary>
        /// Subtraction operator for XY. Subtracts X from X and Y from Y.
        /// </summary>
        /// <param name="first">First operand</param>
        /// <param name="second">Second operand</param>
        /// <returns></returns>
        public static XY operator -(XY first, XY second)
        {
            return new XY(first.X - second.X, first.Y - second.Y);
        }


        /// <summary>
        /// Multiply XY by a scalar
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static XY operator *(XY first, int second)
        {
            return new XY(first.X * second, first.Y * second);
        }

        /// <summary>
        /// Multiply XY by a scalar
        /// </summary>
        /// <param name="second"></param>
        /// <param name="first"></param>
        /// <returns></returns>
        public static XY operator *(int first, XY second)
        {
            return new XY(second.X * first, second.Y * first);
        }

        /// <summary>
        /// Equality operator for XY
        /// </summary>
        /// <param name="first">First operand</param>
        /// <param name="second">Second operand</param>
        /// <returns></returns>
        public static bool operator ==(XY first, XY second)
        {
            if (ReferenceEquals(first, null) ^ ReferenceEquals(second, null))
            {
                return false;
            }
            if (ReferenceEquals(first, null) && ReferenceEquals(second, null))
            {
                return true;
            }
            return (first.X == second.X) && (first.Y == second.Y);
        }

        /// <summary>
        /// Return if value equal to this
        /// </summary>
        /// <param name="value">First operand</param>
        /// <returns></returns>
        public override bool Equals(object value)
        {
            return this == (XY)value;
        }

        /// <summary>
        /// Return hash code of object.
        /// Uses X + Y bitshifted 16 left
        /// </summary>
        /// <returns>Hash code of object</returns>
        public override int GetHashCode()
        {
            return _X + _Y << 16;
        }

        /// <summary>
        /// Inequality operator for XY
        /// </summary>
        /// <param name="first">First operand</param>
        /// <param name="second">Second operand</param>
        /// <returns></returns>
        public static bool operator !=(XY first, XY second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Return like a unit vector, but X and Y are both a -1, 0 or 1
        /// depending on if that element is positive, 0 or negative
        /// </summary>
        /// <returns></returns>
        public XY Unit()
        {
            return new XY(Math.Sign(X), Math.Sign(Y));
        }
    }
}
