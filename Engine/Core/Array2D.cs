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
    public class Array2D<T> : IEquatable<Array2D<T>>, IEnumerable<T>
    {
        private static void Copy(T[][] dst, int dstRow, int dstCol, T[][] src, int srcRow, int srcCol, int height, int width)
        {
            for (int row = 0; row < height; row++)
            {
                T[] srcArray = src[srcRow + row];
                T[] dstArray = dst[dstRow + row];
                for (int column = 0; column < width; column++)
                {
                    dstArray[dstCol + column] = srcArray[srcCol + column];
                }
            }
        }

        public static void Copy(Array2D<T> dst, int dstRow, int dstCol, Array2D<T> src, int srcRow, int srcCol, int height, int width)
        {
            Copy(dst.data, dstRow, dstCol, src.data, srcRow, srcCol, height, width);
        }

        public static void Copy(Array2D<T> dst, int dstRow, int dstCol, T[][] src, int srcRow, int srcCol, int height, int width)
        {
            Copy(dst.data, dstRow, dstCol, src, srcRow, srcCol, height, width);
        }

        public static void Copy(T[][] dst, int dstRow, int dstCol, Array2D<T> src, int srcRow, int srcCol, int height, int width)
        {
            Copy(dst, dstRow, dstCol, src.data, srcRow, srcCol, height, width);
        }

        public static void Copy(Array2D<T> dst, int dstRow, int dstCol, T[,] src, int srcRow, int srcCol, int height, int width)
        {
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    dst[dstRow + row, dstCol + column] = src[srcRow + row, srcCol + column];
                }
            }
        }

        public static void Copy(T[,] dst, int dstRow, int dstCol, Array2D<T> src, int srcRow, int srcCol, int height, int width)
        {
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    dst[dstRow + row, dstCol + column] = src[srcRow + row, srcCol + column];
                }
            }
        }

        int height;
        int width;
        private T[][] data;
        private IEqualityComparer<T> comparer;

        private void Allocate(int height, int width)
        {
            this.height = height;
            this.width = width;
            this.data = new T[height][];
            for (int row = 0; row < height; row++)
            {
                data[row] = new T[width];
            }
            this.comparer = EqualityComparer<T>.Default;
        }

        public int Height
        {
            get
            {
                return height;
            }
        }

        public int Width
        {
            get
            {
                return width;
            }
        }

        public Array2D(int height, int width)
        {
            Allocate(height, width);
        }

        public Array2D(Array2D<T> other)
        {
            Allocate(other.height, other.width);
            Copy(this, 0, 0, other, 0, 0, other.height, other.width);
        }

        public Array2D(T[,] other)
        {
            Allocate(other.GetLength(0), other.GetLength(1));
            Copy(this, 0, 0, other, 0, 0, other.GetLength(0), other.GetLength(1));
        }

        public Array2D(T[][] other)
        {
            Allocate(other.Length, other[0].Length);
            Copy(this, 0, 0, other, 0, 0, other.Length, other[0].Length);
        }

        public bool IsValid(int row, int column)
        {
            return row >= 0 && row < height && column >= 0 && column < width;
        }

        public bool IsValid(Coordinate2D coord)
        {
            return IsValid(coord.Row, coord.Column);
        }

        public T[] this[int row]
        {
            get
            {
                return data[row];
            }
        }

        public T this[int row, int column]
        {
            get
            {
                return data[row][column];
            }
            set
            {
                data[row][column] = value;
            }
        }

        public T this[Coordinate2D coord]
        {
            get
            {
                return data[coord.Row][coord.Column];
            }
            set
            {
                data[coord.Row][coord.Column] = value;
            }
        }

        public void CopyTo(Array2D<T> dst, int dstRow, int dstCol, int srcRow, int srcCol, int height, int width)
        {
            Copy(dst, dstRow, dstCol, this, srcRow, srcCol, height, width);
        }

        public void CopyTo(Array2D<T> dst)
        {
            Copy(dst, 0, 0, this, 0, 0, height, width);
        }

        public void CopyTo(T[,] dst, int dstRow, int dstCol, int srcRow, int srcCol, int height, int width)
        {
            Copy(dst, dstRow, dstCol, this, srcRow, srcCol, height, width);
        }

        public Array2D<T> GetSubarray(int row, int column, int height, int width)
        {
            Array2D<T> subarray = new Array2D<T>(height, width);
            Copy(subarray, 0, 0, this, row, column, height, width);
            return subarray;
        }

        public T[,] ConvertToSystemArray()
        {
            T[,] dst = new T[height, width];
            Copy(dst, 0, 0, this, 0, 0, height, width);
            return dst;
        }

        public void SetAll(T value)
        {
            for (int i = 0; i < height; i++)
            {
                T[] row = data[i];
                for (int j = 0; j < width; j++)
                {
                    row[j] = value;
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < height; i++)
            {
                Array.Clear(data[i], 0, width);
            }
        }

        public void Replace(T oldValue, T newValue)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < height; i++)
            {
                T[] row = data[i];
                for (int j = 0; j < width; j++)
                {
                    if (row[j].Equals(oldValue))
                    {
                        row[j] = newValue;
                    }
                }
            }
        }

        public void FloodFill(int row, int column, T target, T replacement)
        {
            T value = data[row][column];
            if (!comparer.Equals(value, target))
            {
                return;
            }
            if (comparer.Equals(value, replacement))
            {
                return;
            }
            data[row][column] = replacement;
            if (row > 0)
            {
                FloodFill(row - 1, column, target, replacement);
            }
            if (row < height - 1)
            {
                FloodFill(row + 1, column, target, replacement);
            }
            if (column > 0)
            {
                FloodFill(row, column - 1, target, replacement);
            }
            if (column < width - 1)
            {
                FloodFill(row, column + 1, target, replacement);
            }
        }

        public void FloodFill(Coordinate2D coord, T target, T replacement)
        {
            FloodFill(coord.Row, coord.Column, target, replacement);
        }

        public Array2D<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }
            Array2D<TOutput> result = new Array2D<TOutput>(height, width);
            for (int i = 0; i < height; i++)
            {
                T[] oldRow = data[i];
                TOutput[] newRow = result.data[i];
                for (int j = 0; j < width; j++)
                {
                    newRow[j] = converter(oldRow[j]);
                }
            }
            return result;
        }

        public TOutput[] ConvertAllRows<TOutput>(Converter<T[], TOutput> converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }
            TOutput[] result = new TOutput[height];
            for (int i = 0; i < height; i++)
            {
                result[i] = converter(data[i]);
            }
            return result;
        }

        public IEnumerable<T[]> Rows
        {
            get
            {
                for (int row = 0; row < height; row++)
                {
                    yield return data[row];
                }
            }
        }

        public IEnumerable<Coordinate2D> Coordinates
        {
            get
            {
                for (int row = 0; row < height; row++)
                {
                    for (int column = 0; column < width; column++)
                    {
                        yield return new Coordinate2D(row, column);
                    }
                }
            }
        }

        public IEnumerable<Coordinate2D> PerimeterCoordinates
        {
            get
            {
                // Handle pathological cases.
                if (height < 2 || width < 2)
                {
                    for (int row = 0; row < height; row++)
                    {
                        for (int column = 0; column < width; column++)
                        {
                            yield return new Coordinate2D(row, column);
                        }
                    }
                    yield break;
                }

                // The two horizontal rows.
                for (int row = 0; row < height; row += height - 1)
                {
                    for (int column = 0; column < width; column++)
                    {
                        yield return new Coordinate2D(row, column);
                    }
                }

                // The two vertical columns less the corners.
                for (int row = 1; row < height - 1; row++)
                {
                    for (int column = 0; column < width; column += width - 1)
                    {
                        yield return new Coordinate2D(row, column);
                    }
                }
            }
        }

        public IEnumerable<Coordinate2D> NonPerimeterCoordinates
        {
            get
            {
                int rowLimit = height - 1;
                int columnLimit = width - 1;
                for (int row = 1; row < rowLimit; row++)
                {
                    for (int column = 1; column < columnLimit; column++)
                    {
                        yield return new Coordinate2D(row, column);
                    }
                }
            }
        }

        public override string ToString()
        {
            return String.Format("{{Array2D: Height = {0}, Width = {1}}}", height, width); 
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    yield return data[row][column];
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

        #region IEquatable<Array2D<T>> Members

        public bool Equals(Array2D<T> other)
        {
            if (height != other.height || width != other.width)
            {
                return false;
            }
            for (int row = 0; row < height; row++)
            {
                T[] rowData = data[row];
                T[] otherRowData = other.data[row];
                for (int column = 0; column < width; column++)
                {
                    if (!comparer.Equals(rowData[column], otherRowData[column]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        #endregion
    }
}
