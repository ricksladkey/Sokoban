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

using BitStringType = System.UInt64;

namespace Sokoban.Engine.Paths
{
    public class SmallLevelAnyPathFinder : ArrayPathFinder
    {
        private BitStringType[] visitedBit;
        private BitStringType visited;

        public const int MaximumInsideSquares = 64;

        public SmallLevelAnyPathFinder(Level level)
            : base(level)
        {
            if (level.InsideSquares > MaximumInsideSquares)
            {
                throw new InvalidOperationException("level contains too many inside squares for this path finder");
            }

            visitedBit = new BitStringType[m];
            int index = 0;
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                int u = coord.Row * n + coord.Column;
                BitStringType bit = (BitStringType)1u << index;
                visitedBit[u] = bit;
                index++;
            }
        }

        #region PathFinder Members

        public override void Find(int row, int column)
        {
            BitStringType newVisited = 0u;
            for (int i = 0; i < boxCount; i++)
            {
                newVisited |= visitedBit[boxCoordinates[i].Row * n + boxCoordinates[i].Column];
            }
            BitStringType boxBits = newVisited;

            q.Clear();
            int u = row * n + column;
            newVisited |= visitedBit[u];

            while (true)
            {
                int[] neighbors = neighborMap[u];
                int neighborCount = neighbors.Length;
                for (int i = 0; i < neighborCount; i++)
                {
                    int v = neighbors[i];
                    BitStringType bit = visitedBit[v];
                    if ((newVisited & bit) == 0)
                    {
                        newVisited |= bit;
                        q.Enqueue(v);
                    }
                }

                if (q.IsEmpty)
                {
                    break;
                }
                u = q.Dequeue();
            }

            visited = newVisited & ~boxBits;
        }

        public override bool IsAccessible(int row, int column)
        {
            return (visited & visitedBit[row * n + column]) != 0;
        }

        public override int GetDistance(int row, int column)
        {
            return (visited & visitedBit[row * n + column]) != 0 ? 0 : DefaultInaccessible;
        }

        public override Coordinate2D GetFirstAccessibleCoordinate()
        {
            for (int u = firstInside; u < lastInside; u++)
            {
                if ((visited & visitedBit[u]) != 0)
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
                    if ((visited & visitedBit[u]) != 0)
                    {
                        yield return new Coordinate2D(u / n, u % n);
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
