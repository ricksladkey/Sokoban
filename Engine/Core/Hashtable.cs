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
    /// A generic table for mapping keys to values
    /// using open addressing with double hashing.
    /// </summary>
    /// <remarks>
    /// This is similar in spirit to the generic class Dictionary
    /// but uses open addressing and contains extra high-performance
    /// method for smart addition.
    /// <see>Knuth, TOACP, Volume 3, Chapter 6.4, Hashing, Algorithm D</see>
    /// </remarks>
    /// <typeparam name="TKey">The type of the table's keys.</typeparam>
    /// <typeparam name="TValue">The type of the table's values.</typeparam>
    public class Hashtable<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private struct Bucket
        {
            public TKey Key;
            public TValue Value;

            // The index records whether the bucket is in use
            // and if it is in use, it also functions as
            // the occupied list.  If the index is zero, the bucket
            // is unused.  We could use a negative value but
            // then all of the bucket indices would have to
            // initialized before use.  A value of one indicates
            // the bucket is in use but the bucket is the tail
            // of the occupied list.  All other positive value
            // indicate the next bucket in the free list is
            // two less than the index.
            private int Index;

            public int Next
            {
                get
                {
                    return Index - 2;
                }
                set
                {
                    Index = value + 2;
                }
            }

            public bool IsOccupied
            {
                get
                {
                    return Index > 0;
                }
                set
                {
                    // Can only be set to false.
                    Index = 0;
                }
            }
        }

        private const int defaultCapacity = 16;
        private const float defaultLoadFactor = 0.75f;

        private bool validate;
        private int capacity;
        private float loadFactor;
        private int maximumCapacity;
        private uint maximumCapacityUnsigned;
        private int occupied;
        private Bucket[] table;
        private int occupiedList;
        private IEqualityComparer<TKey> comparer;

        public Hashtable()
            : this(defaultCapacity)
        {
        }

        public Hashtable(int capacity)
            : this(capacity, defaultLoadFactor)
        {
        }

        public Hashtable(int capacity, float LoadFactor)
            : this(capacity, LoadFactor, EqualityComparer<TKey>.Default)
        {
        }

        public Hashtable(int capacity, IEqualityComparer<TKey> comparer)
            : this(capacity, defaultLoadFactor, comparer)
        {
        }

        public Hashtable(int capacity, float loadFactor, IEqualityComparer<TKey> comparer)
        {
            this.validate = false;

            this.capacity = capacity;
            this.loadFactor = loadFactor;
            this.comparer = comparer;

            CreateTable();
        }

        public int Capacity
        {
            get
            {
                return capacity;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                int index = Lookup(key);
                if (table[index].IsOccupied)
                {
                    return table[index].Value;
                }
                throw new InvalidOperationException("key not found");
            }
            set
            {
                int index = Lookup(key);
                if (table[index].IsOccupied)
                {
                    table[index].Value = value;
                }
                else
                {
                    AddAt(index, key, value);
                }
            }
        }

        public bool Validate
        {
            get
            {
                return validate;
            }
            set
            {
                validate = value;
            }
        }

        public int Count
        {
            get
            {
                return occupied;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return new CollectionFromEnumerable<TKey>(GetKeys());
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return new CollectionFromEnumerable<TValue>(GetValues());
            }
        }

        private IEnumerable<TKey> GetKeys()
        {
            for (int index = occupiedList; index != -1; index = table[index].Next)
            {
                yield return table[index].Key;
            }
        }

        private IEnumerable<TValue> GetValues()
        {
            for (int index = occupiedList; index != -1; index = table[index].Next)
            {
                yield return table[index].Value;
            }
        }

        private void CreateTable()
        {
            maximumCapacity = HashtableUtils.NextPrime((int)Math.Round(capacity / loadFactor));
            maximumCapacityUnsigned = (uint)maximumCapacity;
            table = new Bucket[maximumCapacity];
            occupied = 0;
            occupiedList = -1;
        }

        public void Clear()
        {
            if (occupied == 0)
            {
                return;
            }

            // Clear the table as efficiently as possible.
            if (occupied > capacity / 4)
            {
                // Clear entire table of entries to the default values.
                Array.Clear(table, 0, table.Length);
            }
            else
            {
                // Just clear occupied entries in the table by walking the occupied list.
                while (occupiedList != -1)
                {
                    int next = table[occupiedList].Next;
                    table[occupiedList].IsOccupied = false;
                    occupiedList = next;
                }
            }

            occupied = 0;
            occupiedList = -1;
        }

        public void TrimExcess()
        {
            Resize((int)Math.Round(occupied / loadFactor));
        }

        public void Add(TKey key, TValue value)
        {
            int index = Lookup(key);
            if (!table[index].IsOccupied)
            {
                AddAt(index, key, value);
                return;
            }
            throw new InvalidOperationException("duplicate key");
        }

        public bool Remove(TKey key)
        {
            throw new NotSupportedException("not supported");
        }

        public bool AddIfNotPresent(TKey key, TValue value)
        {
            int index = Lookup(key);
            if (!table[index].IsOccupied)
            {
                AddAt(index, key, value);
                return true;
            }
            return false;
        }

        public bool ContainsKey(TKey key)
        {
            return table[Lookup(key)].IsOccupied;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int index = Lookup(key);
            if (table[index].IsOccupied)
            {
                value = table[index].Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        private int Lookup(TKey key)
        {
            uint hashCode = (uint)comparer.GetHashCode(key);
            int index = (int)(hashCode % maximumCapacityUnsigned);
#if DEBUG
            if (validate)
            {
                if (table[index].IsOccupied)
                {
                    uint tableHash = (uint)comparer.GetHashCode(table[index].Key);
                    int tableIndex = (int)(tableHash % maximumCapacityUnsigned);
                    if (tableIndex != index)
                    {
                        Trace.WriteLine("hash code mismatch!");
                    }
                }
            }
#endif
            if (!table[index].IsOccupied)
            {
                return index;
            }
            if (comparer.Equals(table[index].Key, key))
            {
                return index;
            }
            int c = (int)(1 + hashCode % (maximumCapacityUnsigned - 2));
            while (true)
            {
                index -= c;
                if (index < 0)
                {
                    index += maximumCapacity;
                }
                if (!table[index].IsOccupied)
                {
                    return index;
                }
                if (comparer.Equals(table[index].Key, key))
                {
                    return index;
                }
            }
        }

        private void AddAt(int index, TKey key, TValue value)
        {
            if (++occupied == capacity)
            {
                Resize(capacity * 2);
                index = Lookup(key);
            }
            table[index].Key = key;
            table[index].Value = value;
            table[index].Next = occupiedList;
            occupiedList = index;
        }

        private void Resize(int newCapacity)
        {
            capacity = newCapacity;
            Bucket[] oldTable = table;
            int otherOccupiedList = occupiedList;
            CreateTable();
            AddTable(oldTable, otherOccupiedList);
        }

        private void AddTable(Bucket[] otherTable, int otherOccupiedList)
        {
            for (int index = otherOccupiedList; index != -1; index = otherTable[index].Next)
            {
                Add(otherTable[index].Key, otherTable[index].Value);
            }
        }

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            TValue value;
            if (TryGetValue(item.Key, out value))
            {
                if (EqualityComparer<TValue>.Default.Equals(item.Value, value))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int next = arrayIndex;
            for (int index = occupiedList; index != -1; index = table[index].Next)
            {
                array[next++] = new KeyValuePair<TKey, TValue>(table[index].Key, table[index].Value);
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            TValue value;
            if (TryGetValue(item.Key, out value))
            {
                if (EqualityComparer<TValue>.Default.Equals(item.Value, value))
                {
                    Remove(item.Key);
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int index = occupiedList; index != -1; index = table[index].Next)
            {
                yield return new KeyValuePair<TKey, TValue>(table[index].Key, table[index].Value);
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
