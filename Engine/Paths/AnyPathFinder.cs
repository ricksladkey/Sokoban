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
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Sokoban.Engine.Core;
using Sokoban.Engine.Levels;

namespace Sokoban.Engine.Paths
{
    public class AnyPathFinder : ArrayPathFinder
    {
        private int[][] insideCoordinates;
        private bool[] visited;

        public AnyPathFinder(Level level)
            : base(level)
        {
            this.insideCoordinates = level.InsideCoordinates;
            this.visited = new bool[m];
        }

        #region PathFinder Members

        public override void Find(int row, int column)
        {
            Array.Clear(visited, firstInside, insideCount);
            for (int i = 0; i < boxCount; i++)
            {
                visited[boxCoordinates[i].Row * n + boxCoordinates[i].Column] = true;
            }

            q.Clear();

            int u = row * n + column;
            visited[u] = true;

            while (true)
            {
                int[] neighbors = neighborMap[u];
                int neighborCount = neighbors.Length;
                for (int i = 0; i < neighborCount; i++)
                {
                    int v = neighbors[i];
                    if (!visited[v])
                    {
                        visited[v] = true;
                        q.Enqueue(v);
                    }
                }

                if (q.IsEmpty)
                {
                    break;
                }
                u = q.Dequeue();
            }
        }

        public override bool IsAccessible(int row, int column)
        {
            return visited[row * n + column] && !Level.IsBox(data[row, column]);
        }

        public override int GetDistance(int row, int column)
        {
            return visited[row * n + column] && !Level.IsBox(data[row, column]) ? 0 : DefaultInaccessible;
        }

        public override Coordinate2D GetFirstAccessibleCoordinate()
        {
            for (int u = firstInside; u <= lastInside; u++)
            {
                if (visited[u])
                {
                    int row = u / n;
                    int column = u % n;
                    if (!Level.IsBox(data[row, column]))
                    {
                        return new Coordinate2D(row, column);
                    }
                }
            }
            return Coordinate2D.Undefined;
        }

        public override IEnumerable<Coordinate2D> AccessibleCoordinates
        {
            get
            {
                for (int u = firstInside; u <= lastInside; u++)
                {
                    if (visited[u])
                    {
                        int row = u / n;
                        int column = u % n;
                        if (!Level.IsBox(data[row, column]))
                        {
                            yield return new Coordinate2D(row, column);
                        }
                    }
                }
            }
        }

        public override MoveList GetPath(int row, int column)
        {
            throw new InvalidOperationException("shortest path not supported by this path finder");
        }

        #endregion
    }
}
