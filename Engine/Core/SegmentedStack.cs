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
    /// A segmented stack is a stack that allocates
    /// its memory in segments.  Segments are
    /// reused so memory allocation ceases once
    /// the stack reaches its maximum size.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SegmentedStack<T> : IStack<T>
    {
        private const int defaultSegmentSize = 0x10000;

        private int segmentSize;
        private List<T[]> segments;
        private List<T[]> freeSegments;
        private T[] currentSegment;
        private int currentIndex;
        private object syncRoot;

        public SegmentedStack()
            : this(defaultSegmentSize)
        {
        }

        public SegmentedStack(IEnumerable<T> collection)
            : this(defaultSegmentSize)
        {
            foreach (T item in collection)
            {
                Push(item);
            }
        }

        public SegmentedStack(int segmentSize)
        {
            this.segmentSize = segmentSize;

            segments = new List<T[]>();
            freeSegments = new List<T[]>();
            currentSegment = new T[segmentSize];
            currentIndex = 0;
            syncRoot = new object();
        }

        public bool IsEmpty
        {
            get
            {
                return currentIndex == 0 && segments.Count == 0;
            }
        }

        public int Count
        {
            get
            {
                return segments.Count * segmentSize + currentIndex;
            }
        }

        public void Push(T item)
        {
            if (currentIndex == segmentSize)
            {
                PushSegment();
            }
            currentSegment[currentIndex++] = item;
        }

        public T Pop()
        {
            if (currentIndex == 0)
            {
                PopSegment();
            }
            return currentSegment[--currentIndex];
        }

        public T Peek()
        {
            if (currentIndex == 0)
            {
                return segments[segments.Count - 1][segmentSize - 1];
            }
            return currentSegment[currentIndex - 1];
        }

        public void TrimExcess()
        {
            freeSegments.Clear();
        }

        public void Clear()
        {
            this.freeSegments.AddRange(this.segments);
            this.segments.Clear();
            this.currentIndex = 0;
        }

        public bool Contains(T item)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            foreach (T stackItem in this)
            {
                if (comparer.Equals(item, stackItem))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int next = arrayIndex;
            foreach (T item in this)
            {
                array[next++] = item;
            }
        }

        private void PushSegment()
        {
            segments.Add(currentSegment);
            if (freeSegments.Count > 0)
            {
                currentSegment = freeSegments[freeSegments.Count - 1];
                freeSegments.RemoveAt(freeSegments.Count - 1);
            }
            else
            {
                currentSegment = new T[segmentSize];
            }
            currentIndex = 0;
        }

        private void PopSegment()
        {
            if (segments.Count == 0)
            {
                throw new InvalidOperationException("empty stack");
            }
            currentIndex = segmentSize;
            freeSegments.Add(currentSegment);
            currentSegment = segments[segments.Count - 1];
            segments.RemoveAt(segments.Count - 1);
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T[] segment in segments)
            {
                foreach (T item in segment)
                {
                    yield return item;
                }
            }
            for (int index = 0; index < currentIndex; index++)
            {
                yield return currentSegment[index];
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion


        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            int next = index;
            foreach (T item in this)
            {
                array.SetValue(item, next++);
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                return syncRoot;
            }
        }

        #endregion
    }
}
