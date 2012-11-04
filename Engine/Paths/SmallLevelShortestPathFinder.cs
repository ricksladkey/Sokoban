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
using Sokoban.Engine.Levels;

using DistanceType = System.Byte;

namespace Sokoban.Engine.Paths
{
    /// <summary>
    /// A shortest path finder is a path finder that calculates
    /// the distance from the specified coordinate to all accessible
    /// coordinates.  Because it is fast to initialize the distances
    /// to zero, we use both zero and the maximum value to mean
    /// inaccessible and store the distances starting with one.  Then
    /// we subtract one and depend on unsigned arithmetic to underflow
    /// to the maximum value of the distance type.
    /// </summary>
    public class SmallLevelShortestPathFinder : ArrayPathFinder
    {
        private DistanceType[] distance;

        public const int MaximumInsideSquares = DistanceType.MaxValue - 1;

        public SmallLevelShortestPathFinder(Level level)
            : base(level)
        {
            // The longest possible distance for a level is a serpentine
            // path connecting all the inside squares.
            if (level.InsideSquares > MaximumInsideSquares)
            {
                throw new InvalidOperationException("level contains too many inside squares for this path finder");
            }

            DerivedInaccessible = DistanceType.MaxValue - 1;
            distance = new DistanceType[m];
        }

        #region PathFinder Members

        public override void Find(int row, int column)
        {
            Array.Clear(distance, firstInside, insideCount);
            for (int i = 0; i < boxCount; i++)
            {
                distance[boxCoordinates[i].Row * n + boxCoordinates[i].Column] = DistanceType.MaxValue;
            }

            q.Clear();
            int u = row * n + column;
            distance[u] = 1;

#if DEBUG
            if (neighborMap[u] == null)
            {
                throw new InvalidOperationException("find requested for wall square");
            }
#endif

            while (true)
            {
                int[] neighbors = neighborMap[u];
                int neighborCount = neighbors.Length;
                DistanceType alt = (DistanceType)(distance[u] + 1);
                for (int i = 0; i < neighborCount; i++)
                {
                    int v = neighbors[i];
                    if (distance[v] == 0)
                    {
                        distance[v] = alt;
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
            return (DistanceType)(distance[row * n + column] - 1) < DistanceType.MaxValue - 1;
        }

        public override int GetDistance(int row, int column)
        {
            return (DistanceType)(distance[row * n + column] - 1);
        }

        public override Coordinate2D GetFirstAccessibleCoordinate()
        {
            for (int u = firstInside; u <= lastInside; u++)
            {
                if ((DistanceType)(distance[u] - 1) < DistanceType.MaxValue - 1)
                {
                    return new Coordinate2D(u / n, u % n);
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
                    if ((DistanceType)(distance[u] - 1) < DistanceType.MaxValue - 1)
                    {
                        yield return new Coordinate2D(u / n, u % n);
                    }
                }
            }
        }

        #endregion
    }
}
