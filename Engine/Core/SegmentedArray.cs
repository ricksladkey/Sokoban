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
    /// A SegmentedArray is an array that is allocated in segments.
    /// </summary>
    /// <typeparam name="T">The type of the array element</typeparam>
    /// <remarks>
    /// Unfortunately, the only way to set a field of a
    /// value type that is part of a collection is to
    /// copy the whole data structure or to use a literal array.
    /// As a result, a segmented array exposes methods to
    /// get the segment and offset into the segment instead
    /// of exposing an item indexer to hide the segments from
    /// the client of the segmented array.  In other words,
    /// for a genuine array, array[index].field = value works
    /// even for value types but for any other collection,
    /// collection[index].field = value is not permitted.
    /// Put yet another way, a user-defined function cannot
    /// return a reference to a value type.
    /// </remarks>
    public class SegmentedArray<T>
    {
        const int defaultSegmentSize = 0x10000;
        const int defaultNumberOfSegments = 1024;
        private int segmentSize;
        private int segmentBits;
        private int segmentMask;
        private int segmentCount;
        private int itemCount;
        private T[][] segments;
        private int maximumCapacity;

        public SegmentedArray()
            : this(defaultSegmentSize)
        {
        }

        public SegmentedArray(int approximateSegmentSize)
        {
            this.segmentBits = (int)Math.Ceiling(Math.Log(approximateSegmentSize) / Math.Log(2));
            this.segmentSize = (int)Math.Round(Math.Pow(2, this.segmentBits));
            this.segmentMask = this.segmentSize - 1;
            this.segmentCount = 1;
            this.segments = new T[defaultNumberOfSegments][];
            this.segments[0] = new T[this.segmentSize];
            this.maximumCapacity = this.segmentSize;
        }

        private void AllocateSegment()
        {
            if (segments.Length == segmentCount)
            {
                Array.Resize<T[]>(ref segments, 2 * segmentCount);
            }
            segments[segmentCount++] = new T[segmentSize];
            maximumCapacity = segmentCount * segmentSize;
        }

        public void Clear()
        {
            itemCount = 0;
        }

        public void Allocate(int count)
        {
            while (count > maximumCapacity)
            {
                AllocateSegment();
            }
            itemCount = count;
        }

        public int Allocate()
        {
            if (itemCount == maximumCapacity)
            {
                AllocateSegment();
            }
            return itemCount++;
        }

        public void TrimExcess()
        {
            segmentCount = (itemCount >> segmentBits) + 1;
            for (int segment = segmentCount; segment < segments.Length; segment++)
            {
                segments[segment] = null;
            }
            maximumCapacity = segmentCount * segmentSize;
        }

        public T[] GetSegment(int index)
        {
            return segments[index >> segmentBits];
        }

        public int GetOffset(int index)
        {
            return index & segmentMask;
        }
    }
}
