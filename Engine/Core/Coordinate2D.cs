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
using System.Diagnostics;

namespace Sokoban.Engine.Core
{
    public struct Coordinate2D : IEquatable<Coordinate2D>, IComparable<Coordinate2D>
    {
        private const int undefinedValue = int.MaxValue;
        private static Coordinate2D[] vectors;

        public static Coordinate2D Undefined = new Coordinate2D(undefinedValue, undefinedValue);

        public static int GetOrthogonalDistance(Coordinate2D coord1, Coordinate2D coord2)
        {
            return Math.Abs(coord1.Row - coord2.Row) + Math.Abs(coord1.Column - coord2.Column);
        }

        public static int GetDiagonalDistance(Coordinate2D coord1, Coordinate2D coord2)
        {
            return Math.Max(Math.Abs(coord1.Row - coord2.Row), Math.Abs(coord1.Column - coord2.Column));
        }

        public static bool operator <(Coordinate2D coord1, Coordinate2D coord2)
        {
            return coord1.Row < coord2.Row || coord1.Row == coord2.Row && coord1.Column < coord2.Column;
        }

        public static bool operator <=(Coordinate2D coord1, Coordinate2D coord2)
        {
            return coord1.Row < coord2.Row || coord1.Row == coord2.Row && coord1.Column <= coord2.Column;
        }

        public static bool operator >(Coordinate2D coord1, Coordinate2D coord2)
        {
            return coord1.Row > coord2.Row || coord1.Row == coord2.Row && coord1.Column > coord2.Column;
        }

        public static bool operator >=(Coordinate2D coord1, Coordinate2D coord2)
        {
            return coord1.Row > coord2.Row || coord1.Row == coord2.Row && coord1.Column >= coord2.Column;
        }

        public static Coordinate2D operator +(Coordinate2D coord1, Coordinate2D coord2)
        {
            return new Coordinate2D(coord1.Row + coord2.Row, coord1.Column + coord2.Column);
        }

        public static Coordinate2D operator -(Coordinate2D coord1, Coordinate2D coord2)
        {
            return new Coordinate2D(coord1.Row - coord2.Row, coord1.Column - coord2.Column);
        }

        public static Coordinate2D operator -(Coordinate2D coord)
        {
            return new Coordinate2D(-coord.Row, -coord.Column);
        }

        public static Coordinate2D operator +(Coordinate2D coord, Direction direction)
        {
            Coordinate2D v = vectors[(int)direction];
            return new Coordinate2D(coord.Row + v.Row, coord.Column + v.Column);
        }

        public static Coordinate2D operator -(Coordinate2D coord, Direction direction)
        {
            Coordinate2D v = vectors[(int)direction];
            return new Coordinate2D(coord.Row - v.Row, coord.Column - v.Column);
        }

        public static Coordinate2D operator +(Direction direction, Coordinate2D coord)
        {
            Coordinate2D v = vectors[(int)direction];
            return new Coordinate2D(v.Row + coord.Row, v.Column + coord.Column);
        }

        public static Coordinate2D operator -(Direction direction, Coordinate2D coord)
        {
            Coordinate2D v = vectors[(int)direction];
            return new Coordinate2D(v.Row - coord.Row, v.Column - coord.Column);
        }

        public static Coordinate2D operator *(int factor, Coordinate2D coord)
        {
            return new Coordinate2D(factor * coord.Row, factor * coord.Column);
        }

        public static Coordinate2D operator *(Coordinate2D coord, int factor)
        {
            return new Coordinate2D(coord.Row * factor, coord.Column * factor);
        }

        public static bool operator ==(Coordinate2D coord1, Coordinate2D coord2)
        {
            return coord1.Equals(coord2);
        }

        public static bool operator !=(Coordinate2D coord1, Coordinate2D coord2)
        {
            return !coord1.Equals(coord2);
        }

        public static implicit operator Coordinate2D(Direction direction)
        {
            return vectors[(int)direction];
        }

        public static Coordinate2D Transpose(Coordinate2D coord)
        {
            return new Coordinate2D(coord.Column, coord.Row);
        }

        static Coordinate2D()
        {
            vectors = new Coordinate2D[Direction.UpperBound];

            vectors[(int)Direction.None] = new Coordinate2D(0, 0);
            vectors[(int)Direction.Up] = new Coordinate2D(-1, 0);
            vectors[(int)Direction.Down] = new Coordinate2D(1, 0);
            vectors[(int)Direction.Left] = new Coordinate2D(0, -1);
            vectors[(int)Direction.Right] = new Coordinate2D(0, 1);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int row;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int column;

        public Coordinate2D(int row, int column)
        {
            this.row = row;
            this.column = column;
        }

        public Coordinate2D(Coordinate2D other)
        {
            this.row = other.row;
            this.column = other.column;
        }

        public Coordinate2D(Direction direction)
        {
            this = vectors[(int)direction];
        }

        public int Row
        {
            get
            {
                return row;
            }
            set
            {
                row = value;
            }
        }

        public int Column
        {
            get
            {
                return column;
            }
            set
            {
                column = value;
            }
        }

        public bool IsUndefined
        {
            get
            {
                return row == undefinedValue;
            }
        }

        public IEnumerable<Coordinate2D> FourNeighbors
        {
            get
            {
                yield return new Coordinate2D(row - 1, column);
                yield return new Coordinate2D(row + 1, column);
                yield return new Coordinate2D(row, column - 1);
                yield return new Coordinate2D(row, column + 1);
            }
        }

        public IEnumerable<Coordinate2D> EightNeighbors
        {
            get
            {
                for (int v = -1; v <= 1; v++)
                {
                    for (int h = -1; h <= 1; h++)
                    {
                        if (v != 0 || h != 0)
                        {
                            yield return new Coordinate2D(row + v, column + h);
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return String.Format("Row {0}, Column {1}", row, column);
        }

        public override bool Equals(object obj)
        {
            return Equals((Coordinate2D)obj);
        }

        public override int GetHashCode()
        {
            return row ^ column;
        }

        #region IEquatable<Coordinate2D> Members

        public bool Equals(Coordinate2D other)
        {
            return row == other.row && column == other.column;
        }

        #endregion

        #region IComparable<Coordinate2D> Members

        public int CompareTo(Coordinate2D other)
        {
            return this < other ? -1 : this == other ? 0 : 1;
        }

        #endregion
    }
}
