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
    public class UnreliablePositionSet : IPositionSet
    {
        private Level level;
        private ISet<HashKey> set;

        public UnreliablePositionSet(Level level, int capacity)
        {
            this.level = level;

            set = new Set<HashKey>(capacity);
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
            HashKey key = level.GetOccupantsHashKey() ^ HashKey.GetSokobanHashKey(sokobanCoord);
            return set.Add(key);
        }

        public bool Contains()
        {
            return Contains(level.SokobanCoordinate);
        }

        public bool Contains(Coordinate2D sokobanCoord)
        {
            HashKey key = level.GetOccupantsHashKey() ^ HashKey.GetSokobanHashKey(sokobanCoord);
            return set.Contains(key);
        }
    }
}
