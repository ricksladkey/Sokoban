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

namespace Sokoban.Engine.Core
{
    /// <summary>
    /// CoordinatesByRow is an efficient data structure for storing a subset of array coordindates.
    /// </summary>
    public class CoordinatesByRow : IEnumerable<Coordinate2D>
    {
        private int[][] coordinates;

        public static implicit operator int[][](CoordinatesByRow coordinates)
        {
            return coordinates.coordinates;
        }

        public CoordinatesByRow(int height, IEnumerable<Coordinate2D> enumerator)
        {
            coordinates = new int[height][];
            int lastRow = 0;
            List<int> columns = new List<int>();
            foreach (Coordinate2D coord in enumerator)
            {
                while (lastRow != coord.Row)
                {
                    coordinates[lastRow] = columns.ToArray();
                    columns.Clear();
                    lastRow++;
                }
                columns.Add(coord.Column);
            }
            while (lastRow != height)
            {
                coordinates[lastRow] = columns.ToArray();
                columns.Clear();
                lastRow++;
            }
        }

        #region IEnumerable<Coordinate2D> Members

        public IEnumerator<Coordinate2D> GetEnumerator()
        {
            int height = coordinates.Length;
            for (int row = 0; row < height; row++)
            {
                int[] columns = coordinates[row];
                int n = columns.Length;
                for (int i = 0; i < n; i++)
                {
                    yield return new Coordinate2D(row, columns[i]);
                }
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
