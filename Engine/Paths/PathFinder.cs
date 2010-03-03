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

using Sokoban.Engine.Core;
using Sokoban.Engine.Levels;

namespace Sokoban.Engine.Paths
{
    public abstract class PathFinder
    {
        // Factory for creating deadlock finders.
        public static PathFinder CreateInstance(Level level, bool calculateDistance, bool incremental)
        {
            if (calculateDistance && incremental)
            {
                throw new InvalidOperationException("calculate distance and incremental not both supported");
            }

            if (incremental)
            {
                return new IncrementalAnyPathFinder(level);
            }

            // Choose the fastest finder that is appropriate for the task.
            if (calculateDistance)
            {
                if (level.InsideSquares <= SmallLevelShortestPathFinder.MaximumInsideSquares)
                {
                    // The small shortest path level finder is faster for small levels.
                    return new SmallLevelShortestPathFinder(level);
                }

                // Otherwise use the general shortest path finder that calculates distances.
                return new ShortestPathFinder(level);
            }

#if false
            if (level.InsideSquares <= SmallLevelAnyPathFinder.MaximumInsideSquares)
            {
                // The small level any path finder is not faster for small levels.
                return new SmallLevelAnyPathFinder(level);
            }
#endif

            // Otherwise use the general any path finder that doesn't calculate distances.
            return new AnyPathFinder(level);
        }

        public static PathFinder CreateInstance(Level level, bool calculateDistance)
        {
            return CreateInstance(level, calculateDistance, false);
        }

        public static PathFinder CreateInstance(Level level)
        {
            return CreateInstance(level, false);
        }

        protected const int DefaultInaccessible = int.MaxValue;

        protected Level level;
        protected int DerivedInaccessible;

        protected PathFinder(Level level)
        {
            this.level = level;
            this.DerivedInaccessible = DefaultInaccessible;
        }

        public string AsText
        {
            get
            {
                Array2D<int> distanceMap = new Array2D<int>(level.Height, level.Width);
                foreach (Coordinate2D coord in level.InsideCoordinates)
                {
                    distanceMap[coord] = GetDistance(coord);
                }
                return LevelUtils.MapAsText(level, distanceMap);
            }
        }

        public int Inaccessible
        {
            get
            {
                return DerivedInaccessible;
            }
        }

        public abstract void Find(int row, int column);

        public void Find(Coordinate2D coord)
        {
            Find(coord.Row, coord.Column);
        }

        public void Find()
        {
            Find(level.SokobanRow, level.SokobanColumn);
        }

        public void IncrementalFind()
        {
            Find(level.SokobanRow, level.SokobanColumn);
        }

        public void IncrementalFind(Coordinate2D coordinate)
        {
            Find(coordinate.Row, coordinate.Column);
        }

        public virtual void IncrementalFind(int row, int column)
        {
            Find(row, column);
        }

        public virtual void ForceFullCalculation()
        {
        }

        public virtual bool IsAccessible(int row, int column)
        {
            return GetDistance(row, column) < DerivedInaccessible;
        }

        public bool IsAccessible(Coordinate2D coord)
        {
            return IsAccessible(coord.Row, coord.Column);
        }

        public void MoveBox(Operation operation, Coordinate2D oldBoxCoordinate, Coordinate2D newBoxCoordinate)
        {
            MoveBox(operation, oldBoxCoordinate.Row, oldBoxCoordinate.Column, newBoxCoordinate.Row, newBoxCoordinate.Column);
        }

        public virtual void MoveBox(Operation operation, int oldBoxRow, int oldBoxColumn, int newBoxRow, int newBoxColumn)
        {
            level.MoveBox(oldBoxRow, oldBoxColumn, newBoxRow, newBoxColumn);
        }

        public void MoveBoxAndIncrementalFind(Operation operation, Coordinate2D oldBoxCoordinate, Coordinate2D newBoxCoordinate)
        {
            MoveBoxAndIncrementalFind(operation, oldBoxCoordinate.Row, oldBoxCoordinate.Column, newBoxCoordinate.Row, newBoxCoordinate.Column);
        }

        public virtual void MoveBoxAndIncrementalFind(Operation operation, int oldBoxRow, int oldBoxColumn, int newBoxRow, int newBoxColumn)
        {
            level.MoveBox(oldBoxRow, oldBoxColumn, newBoxRow, newBoxColumn);
            if (operation == Operation.Push)
            {
                Find(oldBoxRow, oldBoxColumn);
            }
            else if (operation == Operation.Pull)
            {
                int parallelRow = newBoxRow - oldBoxRow;
                int parallelColumn = newBoxColumn - oldBoxColumn;
                Find(newBoxRow + parallelRow, newBoxColumn + parallelColumn);
            }
        }

        public abstract int GetDistance(int row, int column);

        public int GetDistance(Coordinate2D coord)
        {
            return GetDistance(coord.Row, coord.Column);
        }

        public virtual IEnumerable<Coordinate2D> AccessibleCoordinates
        {
            get
            {
                foreach (Coordinate2D coord in level.InsideCoordinates)
                {
                    if (IsAccessible(coord))
                    {
                        yield return coord;
                    }
                }
            }
        }

        public virtual int AccessibleSquares
        {
            get
            {
                int count = 0;
                foreach (Coordinate2D coord in level.InsideCoordinates)
                {
                    if (IsAccessible(coord))
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public virtual Coordinate2D GetFirstAccessibleCoordinate()
        {
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                if (IsAccessible(coord.Row, coord.Column))
                {
                    return coord;
                }
            }
            return Coordinate2D.Undefined;
        }

        public virtual MoveList GetPath(int row, int column)
        {
            MoveList path = new MoveList();
            Coordinate2D coord = new Coordinate2D(row, column);
            while (GetDistance(coord) > 0)
            {
                foreach (Coordinate2D neighbor in coord.FourNeighbors)
                {
                    if (GetDistance(neighbor) + 1 == GetDistance(coord))
                    {
                        path.Add(new OperationDirectionPair(Levels.Operation.Move, GetDirection(neighbor, coord)));
                        coord = neighbor;
                        break;
                    }
                }
            }
            path.Reverse();
            return path;
        }

        public MoveList GetPath(Coordinate2D coord)
        {
            return GetPath(coord.Row, coord.Column);
        }

        public int FindAndGetDistance(int srcRow, int srcCol, int dstRow, int dstCol)
        {
            Find(srcRow, srcCol);
            return GetDistance(dstRow, dstCol);
        }

        public int FindAndGetDistance(Coordinate2D srcCoord, Coordinate2D dstCoord)
        {
            Find(srcCoord.Row, srcCoord.Column);
            return GetDistance(dstCoord.Row, dstCoord.Column);
        }

        public MoveList FindAndGetPath(int srcRow, int srcCol, int dstRow, int dstCol)
        {
            Find(srcRow, srcCol);
            return GetPath(dstRow, dstCol);
        }

        public MoveList FindAndGetPath(Coordinate2D srcCoord, Coordinate2D dstCoord)
        {
            Find(srcCoord.Row, srcCoord.Column);
            return GetPath(dstCoord.Row, dstCoord.Column);
        }

        public MoveList FindAndGetPath(int row, int column)
        {
            Find(level.SokobanRow, level.SokobanColumn);
            return GetPath(row, column);
        }

        public MoveList FindAndGetPath(Coordinate2D coord)
        {
            Find(level.SokobanRow, level.SokobanColumn);
            return GetPath(coord.Row, coord.Column);
        }

        protected static Direction GetDirection(Coordinate2D srcCoord, Coordinate2D dstCoord)
        {
            int rowDelta = dstCoord.Row - srcCoord.Row;
            int colDelta = dstCoord.Column - srcCoord.Column;
            if (Math.Abs(rowDelta) + Math.Abs(colDelta) != 1)
            {
                throw new InvalidOperationException("invalid coordinates");
            }
            if (rowDelta == -1)
            {
                return Direction.Up;
            }
            if (rowDelta == 1)
            {
                return Direction.Down;
            }
            if (colDelta == -1)
            {
                return Direction.Left;
            }
            if (colDelta == 1)
            {
                return Direction.Right;
            }
            throw new InvalidOperationException("invalid coordinates");
        }
    }
}
