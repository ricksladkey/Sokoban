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

#if DEBUG
#undef VERIFY_DEADLOCKS
#endif

using System;
using System.Collections.Generic;
using System.Text;

using Sokoban.Engine.Collections;
using Sokoban.Engine.Core;
using Sokoban.Engine.Levels;
using Sokoban.Engine.Solvers;

namespace Sokoban.Engine.Deadlocks
{
    public class TableBasedDeadlockFinder : DeadlockFinder
    {
        private class WorkingEntry
        {
            public bool Deadlocked;
            public Array2D<bool> Map;
            public Coordinate2D Coordinate;
            public Coordinate2D[] Coordinates;
            public List<WorkingEntry> Entries;

            public WorkingEntry(bool deadlocked, Array2D<bool> map, Coordinate2D coordinate, Coordinate2D[] coordinates)
            {
                Deadlocked = deadlocked;
                Map = map;
                Coordinate = coordinate;
                Coordinates = coordinates;
            }
        }

        private class Entry
        {
            public bool Deadlocked;
            public Array2D<bool> Map;
            public Coordinate2D Coordinate;
            public Coordinate2D[] Coordinates;
            public Entry[] Entries;

            public Entry(bool deadlocked, Array2D<bool> map, Coordinate2D coordinate, Coordinate2D[] coordinates, Entry[] entries)
            {
                Deadlocked = deadlocked;
                Map = map;
                Coordinate = coordinate;
                Coordinates = coordinates;
                Entries = entries;
            }
        }

        protected Coordinate2D[] freeCoordinates;

        private Array2D<List<WorkingEntry>> workingEntriesMap;
        private Array2D<Entry[]> entriesMap;

        public TableBasedDeadlockFinder(Level level)
            : base(level)
        {
            Initialize();
        }

        public TableBasedDeadlockFinder(Level level, IEnumerable<Deadlock> deadlocks)
            : base(level)
        {
            Initialize();

            // Iterate over all the deadlocks.
            foreach (Deadlock deadlock in deadlocks)
            {
                // Add the entry.
                AddDeadlock(deadlock);
            }

            // Finish adding deadlocks.
            PromoteDeadlocks();
        }

        public override void FindDeadlocks()
        {
            // This becomes pathological if we try to find
            // sets that are too large.
            int limit = Math.Min(level.Boxes, 10);

            for (int size = 2; size <= limit; size++)
            {
                // Check for cancel.
                if (cancelInfo.Cancel)
                {
                    return;
                }

                // Give feedback to the user.
                string info =
                    "Calculating deadlocks...\r\n" +
                    string.Format("    Finding frozen deadlocks of size {0}.", size);
                cancelInfo.Info = info;

                // Find frozen deadlocked sets using the frozen deadlock finder.
                FindFrozenDeadlockedSets(size);
            }
        }

        private void Initialize()
        {
            // Calculate the set of free coordinates.
            List<Coordinate2D> freeCoordinateList = new List<Coordinate2D>();
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                if (!simpleDeadlockMap[coord])
                {
                    freeCoordinateList.Add(coord);
                }
            }
            freeCoordinates = freeCoordinateList.ToArray();

            // Prepare to start adding deadlocks.
            workingEntriesMap = new Array2D<List<WorkingEntry>>(level.Height, level.Width);
            entriesMap = new Array2D<Entry[]>(level.Height, level.Width);
        }

        public override IEnumerable<Deadlock> Deadlocks
        {
            get
            {
                // Enumerate all deadlocks.
                foreach (Coordinate2D coord in level.InsideCoordinates)
                {
                    if (entriesMap[coord] != null)
                    {
                        foreach (Deadlock deadlock in GetDeadlocks(entriesMap[coord]))
                        {
                            yield return deadlock;
                        }
                    }
                }
            }
        }

        private IEnumerable<Deadlock> GetDeadlocks(Entry[] entries)
        {
            foreach (Entry entry in entries)
            {
                if (entry.Deadlocked)
                {
                    yield return new Deadlock(entry.Map, entry.Coordinates);
                }
                if (entry.Entries != null)
                {
                    foreach (Deadlock deadlock in GetDeadlocks(entry.Entries))
                    {
                        yield return deadlock;
                    }
                }
            }
        }

        protected void AddDeadlock(Deadlock deadlock)
        {
            AddDeadlock(deadlock.SokobanMap, deadlock.Coordinates);
        }

        protected void AddDeadlock(params Coordinate2D[] coords)
        {
            AddDeadlock(null, coords);
        }

        protected void AddDeadlock(Array2D<bool> map, params Coordinate2D[] coords)
        {
            // Copy and adjust the map.
            Array2D<bool> adjustedMap = null;
            if (map != null)
            {
                adjustedMap = new Array2D<bool>(map);

                // Kludge: Add the unknown sokoban coordinate as solving
                // this deadlock because if we don't know where the
                // sokoban is, we don't know for a fact that the
                // deadlock is not solvable.
                adjustedMap[0, 0] = true;
            }

            // Copy and sort the coordinates.
            Coordinate2D[] sortedCoords = CoordinateUtils.SortCoordinates(coords);

            // Add the entry.
            AddDeadlockEntry(adjustedMap, sortedCoords);
        }

        private void AddDeadlockEntry(Array2D<bool> map, params Coordinate2D[] coords)
        {
#if VERIFY_DEADLOCKS
            VerifyDeadlock(map, coords);
#endif

            // Create a top level entry list if it doesn't exist.
            List<WorkingEntry> entries = workingEntriesMap[coords[0]];
            if (entries == null)
            {
                entries = new List<WorkingEntry>();
                workingEntriesMap[coords[0]] = entries;
            }

            // Iterate through the middle coordinates.
            for (int i = 1; i < coords.Length - 1; i++)
            {
                WorkingEntry entry = null;
                foreach (WorkingEntry candidate in entries)
                {
                    if (candidate.Coordinate == coords[i])
                    {
                        entry = candidate;
                        break;
                    }
                }
                if (entry == null)
                {
                    entry = new WorkingEntry(false, null, coords[i], null);
                    entries.Add(entry);
                }
                if (entry.Entries == null)
                {
                    entries = new List<WorkingEntry>();
                    entry.Entries = entries;
                }
                else
                {
                    entries = entry.Entries;
                }
            }

            // Look for a pre-existing entry.
            foreach (WorkingEntry entry in entries)
            {
                if (entry.Coordinate == coords[coords.Length - 1])
                {
                    if (!entry.Deadlocked)
                    {
                        entry.Deadlocked = true;
                        entry.Map = map;
                        entry.Coordinates = coords;
                        return;
                    }
                    throw new Exception("adding duplicate deadlock");
                }
            }

            // Add the entry.
            entries.Add(new WorkingEntry(true, map, coords[coords.Length - 1], coords));
        }

        private void VerifyDeadlock(Array2D<bool> map, params Coordinate2D[] coords)
        {
            Level subsetLevel = LevelUtils.GetSubsetLevel(level, false, coords);
            ISolver solver = Solver.CreateInstance(SolverAlgorithm.LowerBound);
            solver.Level = subsetLevel;
            solver.OptimizeMoves = false;
            solver.OptimizePushes = true;
            solver.CalculateDeadlocks = false;
            solver.HardCodedDeadlocks = true;
            foreach (Coordinate2D coord in subsetLevel.InsideCoordinates)
            {
                if (map == null || !map[coord])
                {
                    if (subsetLevel.IsEmpty(coord))
                    {
                        subsetLevel.AddSokoban(coord);
                        break;
                    }
                }
            }
            if (solver.Solve())
            {
                throw new Exception("deadlock failed verify test");
            }
        }

        protected void PromoteDeadlocks()
        {
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                entriesMap[coord] = GetFinalEntries(workingEntriesMap[coord]);
            }
        }

        private Entry[] GetFinalEntries(List<WorkingEntry> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return null;
            }
            List<Entry> newEntries = new List<Entry>();
            foreach (WorkingEntry entry in entries)
            {
                newEntries.Add(new Entry(entry.Deadlocked, entry.Map, entry.Coordinate, entry.Coordinates, GetFinalEntries(entry.Entries)));
            }
            newEntries.Sort(delegate(Entry entry1, Entry entry2) { return entry1.Coordinate.CompareTo(entry2.Coordinate); });
            return newEntries.ToArray();
        }

        protected void FindFrozenDeadlockedSets(int size)
        {
            // Create a subset level with the boxes placed anywhere and no sokoban.
            Level deadlockLevel = LevelUtils.GetSubsetLevel(level, false, size);

            // Create a frozen deadlock finder for the subset level.
            DeadlockFinder frozenFinder = new FrozenDeadlockFinder(deadlockLevel, simpleDeadlockMap);

            // Iterate over all unique sets of adjacent free inside coordinates
            // of the specified size consisting only of squares added in
            // alternating perpendicular directions.
            foreach (Coordinate2D[] coords in CoordinateUtils.GetAlternatingAxisSets(freeCoordinates, size))
            {
                // Check for cancel.
                if (cancelInfo.Cancel)
                {
                    return;
                }

                // Move the boxes into position.
                deadlockLevel.MoveBoxes(coords);

                // Check whether the set is a frozen deadlock.
                if (frozenFinder.IsDeadlocked() && !IsDeadlocked(true, deadlockLevel))
                {
                    // Add the deadlock.
                    AddDeadlock(coords);
                }
            }

            // Finish adding deadlocks.
            PromoteDeadlocks();
        }

        protected bool IsSetDeadlocked(params Coordinate2D[] coords)
        {
            return IsSetDeadlocked(false, coords);
        }

        protected bool IsSetDeadlocked(bool unconditionally, params Coordinate2D[] coords)
        {
            // Check whether the set of specified sorted coordinates is
            // presently in the working deadlock entries map.

            int n = coords.Length;

            // Look for a top level entry.
            Entry[] entries = entriesMap[coords[0]];
            if (entries == null)
            {
                return false;
            }

            // Work through the middle coordinates.
            for (int i = 1; i < n - 1; i++)
            {
                foreach (Entry entry in entries)
                {
                    if (entry.Coordinate == coords[i])
                    {
                        entries = entry.Entries;
                        if (entries == null)
                        {
                            return false;
                        }
                        break;
                    }
                }
            }

            // Search the final entries list.
            foreach (Entry entry in entries)
            {
                if (entry.Deadlocked && entry.Coordinate == coords[n - 1])
                {
                    // Obey unconditional restrictions.
                    if (!unconditionally || entry.Map == null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected bool IsAnyProperSubsetDeadlocked(params Coordinate2D[] coords)
        {
            return IsAnyProperSubsetDeadlocked(false, coords);
        }

        protected bool IsAnyProperSubsetDeadlocked(bool unconditionally, params Coordinate2D[] coords)
        {
            // Handle all possible subset sizes from two or one less than the set size.
            for (int k = 2; k < coords.Length; k++)
            {
                foreach (Coordinate2D[] subsetCoords in CoordinateUtils.GetCoordinateSets(coords, k))
                {
                    if (IsSetDeadlocked(unconditionally, subsetCoords))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected bool IsAnySubsetDeadlocked(params Coordinate2D[] coords)
        {
            return IsAnySubsetDeadlocked(false, coords);
        }

        protected bool IsAnySubsetDeadlocked(bool unconditionally, params Coordinate2D[] coords)
        {
            return IsSetDeadlocked(unconditionally, coords) || IsAnyProperSubsetDeadlocked(unconditionally, coords);
        }

        public override bool IsDeadlocked(int sokobanRow, int sokobanColumn)
        {
            for (int i = 0; i < boxes; i++)
            {
                Entry[] entries = entriesMap[boxCoordinates[i].Row, boxCoordinates[i].Column];
                if (entries != null && IsDeadlocked(entries, sokobanRow, sokobanColumn))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsDeadlocked(Entry[] entries, int sokobanRow, int sokobanColumn)
        {
            int n = entries.Length;
            for (int i = 0; i < n; i++)
            {
                Entry entry = entries[i];
                if (Level.IsBox(data[entry.Coordinate.Row, entry.Coordinate.Column]))
                {
                    if (entry.Deadlocked)
                    {
                        Array2D<bool> map = entry.Map;
                        if (map == null || !map[sokobanRow, sokobanColumn])
                        {
                            return true;
                        }
                    }
                    if (entry.Entries != null && IsDeadlocked(entry.Entries, sokobanRow, sokobanColumn))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected bool IsDeadlocked(bool unconditionally, Level otherLevel)
        {
            if (entriesMap == null)
            {
                return false;
            }

            Array2D<Cell> otherData = otherLevel.Data;
            Coordinate2D[] otherBoxCoordinates = otherLevel.BoxCoordinates;
            int otherBoxes = otherBoxCoordinates.Length;
            for (int i = 0; i < otherBoxes; i++)
            {
                Entry[] entries = entriesMap[otherBoxCoordinates[i].Row, otherBoxCoordinates[i].Column];
                if (entries != null && IsDeadlocked(unconditionally, otherData, entries))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsDeadlocked(bool unconditionally, Array2D<Cell> otherData, Entry[] entries)
        {
            int n = entries.Length;
            for (int i = 0; i < n; i++)
            {
                Entry entry = entries[i];
                if (Level.IsBox(otherData[entry.Coordinate.Row, entry.Coordinate.Column]))
                {
                    if (entry.Deadlocked)
                    {
                        if (!unconditionally || entry.Map == null)
                        {
                            return true;
                        }
                    }
                    if (entry.Entries != null && IsDeadlocked(unconditionally, otherData, entry.Entries))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
