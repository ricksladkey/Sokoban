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
    // Reference http://en.wikipedia.org/wiki/Dijkstra's_algorithm

    //  1  function Dijkstra(Graph, source):
    //  2      for each vertex v in Graph:           // Initializations
    //  3          dist[v] := infinity               // Unknown distance function from source to v
    //  4          previous[v] := undefined          // Previous node in optimal path from source
    //  5      dist[source] := 0                     // Distance from source to source
    //  6      Q := the set of all nodes in Graph
    //         // All nodes in the graph are unoptimized - thus are in Q
    //  7      while Q is not empty:                 // The main loop
    //  8          u := vertex in Q with smallest dist[]
    //  9          if dist[u] = infinity:
    // 10              break                         // all remaining vertices are inaccessible
    // 11          remove u from Q
    // 12          for each neighbor v of u:         // where v has not yet been removed from Q.
    // 13              alt := dist[u] + dist_between(u, v) 
    // 14              if alt < dist[v]:             // Relax (u,v,a)
    // 15                  dist[v] := alt
    // 16                  previous[v] := u
    // 17      return previous[]

    public class DijkstraPathFinder : PathFinder
    {
        private class Vertex
        {
            public int Row;
            public int Column;
            public int Distance;
            public bool Visited;
            public Vertex[] Neighbors;

            public Coordinate2D Coordinate
            {
                get
                {
                    return new Coordinate2D(Row, Column);
                }
            }

            public Vertex(int row, int column)
            {
                Row = row;
                Column = column;
            }
        }

        private Array2D<Cell> data;
        private int rowLimit;
        private Array2D<Vertex> vertexMap;
        private FixedQueue<Vertex> q;
        private int[][] insideCoordinates;

        public DijkstraPathFinder(Level level)
            : base(level)
        {
            data = level.Data;
            rowLimit = level.Height - 1;
            insideCoordinates = level.InsideCoordinates;
            int m = level.Height * level.Width;
            q = new FixedQueue<Vertex>(m);

            // Initialize the vertex map.
            vertexMap = new Array2D<Vertex>(level.Height, level.Width);
            foreach (Coordinate2D coord in level.Coordinates)
            {
                Vertex vertex = new Vertex(coord.Row, coord.Column);
                vertex.Distance = DefaultInaccessible;
                vertex.Visited = true;
                vertexMap[coord.Row, coord.Column] = vertex;
            }

            // Calculate the neighbors of each coordinate.
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                Vertex vertex = vertexMap[coord];
                List<Vertex> neighbors = new List<Vertex>();
                foreach (Coordinate2D neighbor in coord.FourNeighbors)
                {
                    if (level.IsFloor(neighbor))
                    {
                        neighbors.Add(vertexMap[neighbor]);
                    }
                }
                vertex.Neighbors = neighbors.ToArray();
            }
        }

        #region PathFinder Members

        public override void Find(int srcRow, int srcCol)
        {
            for (int row = 1; row < rowLimit; row++)
            {
                Cell[] dataRow = data[row];
                Vertex[] infoRow = vertexMap[row];
                int[] columns = insideCoordinates[row];
                int n = columns.Length;
                for (int i = 0; i < n; i++)
                {
                    int column = columns[i];
                    Vertex vertex = infoRow[column];
                    vertex.Distance = DefaultInaccessible;
                    vertex.Visited = Level.IsBox(dataRow[column]);
                }
            }

            q.Clear();
            Vertex u = vertexMap[srcRow, srcCol];
            u.Distance = 0;

            while (true)
            {
                u.Visited = true;

                Vertex[] neighbors = u.Neighbors;
                int n = neighbors.Length;
                int alt = u.Distance + 1;
                for (int i = 0; i < n; i++)
                {
                    Vertex v = neighbors[i];
                    if (v.Visited)
                    {
                        continue;
                    }

                    if (alt < v.Distance)
                    {
                        q.Enqueue(v);
                        v.Distance = alt;
                    }
                }

                if (q.IsEmpty)
                {
                    break;
                }

                u = q.Dequeue();
            }
        }

        public override int GetDistance(int row, int column)
        {
            return vertexMap[row, column].Distance;
        }

        public override Coordinate2D GetFirstAccessibleCoordinate()
        {
            // Find the first accessible coordinate by row.
            for (int row = 1; row < rowLimit; row++)
            {
                int[] columns = insideCoordinates[row];
                int n = columns.Length;
                for (int i = 0; i < n; i++)
                {
                    int column = columns[i];
                    if (vertexMap[row, column].Distance < DefaultInaccessible)
                    {
                        return new Coordinate2D(row, column);
                    }
                }
            }
            return Coordinate2D.Undefined;
        }

        #endregion
    }
}
