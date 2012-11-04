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

using Sokoban.Engine.Collections;
using Sokoban.Engine.Core;
using Sokoban.Engine.Levels;

namespace Sokoban.Engine.Deadlocks
{
    class FrozenDeadlockFinder : DeadlockFinder
    {
        private class BlockedMapArray
        {
            private Array2D<bool>[] maps;

            public BlockedMapArray(Level level)
            {
                maps = new Array2D<bool>[2];
                for (int i = 0; i < 2; i++)
                {
                    maps[i] = new Array2D<bool>(level.Height, level.Width);
                }
            }

            public Array2D<bool> this[Axis axis]
            {
                get
                {
                    return maps[(int)axis];
                }
                set
                {
                    maps[(int)axis] = value;
                }
            }
        }

        private List<int> visited;
        private bool[] frozen;

        private BlockedMapArray blockedMaps;

        public FrozenDeadlockFinder(Level level)
            : base(level)
        {
            Initialize();
        }

        public FrozenDeadlockFinder(Level level, Array2D<bool> simpleDeadlockMap)
            : base(level, simpleDeadlockMap)
        {
            Initialize();
        }

        private void Initialize()
        {
            visited = new List<int>();
            frozen = new bool[boxes];
            blockedMaps = new BlockedMapArray(level);
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                blockedMaps[Axis.Vertical][coord] =
                    level.IsWall(coord + Direction.Up) ||
                    level.IsWall(coord + Direction.Down) ||
                    simpleDeadlockMap[coord + Direction.Up] &&
                    simpleDeadlockMap[coord + Direction.Down];
                blockedMaps[Axis.Horizontal][coord] =
                    level.IsWall(coord + Direction.Left) ||
                    level.IsWall(coord + Direction.Right) ||
                    simpleDeadlockMap[coord + Direction.Left] &&
                    simpleDeadlockMap[coord + Direction.Right];
            }
        }

        public override bool IsDeadlocked(int sokobanRow, int sokobanColumn)
        {
            // Iterate over all boxes.
            for (int index = 0; index < boxes; index++)
            {
                int row = boxCoordinates[index].Row;
                int column = boxCoordinates[index].Column;
#if DEBUG
                // The visited stack should be empty.
                if (visited.Count != 0)
                {
                    throw new Exception("visited not empty");
                }
#endif
                // Check if this box if both vertically and horizontally blocked.
                frozen[index] = CheckBlocked(Axis.Vertical, row, column) && CheckBlocked(Axis.Horizontal, row, column);
            }

            // See if any frozen boxes are not on a target
            // and at the same time clear out the frozen
            // array for the next call.
            bool result = false;
            for (int index = 0; index < boxes; index++)
            {
                if (frozen[index])
                {
                    if (!Level.IsTarget(data[boxCoordinates[index].Row, boxCoordinates[index].Column]))
                    {
                        result = true;
                        for (int j = index; j < boxes; j++)
                        {
                            frozen[j] = false;
                        }
                        break;
                    }
                    frozen[index] = false;
                }
            }

#if DEBUG
            // The frozen array should be cleared.
            for (int index = 0; index < boxes; index++)
            {
                if (frozen[index])
                {
                    throw new Exception("frozen array not cleared");
                }
            }
#endif

            return result;
        }

        private bool CheckBlocked(Axis axis, int row, int column)
        {
            // Make sure the square contains a box.
            if (!Level.IsBox(data[row, column]))
            {
                return false;
            }

            // Check whether the square is unconditionally blocked
            // on the specified axis.
            if (blockedMaps[axis][row, column])
            {
                return true;
            }

            // Look up the index of the box on the square.
            int index = level.BoxIndex(row, column);

            // See if we already determined that the box is frozen.
            if (frozen[index])
            {
                return true;
            }

            // Check for four box loops, the only possible kind.
            if (visited.Count >= 4 && visited[visited.Count - 4] == index)
            {
                return true;
            }
#if DEBUG
            // We shouldn't be revisiting any other squares.
            if (visited.Contains(index))
            {
                throw new Exception("previously visited");
            }
#endif

            // Add this index to the stack.
            visited.Add(index);

            // Check recursively for blocked boxes on the opposite axis.
            bool result;
            if (axis == Axis.Vertical)
            {
                // This square is vertically blocked if
                // it has horizontally blocked boxes
                // above or below it.
                result =
                    CheckBlocked(Axis.Horizontal, row - 1, column) ||
                    CheckBlocked(Axis.Horizontal, row + 1, column);
            }
            else
            {
                // This square is horizontally blocked if
                // it has vertically blocked boxes to
                // the left or right of it.
                result =
                    CheckBlocked(Axis.Vertical, row, column - 1) ||
                    CheckBlocked(Axis.Vertical, row, column + 1);
            }

            // Remove this index from the stack.
            visited.RemoveAt(visited.Count - 1);

            // Return the result.
            return result;
        }
    }
}
