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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Sokoban.Engine.Collections;
using Sokoban.Engine.Core;
using Sokoban.Engine.Deadlocks;
using Sokoban.Engine.Levels;
using Sokoban.Engine.Paths;
using Sokoban.Engine.Utilities;

namespace Sokoban.Engine.Solvers.Reference
{
#if USE_REFERENCE_TYPE_FOR_NODE
    public class NodeCollection : IEnumerable<Node>
    {
        private int capacity;
        private int nodeCount;
        private int visitedCount;
        private int releasedCount;
        private bool validate;
        private List<Node> nodeList;

        public NodeCollection(int capacity, bool validate)
        {
            this.capacity = capacity;
            this.validate = validate;
            this.nodeList = new List<Node>();
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
                return nodeCount;
            }
        }

        public int Count
        {
            get
            {
                return nodeCount - releasedCount;
            }
        }

        public int Free
        {
            get
            {
                return releasedCount;
            }
        }

        public int Visited
        {
            get
            {
                return visitedCount;
            }
        }

        public int Allocate(Node node)
        {
            if (validate)
            {
                nodeList.Add(node);
            }
            visitedCount++;
            return nodeCount++;
        }

        public void Release(Node node)
        {
            if (validate)
            {
                node.Released = true;
            }
            releasedCount++;
        }

        public void Clear()
        {
            if (validate)
            {
                nodeList.Clear();
            }
            nodeCount = 0;
            visitedCount = 0;
            releasedCount = 0;
        }

        public void ClearFlags()
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                nodeList[i].Free = false;
                nodeList[i].InTree = false;
            }
        }

        public void MarkFree()
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                if (nodeList[i].Released)
                {
                    nodeList[i].Free = true;
                }
            }
        }

        public IEnumerable<Node> AllocatedNodes
        {
            get
            {
                for (int i = 0; i < nodeList.Count; i++)
                {
                    yield return nodeList[i];
                }
            }
        }

        #region IEnumerable<Node> Members

        public IEnumerator<Node> GetEnumerator()
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                if (!nodeList[i].Released)
                {
                    yield return nodeList[i];
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
