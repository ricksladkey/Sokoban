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
    public class IncrementalRegionFinder : RegionFinder
    {
        private Array2D<Cell> data;
        private IncrementalAnyPathFinder pathFinder;
        private int rowLimit;
        private int accessibleSquaresLimit;

        public IncrementalRegionFinder(Level level)
            : base(level)
        {
            this.data = level.Data;
            this.pathFinder = new IncrementalAnyPathFinder(level);
            this.rowLimit = level.Height - 1;
            this.accessibleSquaresLimit = level.InsideSquares - level.Boxes;
        }

        public override IEnumerable<Region> Regions
        {
            get
            {
                int lastAccessibleSquares = 0;
                for (Coordinate2D coord = FindFirst(); !coord.IsUndefined; coord = FindNext())
                {
                    int accessibleSquares = pathFinder.AccessibleSquares - lastAccessibleSquares;
                    lastAccessibleSquares = pathFinder.AccessibleSquares;
                    yield return new Region(coord, accessibleSquares);
                }
            }
        }

        public override IEnumerable<Coordinate2D> Coordinates
        {
            get
            {
                for (Coordinate2D coord = FindFirst(); !coord.IsUndefined; coord = FindNext())
                {
                    yield return coord;
                }
            }
        }

        private Coordinate2D FindFirst()
        {
            // Find the first sokoban coordinate.
            for (int row = 1; row < rowLimit; row++)
            {
                int[] columns = insideCoordinates[row];
                int n = columns.Length;
                for (int i = 0; i < n; i++)
                {
                    int column = columns[i];
                    if (!Level.IsBox(data[row, column]))
                    {
                        pathFinder.Find(row, column);
                        return new Coordinate2D(row, column);
                    }
                }
            }
            return Coordinate2D.Undefined;
        }

        private Coordinate2D FindNext()
        {
            // Shortcut for the last region: all squares searched.
            // Requires getting accessible squares to be a constant
            // time operation to get the speed up.
            if (pathFinder.AccessibleSquares == accessibleSquaresLimit)
            {
                return Coordinate2D.Undefined;
            }

            // Find the next sokoban coordinate.
            for (int row = 1; row < rowLimit; row++)
            {
                int[] columns = insideCoordinates[row];
                int n = columns.Length;
                for (int i = 0; i < n; i++)
                {
                    int column = columns[i];
                    if (!Level.IsBox(data[row, column]) && !pathFinder.IsAccessible(row, column))
                    {
                        pathFinder.ContinueFinding(row, column);
                        return new Coordinate2D(row, column);
                    }
                }
            }
            return Coordinate2D.Undefined;
        }
    }
}
