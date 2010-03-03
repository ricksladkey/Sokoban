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

using Sokoban.Engine.Collections;
using Sokoban.Engine.Core;
using Sokoban.Engine.Deadlocks;
using Sokoban.Engine.Levels;
using Sokoban.Engine.Paths;
using Sokoban.Engine.Utilities;

namespace Sokoban.Engine.Solvers.Value
{
#if !USE_REFERENCE_TYPE_FOR_NODE
    /// <summary>
    /// A table that maps hashcodes to nodes.
    /// </summary>
    /// <remarks>
    /// A node is already very lightweight but we only need to
    /// store a node's index.  This wrapper class makes it
    /// appear as though we are mapping hashcodes to nodes
    /// when we are really mapping hashcodes to integers.
    /// </remarks>
    public class TranspositionTable : IPositionLookupTable<Node>
    {
        private const int defaultCapacity = 0x1000;

        private NodeCollection nodes;
        private IPositionLookupTable<int> table;

        public TranspositionTable(NodeCollection nodes)
            : this(nodes, defaultCapacity)
        {
        }

        public TranspositionTable(NodeCollection nodes, int capacity)
        {
            this.nodes = nodes;
            this.table = new PositionLookupTable<int>(capacity);
        }

        public bool Validate
        {
            get
            {
                return table.Validate;
            }
            set
            {
                table.Validate = value;
            }
        }

        public Node this[HashKey key]
        {
            get
            {
                return new Node(nodes, table[key]);
            }
            set
            {
                table[key] = value.ID;
            }
        }

        public bool ContainsKey(HashKey key)
        {
            return table.ContainsKey(key);
        }

        public bool TryGetValue(HashKey key, out Node value)
        {
            int index = -1;
            if (table.TryGetValue(key, out index))
            {
                value = new Node(nodes, index);
                return true;
            }
            value = Node.Empty;
            return false;
        }

        public void Add(HashKey key, Node value)
        {
            table.Add(key, value.ID);
        }

        public bool Remove(HashKey key)
        {
            return table.Remove(key);
        }

        public ICollection<HashKey> Keys
        {
            get
            {
                return new CollectionFromEnumerable<HashKey>(table.Keys);
            }
        }

        public ICollection<Node> Values
        {
            get
            {
                return new CollectionFromEnumerable<Node>(GetValues());
            }
        }

        private IEnumerable<Node> GetValues()
        {
            foreach (int id in table.Values)
            {
                yield return new Node(nodes, id);
            }
        }

        #region ICollection<KeyValuePair<HashKey,Node>> Members

        public void Add(KeyValuePair<HashKey, Node> item)
        {
            table.Add(item.Key, item.Value.ID);
        }

        public void Clear()
        {
            table.Clear();
        }

        public bool Contains(KeyValuePair<HashKey, Node> item)
        {
            int value;
            if (table.TryGetValue(item.Key, out value))
            {
                if (item.Value.ID == value)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(KeyValuePair<HashKey, Node>[] array, int arrayIndex)
        {
            int next = arrayIndex;
            foreach (KeyValuePair<HashKey, int> pair in table)
            {
                array[next++] = new KeyValuePair<HashKey, Node>(pair.Key, new Node(nodes, pair.Value));
            }
        }

        public int Count
        {
            get
            {
                return table.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(KeyValuePair<HashKey, Node> item)
        {
            int value;
            if (table.TryGetValue(item.Key, out value))
            {
                if (item.Value.ID == value)
                {
                    Remove(item.Key);
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region IEnumerable<KeyValuePair<HashKey,Node>> Members

        public IEnumerator<KeyValuePair<HashKey, Node>> GetEnumerator()
        {
            foreach (KeyValuePair<HashKey, int> pair in table)
            {
                yield return new KeyValuePair<HashKey, Node>(pair.Key, new Node(nodes, pair.Value));
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
#endif
}
