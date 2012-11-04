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

#define USE_32BIT_HASH_KEY
#undef USE_64BIT_HASH_KEY

using System;
using System.Collections.Generic;
using System.Text;

using Sokoban.Engine.Core;

#if USE_32BIT_HASH_KEY
using HashType = System.UInt32;
#endif

#if USE_64BIT_HASH_KEY
using HashType = System.UInt64;
#endif

//using RandomNumberAlgorithm = System.Random;
//using RandomNumberAlgorithm = Sokoban.Engine.MersenneTwister32;
using RandomNumberAlgorithm = Sokoban.Engine.Core.MersenneTwister64;

namespace Sokoban.Engine.Levels
{
    public struct HashKey : IEquatable<HashKey>, IFormattable
    {
        private static HashKey[] zobristKeyTable;
        private const int zobristLimit = 40; // Maximum number of row or columns for Zobrist keys.
        private const int maxCell = (int)Cell.UpperBound;

        public static HashKey GetHashKey(int row, int column, Cell cell)
        {
            return zobristKeyTable[(int)cell * zobristLimit * zobristLimit  + row * zobristLimit + column];
        }

        public static HashKey GetHashKey(Coordinate2D coord, Cell cell)
        {
            return GetHashKey(coord.Row, coord.Column, cell);
        }

        public static HashKey GetSokobanHashKey(int row, int column)
        {
            return zobristKeyTable[(int)Cell.Sokoban * zobristLimit * zobristLimit + row * zobristLimit + column];
        }

        public static HashKey GetSokobanHashKey(Coordinate2D coord)
        {
            return GetSokobanHashKey(coord.Row, coord.Column);
        }

        public static HashKey GetBoxHashKey(int row, int column)
        {
            return zobristKeyTable[(int)Cell.Box * zobristLimit * zobristLimit + row * zobristLimit + column];
        }

        public static HashKey GetBoxHashKey(Coordinate2D coord)
        {
            return GetBoxHashKey(coord.Row, coord.Column);
        }

        private static void InitializeHashKeyTable()
        {
            zobristKeyTable = new HashKey[zobristLimit * zobristLimit * maxCell];
            RandomNumberAlgorithm random = new RandomNumberAlgorithm(0);
            for (int row = 0; row < zobristLimit; row++)
            {
                for (int column = 0; column < zobristLimit; column++)
                {
                    for (int cell = 0; cell < maxCell; cell++)
                    {
                        zobristKeyTable[cell * zobristLimit * zobristLimit + row * zobristLimit + column] = (HashKey)random.Next();
                    }
                }
            }
        }

        static HashKey()
        {
            InitializeHashKeyTable();
        }

        private HashType hashValue;
        private const HashType emptyValue = 0u;

        public static implicit operator HashKey(HashType value)
        {
            return new HashKey(value);
        }

        public static implicit operator HashKey(int value)
        {
            return new HashKey((HashType)value);
        }

        public static implicit operator HashType(HashKey hashKey)
        {
            return hashKey.hashValue;
        }

        public static explicit operator int(HashKey hashKey)
        {
            return (int)hashKey.hashValue;
        }

        public static HashKey Empty
        {
            get
            {
                return new HashKey();
            }
        }

        public bool IsEmpty
        {
            get
            {
                return hashValue == emptyValue;
            }
        }

        public HashKey(HashType hashValue)
        {
            this.hashValue = hashValue;
        }

        public HashKey(HashKey other)
        {
            this.hashValue = other.hashValue;
        }

        public override string ToString()
        {
            return hashValue.ToString();
        }

        public override bool Equals(object obj)
        {
            return Equals((HashKey)obj);
        }

        public override int GetHashCode()
        {
            return (int)hashValue;
        }

        public static bool operator ==(HashKey hashKey1, HashKey hashKey2)
        {
            return hashKey1.Equals(hashKey2);
        }

        public static bool operator !=(HashKey hashKey1, HashKey hashKey2)
        {
            return !hashKey1.Equals(hashKey2);
        }

        #region IEquatable<HashKey> Members

        public bool Equals(HashKey other)
        {
            return hashValue == other.hashValue;
        }

        #endregion

        #region IFormattable Members

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return hashValue.ToString(format, formatProvider);
        }

        #endregion
    }
}
