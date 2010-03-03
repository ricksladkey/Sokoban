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
using System.Runtime.InteropServices;

namespace Sokoban.Engine.Core
{
    /// <summary>
    /// CollectionFromEnumerable is a utility class that can be
    /// used to support the Keys and Values properties of the
    /// IDictionary interface which are defined to return an
    /// ICollection instead of an IEnumerable.  This class
    /// implements the ICollection interface without actually
    /// creating a temporary collection.
    /// </summary>
    /// <typeparam name="T">The type the collection is of</typeparam>
    public class CollectionFromEnumerable<T> : ICollection<T>
    {
        private IEnumerable<T> enumerable;

        public CollectionFromEnumerable(IEnumerable<T> enumerable)
        {
            this.enumerable = enumerable;
        }

        #region ICollection<T> Members

        public void Add(T item)
        {
            throw new NotSupportedException("read-only collection");
        }

        public void Clear()
        {
            throw new NotSupportedException("read-only collection");
        }

        public bool Contains(T item)
        {
            foreach (T value in enumerable)
            {
                if (EqualityComparer<T>.Default.Equals(item, value))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int next = arrayIndex;
            foreach (T value in enumerable)
            {
                array[next++] = value;
            }
        }

        public int Count
        {
            get
            {
                int count = 0;
                foreach (T value in enumerable)
                {
                    count++;
                }
                return count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException("read-only collection");
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return enumerable.GetEnumerator();
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
