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
    /// A node in the solver's search tree.
    /// </summary>
    /// <remarks>
    /// This object is a lightweight stack-based object that
    /// encapsulates a reference to an entry in a NodeData
    /// array.  The idea is that it behaves like a reference
    /// type, exposing all the properties of the NodeData
    /// object as though it were a full-fledged object.
    /// </remarks>
    public struct Node
    {
        /// <summary>
        /// The data behind a node in the solver's search tree.
        /// </summary>
        /// <remarks>
        /// Our node is a value type so that we can
        /// allocate millions of them at one time
        /// to avoid massive amounts of allocation
        /// and garbage collection overhead.
        /// </remarks>
        public struct NodeData
        {
            public int child;
            public int sibling;
            public short pushes;
            public short moves;
            public short score;
            public byte row;
            public byte column;
            public byte direction;
            public byte flags;
#if DEBUG
            public short lowerBound;
            public HashKey hashKey;
#endif
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private NodeCollection nodes;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int id;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private NodeData[] array;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int index;

        private static Node empty;

        public const int MaxMoves = short.MaxValue;

        public const int MaxPushes = short.MaxValue;

        public const int MaxScore = short.MaxValue;

        static Node()
        {
            empty = new Node(null, -1);
        }

        public static Node Empty
        {
            get
            {
                return empty;
            }
        }

        public static bool IsEmpty(Node node)
        {
            return node.id == -1;
        }

        public Node(NodeCollection nodes, int id)
        {
            this.nodes = nodes;
            this.id = id;
            if (id == -1)
            {
                this.array = null;
                this.index = 0;
            }
            else
            {
                this.array = nodes.GetArray(this.id);
                this.index = nodes.GetIndex(this.id);
            }
        }

        public Node(NodeCollection nodes, int row, int column, Direction direction, int pushes, int moves)
        {
            this.nodes = nodes;
            this.id = nodes.Allocate();
            this.array = nodes.GetArray(this.id);
            this.index = nodes.GetIndex(this.id);

            array[index].row = (byte)row;
            array[index].column = (byte)column;
            array[index].direction = (byte)direction;
            array[index].pushes = (short)pushes;
            array[index].moves = (short)moves;
            array[index].score = 0;
            array[index].child = -1;
            array[index].sibling = -1;
            array[index].flags = (byte)NodeFlags.Empty;
#if DEBUG
            array[index].hashKey = HashKey.Empty;
            array[index].lowerBound = 0;
#endif
        }

#if false
        private NodeData[] array
        {
            get
            {
                return nodes.GetArray(id);
            }
        }

        private int index
        {
            get
            {
                return nodes.GetIndex(id);
            }
        }
#endif

        public Node Child
        {
            get
            {
                return new Node(nodes, array[index].child);
            }
        }

        public Node Sibling
        {
            get
            {
                return new Node(nodes, array[index].sibling);
            }
        }

        public int ID
        {
            get
            {
                return id;
            }
        }

        public int Row
        {
            get
            {
                return array[index].row;
            }
        }

        public int Column
        {
            get
            {
                return array[index].column;
            }
        }

        public Coordinate2D Coordinate
        {
            get
            {
                return new Coordinate2D(array[index].row, array[index].column);
            }
        }

        public Direction Direction
        {
            get
            {
                return (Direction)array[index].direction;
            }
        }

        public int Pushes
        {
            get
            {
                return array[index].pushes;
            }
            set
            {
                array[index].pushes = (short)value;
            }
        }

        public int Moves
        {
            get
            {
                return array[index].moves;
            }
            set
            {
                array[index].moves = (short)value;
            }
        }

        public int Score
        {
            get
            {
                return array[index].score;
            }
            set
            {
                array[index].score = (short)value;
            }
        }

#if DEBUG
        public int LowerBound
        {
            get
            {
                return array[index].lowerBound;
            }
            set
            {
                array[index].lowerBound = (short)value;
            }
        }
#endif

        public bool Searched
        {
            get
            {
                return FlagGetter(NodeFlags.Searched);
            }
            set
            {
                FlagSetter(NodeFlags.Searched, value);
            }
        }

        public bool Complete
        {
            get
            {
                return FlagGetter(NodeFlags.Complete);
            }
            set
            {
                FlagSetter(NodeFlags.Complete, value);
            }
        }

        public bool Terminal
        {
            get
            {
                return FlagGetter(NodeFlags.Terminal);
            }
            set
            {
                FlagSetter(NodeFlags.Terminal, value);
            }
        }

        public bool Dormant
        {
            get
            {
                return FlagGetter(NodeFlags.Dormant);
            }
            set
            {
                FlagSetter(NodeFlags.Dormant, value);
            }
        }

        public bool InTable
        {
            get
            {
                return FlagGetter(NodeFlags.InTable);
            }
            set
            {
                FlagSetter(NodeFlags.InTable, value);
            }
        }

        public bool InTree
        {
            get
            {
                return FlagGetter(NodeFlags.InTree);
            }
            set
            {
                FlagSetter(NodeFlags.InTree, value);
            }
        }

        public bool Free
        {
            get
            {
                return FlagGetter(NodeFlags.Free);
            }
            set
            {
                FlagSetter(NodeFlags.Free, value);
            }
        }

#if DEBUG
        public HashKey HashKey
        {
            get
            {
                return array[index].hashKey;
            }
            set
            {
                array[index].hashKey = value;
            }
        }
#endif

        public bool HasChildren
        {
            get
            {
                return array[index].child != -1;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IEnumerable<Node> Children
        {
            get
            {
                for (Node child = new Node(nodes, array[index].child); !Node.IsEmpty(child); child = child.Sibling)
                {
                    yield return child;
                }
            }
        }

        [DebuggerDisplay("Children", Name = "Children")]
        private Node[] DebugerChildren
        {
            get
            {
                return new List<Node>(Children).ToArray();
            }
        }

        public void Add(Node child)
        {
            child.array[child.index].sibling = array[index].child;
            array[index].child = child.id;
        }

        public void Remove(Node previous, Node child)
        {
            if (previous.id == id)
            {
                array[index].child = child.array[child.index].sibling;
            }
            else
            {
                previous.array[previous.index].sibling = child.array[child.index].sibling;
            }
        }

        public static bool operator ==(Node node1, Node node2)
        {
            return node1.id == node2.id;
        }

        public static bool operator !=(Node node1, Node node2)
        {
            return !(node1 == node2);
        }

        public override bool Equals(object obj)
        {
            return this == (Node)obj;
        }

        public override int GetHashCode()
        {
            return index;
        }

        public override string ToString()
        {
            return NodeHelper.AsText(this);
        }

        private bool FlagGetter(NodeFlags flag)
        {
            return ((NodeFlags)array[index].flags & flag) != 0;
        }

        private void FlagSetter(NodeFlags flag, bool value)
        {
            array[index].flags = (byte)(((NodeFlags)array[index].flags & ~flag) | (value ? flag : 0));
        }
    }
#endif
}
