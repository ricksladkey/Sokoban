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
    public abstract class ArrayPathFinder : PathFinder
    {
        protected Array2D<Cell> data;
        protected Coordinate2D[] boxCoordinates;
        protected int boxCount;
        protected int n;
        protected int m;
        protected int firstInside;
        protected int lastInside;
        protected int insideCount;
        protected FixedQueue<int> q;
        protected int[][] neighborMap;

        protected ArrayPathFinder(Level level)
            : base(level)
        {
            data = level.Data;
            boxCoordinates = level.BoxCoordinates;
            boxCount = boxCoordinates.Length;
            n = level.Width;
            m = level.Height * level.Width;
            q = new FixedQueue<int>(m);

            firstInside = -1;
            lastInside = -1;
            neighborMap = new int[m][];
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                lastInside = coord.Row * n + coord.Column;
                if (firstInside == -1)
                {
                    firstInside = lastInside;
                }

                List<int> neighbors = new List<int>();
                foreach (Coordinate2D neighbor in coord.FourNeighbors)
                {
                    if (level.IsFloor(neighbor))
                    {
                        neighbors.Add(neighbor.Row * n + neighbor.Column);
                    }
                }
                neighborMap[coord.Row * n + coord.Column] = neighbors.ToArray();
            }
            insideCount = lastInside - firstInside + 1;
        }
    }
}
