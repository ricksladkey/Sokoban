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

#define USE_SEGMENTED_NODE_ARRAY

using System;
using System.Collections;
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
    public class NodeCollection : IEnumerable<Node>
    {
#if USE_SEGMENTED_NODE_ARRAY
        private SegmentedArray<Node.NodeData> array;
#else
        private Node.NodeData[] array;
#endif
        private int capacity;
        private int freeList;
        private int freeCount;
        private int allocatedCount;
        private int visitedCount;
        private bool validate;

        public NodeCollection(int capacity, bool validate)
        {
            this.capacity = capacity;
            this.validate = validate;
#if USE_SEGMENTED_NODE_ARRAY
            this.array = new SegmentedArray<Node.NodeData>(1000000);
#else
            this.array = new Node.NodeData[capacity];
#endif
            this.freeList = -1;
        }

        public void Clear()
        {
            freeCount = 0;
            allocatedCount = 0;
            visitedCount = 0;
            freeList = -1;
#if USE_SEGMENTED_NODE_ARRAY
            array.Clear();
#endif
        }

        public Node.NodeData[] GetArray(int id)
        {
#if USE_SEGMENTED_NODE_ARRAY
            return array.GetSegment(id);
#else
            return array;
#endif
        }

        public int GetIndex(int id)
        {
#if USE_SEGMENTED_NODE_ARRAY
            return array.GetOffset(id);
#else
            return id;
#endif
        }

        public int Capcity
        {
            get
            {
                return capacity;
            }
        }

        public int Allocated
        {
            get
            {
                return allocatedCount;
            }
        }

        public int Count
        {
            get
            {
                return allocatedCount - freeCount;
            }
        }

        public int Free
        {
            get
            {
                return freeCount;
            }
        }

        public int Visited
        {
            get
            {
                return visitedCount;
            }
        }

        public int Allocate()
        {
            visitedCount++;
            if (freeList != -1)
            {
                int id = freeList;
                freeList = GetArray(id)[GetIndex(id)].child;
                freeCount--;
                return id;
            }
#if USE_SEGMENTED_NODE_ARRAY
            allocatedCount++;
            return array.Allocate();
#else
            if (allocatedCount == capacity)
            {
                throw new SolverException("Search nodes exhausted");
            }
            return allocatedCount++;
#endif
        }

        public void Release(Node node)
        {
#if DEBUG
            node.Free = true;
#endif
            int id = node.ID;
            GetArray(id)[GetIndex(id)].child = freeList;
            freeList = id;
            freeCount++;
        }

        public void ClearFlags()
        {
            for (int id = 0; id < allocatedCount; id++)
            {
                Node node = new Node(this, id);
                node.Free = false;
                node.InTree = false;
            }
        }

        public void MarkFree()
        {
            for (Node node = new Node(this, freeList); !Node.IsEmpty(node); node = node.Child)
            {
                node.Free = true;
            }
        }

        public IEnumerable<Node> AllocatedNodes
        {
            get
            {
                for (int id = 0; id < allocatedCount; id++)
                {
                    yield return new Node(this, id);
                }
            }
        }

        #region IEnumerable<Node> Members

        public IEnumerator<Node> GetEnumerator()
        {
            MarkFree();
            for (int id = 0; id < allocatedCount; id++)
            {
                Node node = new Node(this, id);
                if (!node.Free)
                {
                    yield return node;
                }
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
#endif
}
