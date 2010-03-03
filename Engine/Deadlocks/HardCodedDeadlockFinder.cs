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
    public class HardCodedDeadlockFinder : DeadlockFinder
    {
        public HardCodedDeadlockFinder(Level level)
            : base(level)
        {
        }

        public HardCodedDeadlockFinder(Level level, Array2D<bool> simpleDeadlockMap)
            : base(level, simpleDeadlockMap)
        {
        }

        public override bool IsDeadlocked(int sokobanRow, int sokobanColumn)
        {
            // XXX: Currently misses deadlocks like this:
            // The cells are:
            // N1 C1
            // N2 C2
            // and the position matches:
            // X$    #$    X$
            // #$ or X$ or X$
            // Where X is statically deadlocked.

            for (int i = 0; i < boxes; i++)
            {
                Coordinate2D coord = boxCoordinates[i];
                int row = coord.Row;
                int column = coord.Column;

                if (CheckForDeadlocksHorizontally(row, column))
                {
                    return true;
                }
                if (CheckForDeadlocksVertically(row, column))
                {
                    return true;
                }

                // If there is no sokoban we cannot check for corral deadlocks.
                if (sokobanRow != 0 && CheckForCorralDeadlocks(sokobanRow, sokobanColumn, row, column))
                {
                    return true;
                }
            }
            return false;
        }

        protected bool CheckForDeadlocksHorizontally(int row, int column)
        {
            // Detect two boxes that are frozen, at least one of which is displaced.
            Cell[] dataRow = data[row];
            Cell cell1 = dataRow[column];
            Cell cell2 = dataRow[column + 1];
            if (!Level.IsBox(cell2))
            {
                return false;
            }
            if (Level.IsTarget(cell1) && Level.IsTarget(cell2))
            {
                return false;
            }

            Cell[] previousDataRow = data[row - 1];
            Cell neighbor1 = previousDataRow[column];
            Cell neighbor2 = previousDataRow[column + 1];
            bool neighbor1IsWall = Level.IsWall(neighbor1);
            bool neighbor1IsBox = Level.IsBox(neighbor1);
            bool neighbor2IsWall = Level.IsWall(neighbor2);
            bool neighbor2IsBox = Level.IsBox(neighbor2);
            if ((neighbor1IsWall | neighbor1IsBox) & (neighbor2IsWall | neighbor2IsBox))
            {
                // The cells are:
                // N1 C1
                // N2 C2
                // and the position matches:
                // #$    #$    $$    $$
                // #$ or $$ or #$ or $$
                return true;
            }
            Cell[] nextDataRow = data[row + 1];
            Cell neighbor3 = nextDataRow[column];
            Cell neighbor4 = nextDataRow[column + 1];
            bool neighbor3IsWall = Level.IsWall(neighbor3);
            bool neighbor3IsBox = Level.IsBox(neighbor3);
            bool neighbor4IsWall = Level.IsWall(neighbor4);
            bool neighbor4IsBox = Level.IsBox(neighbor4);
            if ((neighbor3IsWall | neighbor3IsBox) & (neighbor4IsWall | neighbor4IsBox))
            {
                // The cells are:
                // C1 N3
                // C2 N4
                // and the position matches:
                // $#    $#    $$    $$
                // $# or $$ or $# or $$
                return true;
            }

            if (neighbor1IsWall && neighbor4IsWall)
            {
                // The cells are:
                // N1 C1
                //    C2 N4
                // and the position matches:
                // #$
                //  $#
                return true;
            }
            if (neighbor2IsWall && neighbor3IsWall)
            {
                // The cells are:
                //    C1 N3
                // N2 C2
                // and the position matches:
                //  $#
                // #$
                return true;
            }

            if (neighbor1IsBox && neighbor4IsWall)
            {
                if (Level.IsWall(previousDataRow[column - 1]))
                {
                    // The cells are:
                    // DI
                    // N1 C1
                    //    C2 N4
                    // and the position matches:
                    // #
                    // $$
                    //  $#
                    return true;
                }
            }

            // This pattern doesn't reflect across the diagonal so
            // it requires two cases which are checked only for the first
            // call to CheckForDeadlocks.
            if (neighbor2IsBox && neighbor3IsWall)
            {
                if (Level.IsWall(previousDataRow[column + 2]))
                {
                    // The cells are:
                    //    C1 N3
                    // N2 C2
                    // DI
                    // and the position matches:
                    //  $#
                    // $$
                    // #
                    return true;
                }
            }
            if (neighbor2IsWall && neighbor3IsBox)
            {
                if (Level.IsWall(nextDataRow[column - 1]))
                {
                    // The cells are:
                    //       DI
                    //    C1 N3
                    // N2 C2
                    // and the position matches:
                    //   #
                    //  $$
                    // #$
                    return true;
                }
            }

            return false;
        }

        protected bool CheckForDeadlocksVertically(int row, int column)
        {
            // Detect two boxes that are frozen, at least one of which is displaced.
            Cell[] dataRow = data[row];
            Cell[] nextDataRow = data[row + 1];
            Cell cell1 = dataRow[column];
            Cell cell2 = nextDataRow[column];
            if (!Level.IsBox(cell2))
            {
                return false;
            }
            if (Level.IsTarget(cell1) && Level.IsTarget(cell2))
            {
                return false;
            }

            Cell neighbor1 = dataRow[column - 1];
            Cell neighbor2 = nextDataRow[column - 1];
            bool neighbor1IsWall = Level.IsWall(neighbor1);
            bool neighbor1IsBox = Level.IsBox(neighbor1);
            bool neighbor2IsWall = Level.IsWall(neighbor2);
            bool neighbor2IsBox = Level.IsBox(neighbor2);
            if ((neighbor1IsWall | neighbor1IsBox) & (neighbor2IsWall | neighbor2IsBox))
            {
                // The cells are:
                // N1 C1
                // N2 C2
                // and the position matches:
                // #$    #$    $$    $$
                // #$ or $$ or #$ or $$
                return true;
            }
            Cell neighbor3 = dataRow[column + 1];
            Cell neighbor4 = nextDataRow[column + 1];
            bool neighbor3IsWall = Level.IsWall(neighbor3);
            bool neighbor3IsBox = Level.IsBox(neighbor3);
            bool neighbor4IsWall = Level.IsWall(neighbor4);
            bool neighbor4IsBox = Level.IsBox(neighbor4);
            if ((neighbor3IsWall | neighbor3IsBox) & (neighbor4IsWall | neighbor4IsBox))
            {
                // The cells are:
                // C1 N3
                // C2 N4
                // and the position matches:
                // $#    $#    $$    $$
                // $# or $$ or $# or $$
                return true;
            }

            if (neighbor1IsWall && neighbor4IsWall)
            {
                // The cells are:
                // N1 C1
                //    C2 N4
                // and the position matches:
                // #$
                //  $#
                return true;
            }
            if (neighbor2IsWall && neighbor3IsWall)
            {
                // The cells are:
                //    C1 N3
                // N2 C2
                // and the position matches:
                //  $#
                // #$
                return true;
            }

            if (neighbor1IsBox && neighbor4IsWall)
            {
                if (Level.IsWall(data[row - 1, column - 1]))
                {
                    // The cells are:
                    // DI
                    // N1 C1
                    //    C2 N4
                    // and the position matches:
                    // #
                    // $$
                    //  $#
                    return true;
                }
            }

            return false;
        }

        protected bool CheckForCorralDeadlocks(int sokobanRow, int sokobanColumn, int row, int column)
        {
            Cell cell1 = data[row, column];
            bool cell1IsTarget = Level.IsTarget(cell1);

            Cell cell2 = data[row + 1, column - 1];
            if (Level.IsBox(cell2) && (!cell1IsTarget || !Level.IsTarget(cell2)) &&
                (Level.IsWall(data[row + 1, column - 2]) &&
                Level.IsWall(data[row, column - 2]) &&
                Level.IsWall(data[row - 1, column - 1]) &&
                Level.IsWall(data[row - 1, column]) &&
                !(sokobanRow == row && sokobanColumn == column - 1) ||
                Level.IsWall(data[row, column + 1]) &&
                Level.IsWall(data[row + 1, column + 1]) &&
                Level.IsWall(data[row + 2, column - 1]) &&
                Level.IsWall(data[row + 2, column]) &&
                !(sokobanRow == row + 1 && sokobanColumn == column)))
            {
                return true;
            }

            Cell cell3 = data[row + 1, column + 1];
            if (Level.IsBox(cell3) && (!cell1IsTarget || !Level.IsTarget(cell3)) &&
                (Level.IsWall(data[row - 1, column]) &&
                Level.IsWall(data[row - 1, column + 1]) &&
                Level.IsWall(data[row, column + 2]) &&
                Level.IsWall(data[row + 1, column + 2]) &&
                !(sokobanRow == row && sokobanColumn == column + 1) ||
                Level.IsWall(data[row, column - 1]) &&
                Level.IsWall(data[row + 1, column - 1]) &&
                Level.IsWall(data[row + 2, column]) &&
                Level.IsWall(data[row + 2, column + 1]) &&
                !(sokobanRow == row + 1 && sokobanColumn == column)))
            {
                return true;
            }

            return false;
        }
    }
}
