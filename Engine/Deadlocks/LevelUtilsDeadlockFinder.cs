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
    public class LevelUtilsDeadlockFinder : DeadlockFinder
    {
        public LevelUtilsDeadlockFinder(Level level, Array2D<bool> simpleDeadlockMap)
            : base(level, simpleDeadlockMap)
        {
        }

        public override bool IsDeadlocked(int sokobanRow, int sokobanColumn)
        {
            Array2D<Cell> data = level.Data;
            Coordinate2D[] boxCoordinates = level.BoxCoordinates;
            int n = boxCoordinates.Length;
            for (int i = 0; i < n; i++)
            {
                Coordinate2D coord = boxCoordinates[i];
                int row = coord.Row;
                int column = coord.Column;
                Cell cell1 = data[row, column];

                if (CheckForDeadlocks(data, cell1, row, column, 0, 1))
                {
                    return true;
                }
                if (CheckForDeadlocks(data, cell1, row, column, 1, 0))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool CheckForDeadlocks(Array2D<Cell> data, Cell cell1, int row, int column, int v, int h)
        {
            // Detect two boxes that are frozen, at least one of which is displaced.
            Cell cell2 = data[row + h, column + v];
            if (!Level.IsBox(cell2))
            {
                return false;
            }
            if (Level.IsTarget(cell1) && Level.IsTarget(cell2))
            {
                return false;
            }

            Cell neighbor1 = data[row - v, column - h];
            Cell neighbor2 = data[row + h - v, column + v - h];
            if (Level.IsWallOrBox(neighbor1) && Level.IsWallOrBox(neighbor2))
            {
                // The cells are:
                // N1 C1
                // N2 C2
                // and the position matches:
                // #$    #$    $$    $$
                // #$ or $$ or #$ or $$
                return true;
            }
            Cell neighbor3 = data[row + v, column + h];
            Cell neighbor4 = data[row + h + v, column + v + h];
            if (Level.IsWallOrBox(neighbor3) && Level.IsWallOrBox(neighbor4))
            {
                // The cells are:
                // C1 N3
                // C2 N4
                // and the position matches:
                // $#    $#    $$    $$
                // $# or $$ or $# or $$
                return true;
            }

            if (Level.IsWall(neighbor1) && Level.IsWall(neighbor4))
            {
                // The cells are:
                // N1 C1
                //    C2 N4
                // and the position matches:
                // #$
                //  $#
                return true;
            }
            if (Level.IsWall(neighbor2) && Level.IsWall(neighbor3))
            {
                // The cells are:
                //    C1 N3
                // N2 C2
                // and the position matches:
                //  $#
                // #$
                return true;
            }

            if (Level.IsBox(neighbor1) && Level.IsWall(neighbor4))
            {
                Cell diagonal = data[row - h - v, column - v - h];
                if (Level.IsWall(diagonal))
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
            if (h == 0)
            {
                // This pattern doesn't reflect across the diagonal so
                // it requires two cases which are checked only for the first
                // call to CheckForDeadlocks.
                if (Level.IsBox(neighbor2) && Level.IsWall(neighbor3))
                {
                    Cell diagonal = data[row + 2 * h - v, column + 2 * v - h];
                    if (Level.IsWall(diagonal))
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
                if (Level.IsWall(neighbor2) && Level.IsBox(neighbor3))
                {
                    Cell diagonal = data[row - h + v, column - v + h];
                    if (Level.IsWall(diagonal))
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
            }

            return false;
        }
    }
}
