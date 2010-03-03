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
    /// <summary>
    /// A region finder is an object that can enumerate
    /// all the disconnected regions of a level position.
    /// Each region is defined by the lexicographically
    /// first square in the region and the count of
    /// squares accessible from that square.
    /// </summary>
    public class BruteForceRegionFinder : RegionFinder
    {
        private PathFinder pathFinder;

        private Array2D<bool> tried;

        public BruteForceRegionFinder(Level level)
            : base(level)
        {
            this.pathFinder = PathFinder.CreateInstance(level);

            // Initialize map of coordinates already tried.
            tried = new Array2D<bool>(level.Height, level.Width);
        }

        public override IEnumerable<Region> Regions
        {
            get
            {
                // Clear the map of all squares tried as the sokoban coordinate.
                tried.SetAll(false);

                // Avoid the boxes.
                for (int i = 0; i < boxes; i++)
                {
                    tried[boxCoordinates[i].Row, boxCoordinates[i].Column] = true;
                }

                // Find the first sokoban coordinate.
                int sokobanRow = -1;
                int sokobanColumn = -1;
                int rowLimit = level.Height - 1;
                for (int row = 1; row < rowLimit; row++)
                {
                    bool[] triedRow = tried[row];
                    int[] columns = insideCoordinates[row];
                    int n = columns.Length;
                    for (int i = 0; i < n; i++)
                    {
                        if (!triedRow[columns[i]])
                        {
                            sokobanRow = row;
                            sokobanColumn = columns[i];
                            goto tryCoordinate;
                        }
                    }
                }

            tryCoordinate:

                // Iterate over all discontiguous regions.
                Region region = new Region();
                while (sokobanRow != -1)
                {
                    // Find accessible coordinates from this square.
                    region.Coordinate = new Coordinate2D(sokobanRow, sokobanColumn);
                    pathFinder.Find(sokobanRow, sokobanColumn);

                    // Mark all accessible squares as tried
                    // and at the same time find the first
                    // remaining untried square.
                    sokobanRow = -1;
                    sokobanColumn = -1;
                    int count = 0;
                    for (int row = 1; row < rowLimit; row++)
                    {
                        bool[] triedRow = tried[row];
                        int[] columns = insideCoordinates[row];
                        int n = columns.Length;
                        if (sokobanRow == -1)
                        {
                            for (int i = 0; i < n; i++)
                            {
                                int column = columns[i];
                                if (pathFinder.IsAccessible(row, column))
                                {
                                    tried[row, column] = true;
                                    count++;
                                }
                                else if (sokobanRow == -1 && !triedRow[column])
                                {
                                    sokobanRow = row;
                                    sokobanColumn = column;
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < n; i++)
                            {
                                int column = columns[i];
                                if (pathFinder.IsAccessible(row, column))
                                {
                                    tried[row, column] = true;
                                    count++;
                                }
                            }
                        }
                    }
                    region.Count = count;

                    // Return this region.
                    yield return region;
                }
            }
        }
    }
}
