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

using Sokoban.Engine.Core;

namespace Sokoban.Engine.Levels
{
    public struct OperationDirectionPair : IEquatable<OperationDirectionPair>
    {
        public static bool operator ==(OperationDirectionPair pair1, OperationDirectionPair pair2)
        {
            return pair1.Equals(pair2);
        }

        public static bool operator !=(OperationDirectionPair pair1, OperationDirectionPair pair2)
        {
            return !pair1.Equals(pair2);
        }

        public static OperationDirectionPair GetOpposite(OperationDirectionPair pair)
        {
            return new OperationDirectionPair(Operation.GetOpposite(pair.Operation), Direction.GetOpposite(pair.Direction));
        }

        public static OperationDirectionPair Rotate(OperationDirectionPair pair)
        {
            return new OperationDirectionPair(pair.Operation, Direction.Rotate(pair.Direction));
        }

        public static OperationDirectionPair Mirror(OperationDirectionPair pair)
        {
            return new OperationDirectionPair(pair.Operation, Direction.Mirror(pair.Direction));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Operation operation;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Direction direction;

        public Operation Operation
        {
            get
            {
                return operation;
            }
            set
            {
                operation = value;
            }
        }

        public Direction Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
            }
        }

        public OperationDirectionPair(Operation operation, Direction direction)
        {
            this.operation = operation;
            this.direction = direction;
        }

        public override string ToString()
        {
            return String.Format("({0}, {1})", this.Operation, this.Direction);
        }

        public override bool Equals(object obj)
        {
            return Equals((OperationDirectionPair)obj);
        }

        public override int GetHashCode()
        {
            return (int)operation ^ (int)direction;
        }

        #region IEquatable<OperationDirectionPair> Members

        public bool Equals(OperationDirectionPair other)
        {
            return operation == other.operation && direction == other.direction;
        }

        #endregion
    }
}
