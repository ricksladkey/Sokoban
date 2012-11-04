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

using Sokoban.Engine.Core;

namespace Sokoban.Engine.Levels
{
    public class CellData : IEnumerable<Cell>, IEquatable<Level>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected Array2D<Cell> data;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected int levelHeight;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected int levelWidth;

        protected CellData()
        {
        }

        public CellData(Array2D<Cell> data)
        {
            this.data = data;
            levelHeight = data.Height;
            levelWidth = data.Width;
        }

        public int Height
        {
            get
            {
                return levelHeight;
            }
        }

        public int Width
        {
            get
            {
                return levelWidth;
            }
        }

        public Array2D<Cell> Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
                levelHeight = data.Height;
                levelWidth = data.Width;
            }
        }

        public Cell this[int row, int column]
        {
            get
            {
                return data[row, column];
            }
            set
            {
                data[row, column] = value;
            }
        }

        public Cell this[Coordinate2D coord]
        {
            get
            {
                return data[coord];
            }
            set
            {
                data[coord.Row, coord.Column] = value;
            }
        }

        public bool IsValid(int row, int column)
        {
            return row >= 0 && row < levelHeight && column >= 0 && column < levelWidth;
        }

        public bool IsValid(Coordinate2D coord)
        {
            return IsValid(coord.Row, coord.Column);
        }

        public static bool IsFloor(Cell cell)
        {
            return (cell & Cell.Wall) == 0;
        }

        public bool IsFloor(int row, int column)
        {
            return IsValid(row, column) && IsFloor(data[row, column]);
        }

        public bool IsFloor(Coordinate2D coord)
        {
            return IsFloor(coord.Row, coord.Column);
        }

        public static bool IsJustFloor(Cell cell)
        {
            return (cell & (Cell.Box | Cell.Sokoban | Cell.Wall | Cell.Target)) == 0;
        }

        public bool IsJustFloor(int row, int column)
        {
            return IsValid(row, column) && IsJustFloor(data[row, column]);
        }

        public bool IsJustFloor(Coordinate2D coord)
        {
            return IsJustFloor(coord.Row, coord.Column);
        }

        public static bool IsEmpty(Cell cell)
        {
            return (cell & (Cell.Box | Cell.Sokoban | Cell.Wall)) == 0;
        }

        public bool IsEmpty(Coordinate2D coord)
        {
            return IsEmpty(coord.Row, coord.Column);
        }

        public bool IsEmpty(int row, int column)
        {
            return IsValid(row, column) && IsEmpty(data[row, column]);
        }

        public static bool IsOccupied(Cell cell)
        {
            return (cell & (Cell.Box | Cell.Sokoban)) != 0;
        }

        public bool IsOccupied(int row, int column)
        {
            return IsValid(row, column) && IsOccupied(data[row, column]);
        }

        public bool IsOccupied(Coordinate2D coord)
        {
            return IsOccupied(coord.Row, coord.Column);
        }

        public static bool IsSokobanOrEmpty(Cell cell)
        {
            return (cell & (Cell.Wall | Cell.Box)) == 0;
        }

        public bool IsSokobanOrEmpty(int row, int column)
        {
            return IsSokobanOrEmpty(data[row, column]);
        }

        public bool IsSokobanOrEmpty(Coordinate2D coord)
        {
            return IsSokobanOrEmpty(coord.Row, coord.Column);
        }

        public static bool IsTarget(Cell cell)
        {
            return (cell & Cell.Target) != 0;
        }

        public bool IsTarget(int row, int column)
        {
            return IsValid(row, column) && IsTarget(data[row, column]);
        }

        public bool IsTarget(Coordinate2D coord)
        {
            return IsTarget(coord.Row, coord.Column);
        }

        public static bool IsSokoban(Cell cell)
        {
            return (cell & Cell.Sokoban) != 0;
        }

        public bool IsSokoban(int row, int column)
        {
            return IsValid(row, column) && IsSokoban(data[row, column]);
        }

        public bool IsSokoban(Coordinate2D coord)
        {
            return IsSokoban(coord.Row, coord.Column);
        }

        public static bool IsBox(Cell cell)
        {
            return (cell & Cell.Box) != 0;
        }

        public bool IsBox(int row, int column)
        {
            return IsValid(row, column) && IsBox(data[row, column]);
        }

        public bool IsBox(Coordinate2D coord)
        {
            return IsBox(coord.Row, coord.Column);
        }

        public static bool IsBoxOrTarget(Cell cell)
        {
            return (cell & (Cell.Box | Cell.Target)) != 0;
        }

        public bool IsBoxOrTarget(int row, int column)
        {
            return IsValid(row, column) && IsBoxOrTarget(data[row, column]);
        }

        public bool IsBoxOrTarget(Coordinate2D coord)
        {
            return IsBoxOrTarget(coord.Row, coord.Column);
        }

        public static bool IsSokobanOrBox(Cell cell)
        {
            return (cell & (Cell.Box | Cell.Sokoban)) != 0;
        }

        public bool IsSokobanOrBox(int row, int column)
        {
            return IsValid(row, column) && IsSokobanOrBox(data[row, column]);
        }

        public bool IsSokobanOrBox(Coordinate2D coord)
        {
            return IsSokobanOrBox(coord.Row, coord.Column);
        }

        public static bool IsSokobanBoxOrTarget(Cell cell)
        {
            return (cell & (Cell.Sokoban | Cell.Box | Cell.Target)) != 0;
        }

        public bool IsSokobanBoxOrTarget(int row, int column)
        {
            return IsValid(row, column) && IsSokobanBoxOrTarget(data[row, column]);
        }

        public bool IsSokobanBoxOrTarget(Coordinate2D coord)
        {
            return IsSokobanBoxOrTarget(coord.Row, coord.Column);
        }

        private bool IsDisplacedBox(Cell cell)
        {
            return (cell & (Cell.Box | Cell.Target)) == Cell.Box;
        }

        public bool IsDisplacedBox(Coordinate2D coord)
        {
            return IsDisplacedBox(coord.Row, coord.Column);
        }

        public bool IsDisplacedBox(int row, int column)
        {
            return IsValid(row, column) && IsDisplacedBox(data[row, column]);
        }

        private bool IsInplaceBox(Cell cell)
        {
            return (cell & (Cell.Box | Cell.Target)) == (Cell.Box | Cell.Target);
        }

        public bool IsInplaceBox(int row, int column)
        {
            return IsValid(row, column) && IsInplaceBox(data[row, column]);
        }

        public bool IsInplaceBox(Coordinate2D coord)
        {
            return IsInplaceBox(coord.Row, coord.Column);
        }

        public static bool IsWall(Cell cell)
        {
            return (cell & Cell.Wall) != 0;
        }

        public bool IsWall(int row, int column)
        {
            return IsValid(row, column) && IsWall(data[row, column]);
        }

        public bool IsWall(Coordinate2D coord)
        {
            return IsWall(coord.Row, coord.Column);
        }

        public static bool IsWallOrEmpty(Cell cell)
        {
            return (cell & (Cell.Sokoban | Cell.Box)) == 0;
        }

        public bool IsWallOrEmpty(int row, int column)
        {
            return IsValid(row, column) && IsWallOrEmpty(data[row, column]);
        }

        public bool IsWallOrEmpty(Coordinate2D coord)
        {
            return IsWallOrEmpty(coord.Row, coord.Column);
        }

        public static bool IsWallOrBox(Cell cell)
        {
            return (cell & (Cell.Wall | Cell.Box)) != 0;
        }

        public bool IsWallOrBox(int row, int column)
        {
            return IsValid(row, column) && IsWallOrBox(data[row, column]);
        }

        public bool IsWallOrBox(Coordinate2D coord)
        {
            return IsWallOrBox(coord.Row, coord.Column);
        }

        public static bool IsWallOrOutside(Cell cell)
        {
            return (cell & (Cell.Wall | Cell.Outside)) != 0;
        }

        public bool IsWallOrOutside(int row, int column)
        {
            return IsValid(row, column) && IsWallOrOutside(data[row, column]);
        }

        public bool IsWallOrOutside(Coordinate2D coord)
        {
            return IsWallOrOutside(coord.Row, coord.Column);
        }

        public static bool IsAccessible(Cell cell)
        {
            return (cell & Cell.Accessible) != 0;
        }

        public bool IsAccessible(int row, int column)
        {
            return IsValid(row, column) && IsAccessible(data[row, column]);
        }

        public bool IsAccessible(Coordinate2D coord)
        {
            return IsAccessible(coord.Row, coord.Column);
        }

        public static bool IsInside(Cell cell)
        {
            return (cell & (Cell.Wall | Cell.Outside)) == 0;
        }

        public bool IsInside(int row, int column)
        {
            return IsValid(row, column) && IsInside(data[row, column]);
        }

        public bool IsInside(Coordinate2D coord)
        {
            return IsInside(coord.Row, coord.Column);
        }

        public static bool IsOutside(Cell cell)
        {
            return (cell & Cell.Outside) != 0;
        }

        public bool IsOutside(int row, int column)
        {
            return IsOutside(data[row, column]);
        }

        public bool IsOutside(Coordinate2D coord)
        {
            return IsOutside(coord.Row, coord.Column);
        }

        public bool IsEdge(int row, int column)
        {
            return row == 0 || row == levelHeight - 1 || column == 0 || column == levelWidth - 1;
        }

        public bool IsEdge(Coordinate2D coord)
        {
            return IsEdge(coord.Row, coord.Column);
        }

        public bool IsEdgeWall(int row, int column)
        {
            return IsEdge(row, column) && IsWall(row, column);
        }

        public bool IsEdgeWall(Coordinate2D coord)
        {
            return IsEdgeWall(coord.Row, coord.Column);
        }

        public bool IsEdgeFloor(int row, int column)
        {
            return IsEdge(row, column) && IsFloor(row, column);
        }

        public bool IsEdgeFloor(Coordinate2D coord)
        {
            return IsEdgeFloor(coord.Row, coord.Column);
        }

        public bool IsCriticalWall(int row, int column)
        {
            return IsCriticalWall(new Coordinate2D(row, column));
        }

        public bool IsCriticalWall(Coordinate2D coord)
        {
            if (!IsWall(coord))
            {
                return false;
            }

            int inside = 0;
            int outside = 0;
            foreach (Coordinate2D neighbor in coord.FourNeighbors)
            {
                if (!IsValid(neighbor) || IsOutside(neighbor))
                {
                    outside++;
                }
                else if (!IsWallOrOutside(neighbor))
                {
                    inside++;
                }
            }

            return inside > 0 && outside > 0;
        }

        public bool IsOutsideWall(int row, int column)
        {
            return IsOutsideWall(new Coordinate2D(row, column));
        }

        public bool IsOutsideWall(Coordinate2D coord)
        {
            if (!IsWall(coord))
            {
                return false;
            }

            int inside = 0;
            int outside = 0;
            foreach (Coordinate2D neighbor in coord.EightNeighbors)
            {
                if (!IsValid(neighbor) || IsOutside(neighbor))
                {
                    outside++;
                }
                else if (!IsWallOrOutside(neighbor))
                {
                    inside++;
                }
            }

            return inside > 0 && outside > 0;
        }

        public HashKey GetHashKey(int row, int column)
        {
            return HashKey.GetHashKey(row, column, data[row, column]);
        }

        public HashKey GetHashKey(Coordinate2D coord)
        {
            return HashKey.GetHashKey(coord.Row, coord.Column, data[coord.Row, coord.Column]);
        }

        public HashKey GetHashKey()
        {
            HashKey hashKey = 0;
            for (int row = 0; row < levelHeight; row++)
            {
                for (int column = 0; column < levelWidth; column++)
                {
                    hashKey ^= HashKey.GetHashKey(row, column, data[row, column]);
                }
            }
            return hashKey;
        }

        public override int GetHashCode()
        {
            return (int)GetHashKey();
        }

        public override bool Equals(object obj)
        {
            Level other = obj as Level;
            if (other == null)
            {
                return false;
            }
            return Equals(other);
        }

        #region IEnumerable<Coordinate2D> Members

        public IEnumerator<Cell> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IEquatable<Level> Members

        public bool Equals(Level other)
        {
            return data.Equals(other.data);
        }

        #endregion
    }
}
