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
    public static class DeadlockUtils
    {
        public static DeadlockFinder LoadDeadlocks(Level level, string deadlocksDirectory)
        {
            string deadlockFilename = GetDeadlockFilename(level, deadlocksDirectory);
            if (File.Exists(deadlockFilename))
            {
                return DeadlockFinder.CreateInstance(level,
                    DeadlockUtils.GetDeadlocks(new LevelSet(deadlockFilename)));
            }
            return null;
        }

        public static void SaveDeadlocks(DeadlockFinder deadlockFinder, string deadlocksDirectory)
        {
            Level level = deadlockFinder.Level;
            string deadlockFilename = GetDeadlockFilename(level, deadlocksDirectory);
            LevelSet deadlockLevelSet = DeadlockUtils.GetDeadlockLevelSet(level, deadlockFinder.Deadlocks);
            deadlockLevelSet.Name = "Calculated Deadlocks";
            deadlockLevelSet.SaveAs(deadlockFilename);
        }

        public static string GetDeadlockFilename(Level level, string deadlocksDirectory)
        {
            return String.Format("{0}{1}.xsb", deadlocksDirectory, DeadlockUtils.GetDeadlockHashKey(level));
        }

        public static HashKey GetDeadlockHashKey(Level level)
        {
            return (UInt32)LevelUtils.GetSubsetLevel(level, true, 0).GetHashKey();
        }

        public static IEnumerable<Deadlock> GetDeadlocks(IEnumerable<Level> levels)
        {
            foreach (Level level in levels)
            {
                yield return GetDeadlock(level);
            }
        }

        public static LevelSet GetDeadlockLevelSet(Level level, IEnumerable<Deadlock> deadlocks)
        {
            return new LevelSet(GetDeadlocksAsLevels(level, deadlocks));
        }

        private static Deadlock GetDeadlock(Level level)
        {
            // Decode the map.
            Array2D<bool> map = null;
            if (level.Targets != 0)
            {
                map = new Array2D<bool>(level.Height, level.Width);
                foreach (Coordinate2D coord in level.TargetCoordinates)
                {
                    map[coord] = true;
                }
            }
            return new Deadlock(map, level.BoxCoordinates);
        }

        private static IEnumerable<Level> GetDeadlocksAsLevels(Level level, IEnumerable<Deadlock> deadlocks)
        {
            foreach (Deadlock deadlock in deadlocks)
            {
                yield return GetDeadlockAsLevel(level, deadlock);
            }
        }

        private static Level GetDeadlockAsLevel(Level level, Deadlock deadlock)
        {
            Array2D<Cell> newData = new Array2D<Cell>(level.Data);
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                newData[coord] &= ~(Cell.Sokoban | Cell.Box | Cell.Target);
            }
            foreach (Coordinate2D coord in deadlock.Coordinates)
            {
                newData[coord] |= Cell.Box;
            }
            if (deadlock.SokobanMap != null)
            {
                foreach (Coordinate2D coord in level.InsideCoordinates)
                {
                    if (deadlock.SokobanMap[coord])
                    {
                        newData[coord] |= Cell.Target;
                    }
                }
            }
            return new Level(newData);
        }

        public static void ShowDeadlocks(TextWriter writer, bool verbose, Level level, IEnumerable<Deadlock> deadlocks)
        {
            if (verbose)
            {
                // Enumerate deadlocked sets.
                foreach (Deadlock deadlock in deadlocks)
                {
                    ShowDeadlock(writer, level, deadlock);
                }
            }

            // Collect statistics about the deadlocks.
            int maxSetSize = 100;
            int total = 0;
            int[] totalSized = new int[maxSetSize + 1];
            int conditional = 0;
            int[] conditionalSized = new int[maxSetSize + 1];
            foreach (Deadlock deadlock in deadlocks)
            {
                if (deadlock.Coordinates.Length < totalSized.Length)
                {
                    totalSized[deadlock.Coordinates.Length]++;
                    if (deadlock.SokobanMap != null)
                    {
                        conditionalSized[deadlock.Coordinates.Length]++;
                    }
                }
                total++;
                if (deadlock.SokobanMap != null)
                {
                    conditional++;
                }
            }

            // Print out the statistics.
            for (int size = 0; size < totalSized.Length; size++)
            {
                if (totalSized[size] == 0)
                {
                    continue;
                }

                writer.WriteLine("{0} deadlocks with set size {1}, conditional: {2}", totalSized[size], size, conditionalSized[size]);
            }

            // Print out the summary.
            writer.WriteLine("------");
            writer.WriteLine("{0} deadlocks total, conditional: {1}", total, conditional);
        }

        public static void ShowDeadlocks(bool verbose, Level level, IEnumerable<Deadlock> deadlocks)
        {
            ShowDeadlocks(Log.LogOut, verbose, level, deadlocks);
        }

        private static void ShowDeadlock(TextWriter writer, Level level, Deadlock deadlock)
        {
            string text = String.Format("length {0}:", deadlock.Coordinates.Length);
            foreach (Coordinate2D coord in deadlock.Coordinates)
            {
                text += String.Format(" ({0}, {1})", coord.Row, coord.Column);
            }
            if (deadlock.SokobanMap != null)
            {
                text += String.Format(" {0}", "conditional");
            }
            writer.WriteLine(text);

            Log.DebugPrint(GetDeadlockAsLevel(level, deadlock).AsText);
        }

        private static void ShowDeadlock(Level level, Deadlock deadlock)
        {
            ShowDeadlock(Log.LogOut, level, deadlock);
        }

    }
}
