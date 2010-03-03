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
using Sokoban.Engine.Levels;

namespace Sokoban.Engine.Collections
{
    public class TransformationInvariantLevelSet
    {
        private bool includeSokoban;
        private Hashtable<Level, int> set;
        private int count;

        public bool IncludeSokoban
        {
            get
            {
                return includeSokoban;
            }
            set
            {
                includeSokoban = value;
            }
        }

        public TransformationInvariantLevelSet(int capacity)
        {
            includeSokoban = true;
            set = new Hashtable<Level, int>(capacity);
            count = 0;
        }

        public TransformationInvariantLevelSet()
            : this(100000)
        {
        }

        public int this[Level level]
        {
            get
            {
                return Lookup(level);
            }
        }

        public bool Contains(Level level)
        {
            return Lookup(level) > 0;
        }

        private int Lookup(Level level)
        {
            if (includeSokoban)
            {
                return LookupKey(level);
            }

            Level tempLevel = new Level(level);
            tempLevel.MarkAccessible = true;
            tempLevel.RemoveSokoban();
            return LookupKey(tempLevel);
        }

        private int LookupKey(Level level)
        {
            return set.ContainsKey(level) ? set[level] : -1;
        }

        public void Add(Level level)
        {
            Add(level, count);
        }

        public void Add(Level level, int value)
        {
            Level r0 = new Level(level);
            AddLevel(r0, value);
            Level r90 = LevelUtils.GetRotatedLevel(r0);
            AddLevel(r90, value);
            Level r180 = LevelUtils.GetRotatedLevel(r90);
            AddLevel(r180, value);
            Level r270 = LevelUtils.GetRotatedLevel(r180);
            AddLevel(r270, value);
            Level r0m = LevelUtils.GetMirroredLevel(r0);
            AddLevel(r0m, value);
            Level r90m = LevelUtils.GetMirroredLevel(r90);
            AddLevel(r90m, value);
            Level r180m = LevelUtils.GetMirroredLevel(r180);
            AddLevel(r180m, value);
            Level r270m = LevelUtils.GetMirroredLevel(r270);
            AddLevel(r270m, value);
            count++;
        }

        private void AddLevel(Level level, int value)
        {
            level.MarkAccessible = true;
            level.RemoveSokoban();
            set[level] = value;
        }
    }
}
