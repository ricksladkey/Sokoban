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
    public class Set<T> : ISet<T>
    {
        private struct EmptyValue
        {
        }

        private const int defaultCapacity = 16;

        private IEqualityComparer<T> comparer;
        private Hashtable<T, EmptyValue> hashtable;

        public Set()
            : this(defaultCapacity)
        {
        }

        public Set(IEnumerable<T> collection)
            : this(defaultCapacity, collection)
        {
        }

        public Set(int capacity, IEnumerable<T> collection)
            : this(capacity, EqualityComparer<T>.Default, collection)
        {
        }

        public Set(int capacity)
            : this(capacity, EqualityComparer<T>.Default)
        {
        }

        public Set(int capacity, IEqualityComparer<T> comparer)
        {
            this.comparer = comparer;
            this.hashtable = new Hashtable<T, EmptyValue>(capacity, comparer);
        }

        public Set(int capacity, IEqualityComparer<T> comparer, IEnumerable<T> collection)
            : this(capacity, comparer)
        {
            foreach (T item in collection)
            {
                Add(item);
            }
        }

        #region ISet<T> Members

        public bool Add(T item)
        {
            return hashtable.AddIfNotPresent(item, default(EmptyValue));
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                Remove(item);
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return IsSubsetOf(other) && !IsSupersetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return IsSupersetOf(other) && !IsSubsetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            // Verify that all items contained in this set
            // are also contained in the other set.
            ICollection<T> otherCollection = other as ICollection<T>;
            if (otherCollection != null)
            {
                foreach (T item in this)
                {
                    if (!otherCollection.Contains(item))
                    {
                        return false;
                    }
                }
                return true;
            }
            foreach (T item in this)
            {
                if (!EnumerableContains(other, item))
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            // Verify that every element of the other set
            // is contained in this set.
            foreach (T item in other)
            {
                if (!Contains(item))
                {
                    return false;
                }
            }
            return true;
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            // Verify that this set and the other set have at least
            // one item in common.
            ICollection<T> otherCollection = other as ICollection<T>;
            if (otherCollection != null && otherCollection.Count > Count)
            {
                foreach (T item in this)
                {
                    if (otherCollection.Contains(item))
                    {
                        return true;
                    }
                }
                return false;
            }
            foreach (T item in other)
            {
                if (Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        public int RemoveWhere(Predicate<T> match)
        {
            List<T> list = new List<T>();
            foreach (T item in this)
            {
                if (match.Invoke(item))
                {
                    list.Add(item);
                }
            }
            int count = 0;
            foreach (T item in list)
            {
                Remove(item);
                count++;
            }
            return count;
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return IsSubsetOf(other) && IsSupersetOf(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                if (!Remove(item))
                {
                    Add(item);
                }
            }
        }

        public void TrimExcess()
        {
            this.hashtable.TrimExcess();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                Add(item);
            }
        }

        #endregion

        private bool EnumerableContains(IEnumerable<T> other, T item)
        {
            foreach (T otherItem in other)
            {
                if (comparer.Equals(item, otherItem))
                {
                    return true;
                }
            }
            return false;
        }

        #region ICollection<T> Members

        public int Count
        {
            get
            {
                return hashtable.Count;
            }
        }

        void ICollection<T>.Add(T item)
        {
            hashtable.Add(item, default(EmptyValue));
        }

        public bool Contains(T item)
        {
            return hashtable.ContainsKey(item);
        }

        public void Clear()
        {
            hashtable.Clear();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int next = arrayIndex;
            foreach (T value in hashtable.Keys)
            {
                array[next++] = value;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(T item)
        {
            return hashtable.Remove(item);
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return hashtable.Keys.GetEnumerator();
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
