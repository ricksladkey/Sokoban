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

#define USE_TWO_LAYER_HASHTABLE
#undef USE_STRONG_HASHTABLE
#undef USE_DICTIONARY
#undef USE_WEAK_HASHTABLE
#undef USE_SORTED_DICTIONARY

using System;
using System.Collections.Generic;
using System.Text;

using Sokoban.Engine.Core;
using Sokoban.Engine.Levels;

namespace Sokoban.Engine.Collections
{
    /// <summary>
    /// A table to record previously visited level positions.
    /// </summary>
    /// <remarks>
    /// This class represents the never-ending search for the
    /// perfect data-structure.  In the end, a very specialized
    /// class was hand-written to achieve the best compromise
    /// between performance and features.
    /// </remarks>

#if USE_TWO_LAYER_HASHTABLE

    public class PositionLookupTable<TValue> : TwoLayerHashtable<HashKey, TValue>, IPositionLookupTable<TValue>
    {
        public PositionLookupTable()
        {
        }

        public PositionLookupTable(int capacity)
            : base(16, capacity)
        {
        }
    }

#endif

#if USE_STRONG_HASHTABLE

    public class PositionLookupTable<TValue> : Hashtable<HashKey, TValue>, IPositionLookupTable<TValue>
    {
        public PositionLookupTable()
        {
        }

        public PositionLookupTable(int capacity)
            : base(capacity)
        {
        }
    }

#endif

#if USE_DICTIONARY

    public class PositionLookupTable<TValue> : Dictionary<HashKey, TValue>, IPositionLookupTable<TValue>
    {
        public PositionLookupTable()
        {
        }

        public PositionLookupTable(int capacity)
            : base(capacity)
        {
        }

        private bool validate;
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

        public bool ContainsKey(HashKey key, ref TValue value)
        {
            if (ContainsKey(key))
            {
                value = this[key];
                return true;
            }
            return false;
        }
    }

#endif

#if USE_WEAK_HASHTABLE

    public class PositionLookupTable<TValue> : System.Collections.Hashtable, IPositionLookupTable<TValue>
    {
        public PositionLookupTable()
        {
        }

        public PositionLookupTable(int capacity)
        {
        }

        private bool validate;
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

        public void Add(HashKey key, TValue value)
        {
            base.Add(key, value);
        }

        public bool Remove(HashKey key)
        {
            if (base.Contains(key))
            {
                base.Remove(key);
                return true;
            }
            return false;
        }

        public bool ContainsKey(HashKey key)
        {
            return base.ContainsKey(key);
        }

        public bool TryGetValue(HashKey key, out TValue value)
        {
            if (base.Contains(key))
            {
                value = (TValue)base[key];
                return true;
            }
            value = default(TValue);
            return false;
        }

        public TValue this[HashKey key]
        {
            get
            {
                return (TValue)base[key];
            }
            set
            {
                base[key] = value;
            }
        }

        #region IDictionary<HashKey,TValue> Members

        public new ICollection<HashKey> Keys
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public new ICollection<TValue> Values
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion

        #region ICollection<KeyValuePair<HashKey,TValue>> Members

        public void Add(KeyValuePair<HashKey, TValue> item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Contains(KeyValuePair<HashKey, TValue> item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void CopyTo(KeyValuePair<HashKey, TValue>[] array, int arrayIndex)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Remove(KeyValuePair<HashKey, TValue> item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IEnumerable<KeyValuePair<HashKey,TValue>> Members

        public new IEnumerator<KeyValuePair<HashKey, TValue>> GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }

#endif

#if USE_SORTED_DICTIONARY

    public class PositionLookupTable<TValue> : SortedDictionary<HashKey, TValue>, IPositionLookupTable<TValue>
    {
        public PositionLookupTable()
        {
        }

        public PositionLookupTable(int capacity)
        {
        }

        private bool validate;
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
    }

#endif

}
