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

using Sokoban.Engine.Core;

namespace Sokoban.Engine.Levels
{
    public struct Operation : IEquatable<Operation>
    {
        private enum Value
        {
            Move = 1,
            Push = 2,
            Pull = 3,
        }

        public static Operation Move = new Operation(Value.Move);
        public static Operation Push = new Operation(Value.Push);
        public static Operation Pull = new Operation(Value.Pull);

        public static Operation GetOpposite(Operation operation)
        {
            if (operation == Operation.Move)
            {
                return Operation.Move;
            }
            if (operation == Operation.Push)
            {
                return Operation.Pull;
            }
            if (operation == Operation.Pull)
            {
                return Operation.Push;
            }
            throw new InvalidOperationException("Invalid operation");
        }

        public static explicit operator int(Operation operation)
        {
            return (int)operation.value;
        }

        public static explicit operator Operation(byte operation)
        {
            return new Operation((Value)operation);
        }

        public static bool operator ==(Operation operation1, Operation operation2)
        {
            return operation1.value == operation2.value;
        }

        public static bool operator !=(Operation operation1, Operation operation2)
        {
            return operation1.value != operation2.value;
        }

        private Value value;

        private Operation(Value value)
        {
            this.value = value;
        }

        public Operation(Operation other)
        {
            this.value = other.value;
        }

        public override string ToString()
        {
            return String.Format("{0}", value);
        }

        public override bool Equals(object obj)
        {
            return Equals((Direction)obj);
        }

        public override int GetHashCode()
        {
            return (int)value;
        }

        #region IEquatable<Operation> Members

        public bool Equals(Operation other)
        {
            return value == other.value;
        }

        #endregion
    }
}
