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
using Sokoban.Engine.Levels;

namespace Sokoban.Engine.Collections
{
    /// <summary>
    /// A ReliablePositionSet position set is a set of distinct positions
    /// for a a level.  Reliable means that the set is 100%
    /// accurate and doesn't depend on hashing to distinguish
    /// positions.
    /// </summary>
    public static class ReliablePositionSet
    {
        public static IPositionSet CreateInstance(Level level, int count)
        {
            if (level.InsideSquares > Byte.MaxValue)
            {
                return new ReliablePositionSetBase<UInt16>(level, count);
            }
            else
            {
                return new ReliablePositionSetBase<Byte>(level, count);
            }
        }

        private class ReliablePositionSetBase<CoordinateKey> : IPositionSet
            where CoordinateKey : IConvertible
        {
            private class PositionComparer : IEqualityComparer<int>
            {
                IEqualityComparer<CoordinateKey> comparer;
                private int boxes;
                private int[] boxCodes;
                private int[] sokobanCodes;
                private CoordinateKey[] keys;

                public PositionComparer(int boxes, int maxKey)
                {
                    comparer = EqualityComparer<CoordinateKey>.Default;
                    this.boxes = boxes;

                    boxCodes = new int[maxKey];
                    sokobanCodes = new int[maxKey];
                    MersenneTwister32 random = new MersenneTwister32(0);
                    for (int i = 0; i < maxKey; i++)
                    {
                        boxCodes[i] = (int)random.Next();
                        sokobanCodes[i] = (int)random.Next();
                    }
                }

                public CoordinateKey[] Keys
                {
                    get
                    {
                        return keys;
                    }
                    set
                    {
                        keys = value;
                    }
                }

                #region IEqualityComparer<CoordinateKey[]> Members

                public bool Equals(int x, int y)
                {
                    for (int i = 0; i <= boxes; i++)
                    {
                        if (!comparer.Equals(keys[x + i], keys[y + i]))
                        {
                            return false;
                        }
                    }
                    return true;
                }

                public int GetHashCode(int key)
                {
                    int hashCode = sokobanCodes[keys[key + boxes].ToInt32(null)];
                    for (int i = 0; i < boxes; i++)
                    {
                        hashCode ^= boxCodes[keys[key + i].ToInt32(null)];
                    }
                    return hashCode;
                }

                #endregion
            }

            private Level level;
            private IComparer<CoordinateKey> coordinateKeyComparer;
            private Coordinate2D[] boxCoordinates;
            private int boxes;
            private int keySize;
            private Array2D<CoordinateKey> insideMap;
            private PositionComparer comparer;
            private ISet<int> set;
            private CoordinateKey[] keys;
            private int currentKey;

            public ReliablePositionSetBase(Level level, int capacity)
            {
                this.level = level;

                coordinateKeyComparer = Comparer<CoordinateKey>.Default;
                boxCoordinates = level.BoxCoordinates;
                boxes = level.Boxes;
                keySize = boxes + 1;

                insideMap = new Array2D<CoordinateKey>(level.Height, level.Width);
                int index = 0;
                foreach (Coordinate2D coord in level.InsideCoordinates)
                {
                    insideMap[coord] = (CoordinateKey)Convert.ChangeType(index, typeof(CoordinateKey));
                    index++;
                }

                keys = new CoordinateKey[keySize * capacity];
                comparer = new PositionComparer(boxes, level.InsideSquares);
                comparer.Keys = keys;
                set = new Set<int>(capacity, comparer);
                currentKey = 0;
            }

            public int Count
            {
                get
                {
                    return set.Count;
                }
            }

            public bool Add()
            {
                return Add(level.SokobanCoordinate);
            }

            public bool Add(Coordinate2D sokobanCoord)
            {
                GetKey(sokobanCoord);
                if (!set.Add(currentKey))
                {
                    return false;
                }
                currentKey += keySize;
                if (currentKey + keySize >= keys.Length)
                {
                    ExpandKeys();
                }
                return true;
            }

            public bool Contains()
            {
                return Contains(level.SokobanCoordinate);
            }

            public bool Contains(Coordinate2D sokobanCoord)
            {
                GetKey(sokobanCoord);
                return set.Contains(currentKey);
            }

            private void GetKey(Coordinate2D sokobanCoord)
            {
                // Get the "key" of the position into the
                // currentKey slot of the key table.
                for (int i = 0; i < boxes; i++)
                {
                    CoordinateKey value = insideMap[boxCoordinates[i]];

                    // Perform an insertion sort so the box
                    // coordinates are in a predictable order.
                    int j = currentKey + i - 1;
                    while (j >= currentKey && coordinateKeyComparer.Compare(keys[j], value) > 0)
                    {
                        keys[j + 1] = keys[j];
                        j--;
                    }
                    keys[j + 1] = value;
                }

                // The sokoban coordinate is always last.
                keys[currentKey + boxes] = insideMap[sokobanCoord];
            }

            private void ExpandKeys()
            {
                CoordinateKey[] newKeys = new CoordinateKey[2 * keys.Length];
                keys.CopyTo(newKeys, 0);
                keys = newKeys;
                comparer.Keys = keys;
            }
        }
    }
}
