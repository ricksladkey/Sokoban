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
using System.IO;

using Sokoban.Engine.Collections;
using Sokoban.Engine.Core;
using Sokoban.Engine.Levels;
using Sokoban.Engine.Utilities;

namespace Sokoban.Engine.Deadlocks
{
    public class DeadlockFinder
    {
        // Factory for creating deadlock finders.
        public static DeadlockFinder CreateInstance(Level level, bool calculateDeadlocks, bool hardCodedDeadlocks)
        {
            if (calculateDeadlocks)
            {
                return new BackwardsDeadlockFinder(level, false);
            }

            if (hardCodedDeadlocks)
            {
                return new HardCodedDeadlockFinder(level);
            }

            return new TableBasedDeadlockFinder(level);

#if false
            return new BackwardsDeadlockFinder(level);

            return new ForewardsDeadlockFinder(level, allDeadlocks);

            return new TableBasedDeadlockFinder(level, true);

            return new FrozenDeadlockFinder(level);

            return new HardCodedDeadlockFinder(level);

            return new LevelUtilsDeadlockFinder(level);

            DeadlockFinder finder1 = new HardCodedDeadlockFinder(level);
            DeadlockFinder finder2 = new FrozendDeadlockFinder(level);
            return new ComparisonDeadlockFinder(level, finder1, finder2);
#endif
        }

        public static DeadlockFinder CreateInstance(Level level, bool calculateDeadlocks)
        {
            return CreateInstance(level, calculateDeadlocks, false);
        }

        public static DeadlockFinder CreateInstance(Level level)
        {
            return CreateInstance(level, false);
        }

        public static DeadlockFinder CreateInstance(Level level, IEnumerable<Deadlock> deadlocks)
        {
            return new TableBasedDeadlockFinder(level, deadlocks);
        }

        public static Array2D<bool> GetSimpleDeadlockMap(Level level)
        {
            DeadlockFinder deadlockFinder = new DeadlockFinder(level);
            return deadlockFinder.SimpleDeadlockMap;
        }

        protected Level level;
        protected Array2D<bool> simpleDeadlockMap;
        protected Array2D<Cell> data;
        protected Coordinate2D[] boxCoordinates;
        protected int boxes;
        protected CancelInfo cancelInfo;

        public DeadlockFinder(Level level)
        {
            this.level = level;
            CalculateSimpleDeadlockMap();

            Initialize();
        }

        protected DeadlockFinder(Level level, Array2D<bool> simpleDeadlockMap)
        {
            this.level = level;
            this.simpleDeadlockMap = simpleDeadlockMap;

            Initialize();
        }

        private void Initialize()
        {
            this.data = level.Data;
            this.boxCoordinates = level.BoxCoordinates;
            this.boxes = level.Boxes;
            this.cancelInfo = new CancelInfo();
        }

        public CancelInfo CancelInfo
        {
            get
            {
                return cancelInfo;
            }
            set
            {
                cancelInfo = value;
            }
        }

        public Level Level
        {
            get
            {
                return level;
            }
        }

        public Array2D<bool> SimpleDeadlockMap
        {
            get
            {
                return simpleDeadlockMap;
            }
        }

        public virtual IEnumerable<Deadlock> Deadlocks
        {
            get
            {
                yield break;
            }
        }

        public virtual void FindDeadlocks()
        {
        }

        public virtual bool IsDeadlocked(int sokobanRow, int sokobanColumn)
        {
            return IsSimplyDeadlocked();
        }

        public bool IsDeadlocked(Coordinate2D coord)
        {
            return IsDeadlocked(coord.Row, coord.Column);
        }

        public bool IsDeadlocked()
        {
            return IsDeadlocked(level.SokobanRow, level.SokobanColumn);
        }

        public bool IsSimplyDeadlocked()
        {
            int n = boxCoordinates.Length;
            for (int i = 0; i < n; i++)
            {
                if (simpleDeadlockMap[boxCoordinates[i].Row, boxCoordinates[i].Column])
                {
                    return true;
                }
            }
            return false;
        }

        private void CalculateSimpleDeadlockMap()
        {
            // Create a simple deadlock map with all inside squares initialized to true.
            simpleDeadlockMap = new Array2D<bool>(level.Height, level.Width);
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                simpleDeadlockMap[coord] = true;
            }

            // For each target visit as many squares as possible using pulls only.
            foreach (Coordinate2D coord in level.TargetCoordinates)
            {
                VisitAllPulls(coord);
            }
        }

        private void VisitAllPulls(Coordinate2D coord)
        {
            if (!simpleDeadlockMap[coord])
            {
                return;
            }

            simpleDeadlockMap[coord] = false;

            foreach (Direction direction in Direction.Directions)
            {
                if (level.IsFloor(coord + direction) && level.IsFloor(coord + 2 * direction))
                {
                    VisitAllPulls(coord + direction);
                }
            }
        }
    }
}
