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

namespace Sokoban.Engine.Solvers.Reference
{
#if USE_REFERENCE_TYPE_FOR_NODE
    public sealed class Node
    {
        public const int MaxMoves = short.MaxValue;

        public const int MaxPushes = short.MaxValue;

        public const int MaxScore = short.MaxValue;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Node child;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Node sibling;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int id;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int row;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int column;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Direction direction;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int pushes;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int moves;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int score;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private byte flags;
#if DEBUG
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private HashKey hashKey;
#endif


        public static Node Empty
        {
            get
            {
                return null;
            }
        }

        public static bool IsEmpty(Node node)
        {
            return node == null;
        }

        public Node(NodeCollection nodes, int row, int column, Direction direction, int pushes, int moves)
        {
            this.id = nodes.Allocate(this);
            this.row = row;
            this.column = column;
            this.direction = direction;
            this.pushes = pushes;
            this.moves = moves;
            this.score = 0;
#if DEBUG
            this.hashKey = HashKey.Empty;
#endif
        }

        public Node Child
        {
            get
            {
                return child;
            }
        }

        public Node Sibling
        {
            get
            {
                return sibling;
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
                return row;
            }
        }

        public int Column
        {
            get
            {
                return column;
            }
        }

        public Coordinate2D Coordinate
        {
            get
            {
                return new Coordinate2D(row, column);
            }
        }

        public Direction Direction
        {
            get
            {
                return direction;
            }
        }

        public int Pushes
        {
            get
            {
                return pushes;
            }
            set
            {
                pushes = value;
            }
        }

        public int Moves
        {
            get
            {
                return moves;
            }
            set
            {
                moves = value;
            }
        }

        public int Score
        {
            get
            {
                return score;
            }
            set
            {
                score = value;
            }
        }

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
                return hashKey;
            }
            set
            {
                hashKey = value;
            }
        }
#endif

        public bool HasChildren
        {
            get
            {
                return child != null;
            }
        }

        public IEnumerable<Node> Children
        {
            get
            {
                for (Node child = this.child; child != null; child = child.sibling)
                {
                    yield return child;
                }
            }
        }

        public void Add(Node child)
        {
            child.sibling = this.child;
            this.child = child;
        }

        public void Remove(Node previous, Node child)
        {
            if (previous == this)
            {
                previous.child = child.sibling;
            }
            else
            {
                previous.sibling = child.sibling;
            }
        }

        public override string ToString()
        {
            return NodeHelper.AsText(this);
        }

        private bool FlagGetter(NodeFlags flag)
        {
            return ((NodeFlags)flags & flag) != 0;
        }

        private void FlagSetter(NodeFlags flag, bool value)
        {
            flags = (byte)(((NodeFlags)flags & ~flag) | (value ? flag : 0));
        }
    }
#endif
}
