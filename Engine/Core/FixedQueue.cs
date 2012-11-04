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

namespace Sokoban.Engine.Core
{
    /// <summary>
    /// A FixedQueue is a simple queue with an upper limit
    /// on the number of items that can be queued without
    /// clearing.
    /// </summary>
    /// <remarks>
    /// A FixedQueue doesn't clear unused entries so dangling
    /// references to previously queued items will persist.
    /// However, it can be safely used for pure value types
    /// and any objects that persist longer than the queue
    /// itself.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class FixedQueue<T>
    {
        T[] items;
        int head;
        int tail;

        public int Count
        {
            get
            {
                return tail - head;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return head == tail;
            }
        }

        public FixedQueue(int maxItems)
        {
            items = new T[maxItems];
            Clear();
        }

        public void Clear()
        {
            head = 0;
            tail = 0;
        }

        public void Enqueue(T item)
        {
            items[tail++] = item;
        }

        public T Dequeue()
        {
            return items[head++];
        }
    }
}
