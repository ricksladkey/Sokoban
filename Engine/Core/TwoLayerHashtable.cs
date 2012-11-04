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
    /// using two layer hashing.
    /// </summary>
    /// <remarks>
    /// This class hashes keys into a table of hashtables.  The
    /// individual hashtables can be resized independently which
    /// greatly reduces the memory demand at the critical moment
    /// when a very large hashtable needs to increase its size.
    /// </remarks>
    /// <typeparam name="TKey">The type of the table's keys.</typeparam>
    /// <typeparam name="TValue">The type of the table's values.</typeparam>
    public class TwoLayerHashtable<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private const int defaultTableSize = 16;
        private const int defaultCapacity = 256;
        private const float defaultLoadFactor = 0.75f;

        private bool validate;
        private int tableSize;
        private int capacity;
        private float loadFactor;
        private Hashtable<TKey, TValue>[] table;
        private IEqualityComparer<TKey> comparer;

        public TwoLayerHashtable()
            : this(defaultTableSize)
        {
        }

        public TwoLayerHashtable(int tableSize)
            : this(tableSize, defaultCapacity)
        {
        }

        public TwoLayerHashtable(int tableSize, int capacity)
            : this(tableSize, capacity, defaultLoadFactor)
        {
        }

        public TwoLayerHashtable(int tableSize, int capacity, float LoadFactor)
            : this(tableSize, capacity, LoadFactor, EqualityComparer<TKey>.Default)
        {
        }

        public TwoLayerHashtable(int tableSize, int capacity, IEqualityComparer<TKey> comparer)
            : this(tableSize, capacity, defaultLoadFactor, comparer)
        {
        }

        public TwoLayerHashtable(int tableSize, int capacity, float loadFactor, IEqualityComparer<TKey> comparer)
        {
            this.validate = false;

            this.tableSize = HashtableUtils.NextPrime(defaultTableSize);
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
                return table[Lookup(key)][key];
            }
            set
            {
                table[Lookup(key)][key] = value;
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
                foreach (Hashtable<TKey, TValue> hashtable in table)
                {
                    hashtable.Validate = validate;
                }
            }
        }

        public int Count
        {
            get
            {
                int count = 0;
                foreach (Hashtable<TKey, TValue> hashtable in table)
                {
                    count += hashtable.Count;
                }
                return count;
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
            foreach (Hashtable<TKey, TValue> hashtable in table)
            {
                foreach (TKey key in hashtable.Keys)
                {
                    yield return key;
                }
            }
        }

        private IEnumerable<TValue> GetValues()
        {
            foreach (Hashtable<TKey, TValue> hashtable in table)
            {
                foreach (TValue value in hashtable.Values)
                {
                    yield return value;
                }
            }
        }

        private void CreateTable()
        {
            table = new Hashtable<TKey, TValue>[tableSize];
            for (int i = 0; i < tableSize; i++)
            {
                table[i] = new Hashtable<TKey, TValue>(capacity / tableSize + 1, loadFactor, comparer);
            }
        }

        public void Clear()
        {
            foreach (Hashtable<TKey, TValue> hashtable in table)
            {
                hashtable.Clear();
            }
        }

        public void Add(TKey key, TValue value)
        {
            table[Lookup(key)].Add(key, value);
        }

        public bool Remove(TKey key)
        {
            return table[Lookup(key)].Remove(key);
        }

        public bool AddIfNotPresent(TKey key, TValue value)
        {
            return table[Lookup(key)].AddIfNotPresent(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return table[Lookup(key)].ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return table[Lookup(key)].TryGetValue(key, out value);
        }

        private int Lookup(TKey key)
        {
            return (int)((uint)comparer.GetHashCode(key) % (uint)tableSize);
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
            foreach (Hashtable<TKey, TValue> hashtable in table)
            {
                foreach (KeyValuePair<TKey, TValue> pair in hashtable)
                {
                    array[next++] = pair;
                }
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
            foreach (Hashtable<TKey, TValue> hashtable in table)
            {
                foreach (KeyValuePair<TKey, TValue> pair in hashtable)
                {
                    yield return pair;
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
