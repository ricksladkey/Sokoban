/*
 * Copyright (c) 2010 by Rick Sladkey
 * 
 * This program is free software: you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the
 * Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along
 * with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Sokoban.Engine.Core
{
    public struct Direction : IEquatable<Direction>
    {
        private enum Value : byte
        {
            None = 0,
            Up = 1,
            Down = 2,
            Left = 3,
            Right = 4,
        }

        public static Direction None = new Direction(Value.None);
        public static Direction Up = new Direction(Value.Up);
        public static Direction Down = new Direction(Value.Down);
        public static Direction Left = new Direction(Value.Left);
        public static Direction Right = new Direction(Value.Right);

        public static int UpperBound = (int)Value.Right + 1;

        public static Direction[] Directions
        {
            get
            {
                return directions;
            }
        }

        private static Direction[] directions;
        private static int[] horizontalTable;
        private static int[] verticalTable;
        private static Direction[] oppositeDirectionTable;

        public static explicit operator int(Direction direction)
        {
            return (int)direction.value;
        }

        public static explicit operator Direction(byte direction)
        {
            return new Direction((Value)direction);
        }

        public static Coordinate2D operator *(int factor, Direction direction)
        {
            Coordinate2D v = direction;
            return new Coordinate2D(factor * v.Row, factor * v.Column);
        }

        public static Coordinate2D operator *(Direction direction, int factor)
        {
            Coordinate2D v = direction;
            return new Coordinate2D(v.Row * factor, v.Column * factor);
        }

        public static bool operator ==(Direction direction1, Direction direction2)
        {
            return direction1.value == direction2.value;
        }

        public static bool operator !=(Direction direction1, Direction direction2)
        {
            return direction1.value != direction2.value;
        }

        public static bool AreParallel(Direction direction1, Direction direction2)
        {
            if (direction1 == Direction.Up || direction1 == Direction.Down)
            {
                return direction2 == Direction.Up || direction2 == Direction.Down;
            }
            if (direction1 == Direction.Left || direction1 == Direction.Right)
            {
                return direction2 == Direction.Left || direction2 == Direction.Right;
            }
            return false;
        }

        public static bool ArePerpendicular(Direction direction1, Direction direction2)
        {
            if (direction1 == Direction.Up || direction1 == Direction.Down)
            {
                return direction2 == Direction.Left || direction2 == Direction.Right;
            }
            if (direction1 == Direction.Left || direction1 == Direction.Right)
            {
                return direction2 == Direction.Up || direction2 == Direction.Down;
            }
            return false;
        }

        public static int GetVertical(Direction direction)
        {
            return verticalTable[(int)direction.value];
        }

        public static int GetHorizontal(Direction direction)
        {
            return horizontalTable[(int)direction.value];
        }

        public static Direction GetOpposite(Direction direction)
        {
            return oppositeDirectionTable[(int)direction.value];
        }

        public static Direction Rotate(Direction direction)
        {
            if (direction == Direction.Up)
            {
                return Direction.Right;
            }
            if (direction == Direction.Right)
            {
                return Direction.Down;
            }
            if (direction == Direction.Down)
            {
                return Direction.Left;
            }
            if (direction == Direction.Left)
            {
                return Direction.Up;
            }
            throw new InvalidOperationException("Invalid direction");
        }

        public static Direction Mirror(Direction direction)
        {
            if (direction == Direction.Right)
            {
                return Direction.Left;
            }
            if (direction == Direction.Left)
            {
                return Direction.Right;
            }
            return direction;
        }

        static Direction()
        {
            directions = new Direction[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

            horizontalTable = new int[UpperBound];
            horizontalTable[(int)Direction.Up] = 0;
            horizontalTable[(int)Direction.Down] = 0;
            horizontalTable[(int)Direction.Left] = -1;
            horizontalTable[(int)Direction.Right] = 1;

            verticalTable = new int[UpperBound];
            verticalTable[(int)Direction.Up] = -1;
            verticalTable[(int)Direction.Down] = 1;
            verticalTable[(int)Direction.Left] = 0;
            verticalTable[(int)Direction.Right] = 0;

            oppositeDirectionTable = new Direction[UpperBound];
            oppositeDirectionTable[(int)Direction.Up] = Direction.Down;
            oppositeDirectionTable[(int)Direction.Down] = Direction.Up;
            oppositeDirectionTable[(int)Direction.Left] = Direction.Right;
            oppositeDirectionTable[(int)Direction.Right] = Direction.Left;

        }

        private Value value;

        private Direction(Value value)
        {
            this.value = value;
        }

        public Direction(Direction other)
        {
            this.value = other.value;
        }

        public override string ToString()
        {
            return String.Format("{0}", value);
        }

        public override bool Equals(object obj)
        {
            return Equals((Direction)obj);
        }

        public override int GetHashCode()
        {
            return (int)value;
        }

        #region IEquatable<Direction> Members

        public bool Equals(Direction other)
        {
            return value == other.value;
        }

        #endregion
    }
}
