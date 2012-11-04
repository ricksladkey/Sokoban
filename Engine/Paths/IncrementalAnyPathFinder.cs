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

#undef DEBUG_INCREMENTAL_FIND
#undef VALIDATE_INCREMENTAL_FIND
#undef INSTRUMENT_INCREMENTAL_FIND

using System;
using System.Collections.Generic;
using System.Text;

using Sokoban.Engine.Core;
using Sokoban.Engine.Levels;
using Sokoban.Engine.Utilities;

namespace Sokoban.Engine.Paths
{
    public class IncrementalAnyPathFinder : ArrayPathFinder
    {
        private int[][] insideCoordinates;
        private bool[] visited;
        private int count;
        private bool isDirty;
        int findRow;
        int findColumn;

        public IncrementalAnyPathFinder(Level level)
            : base(level)
        {
            this.isDirty = true;
            this.insideCoordinates = level.InsideCoordinates;
            this.visited = new bool[m];
        }

        #region PathFinder Members

        public override void Find(int row, int column)
        {
#if DEBUG_INCREMENTAL_FIND
            Log.DebugPrint("+Find({0}, {1}), isDirty = {2}", row, column, isDirty);
#endif
            isDirty = false;

            Array.Clear(visited, firstInside, insideCount);
            for (int i = 0; i < boxCount; i++)
            {
                visited[boxCoordinates[i].Row * n + boxCoordinates[i].Column] = true;
            }

            q.Clear();
            count = 0;

            ContinueFinding(row, column);
#if DEBUG_INCREMENTAL_FIND
            Log.DebugPrint("-Find({0}, {1}), isDirty = {2}", row, column, isDirty);
#endif
        }

        public void ContinueFinding(int row, int column)
        {
#if DEBUG_INCREMENTAL_FIND
            Log.DebugPrint("+ContinueFinding({0}, {1}), isDirty = {2}", row, column, isDirty);
#endif
            int u = row * n + column;
            visited[u] = true;
            count++;

            while (true)
            {
                int[] neighbors = neighborMap[u];
                int neighborCount = neighbors.Length;
                for (int i = 0; i < neighborCount; i++)
                {
                    int v = neighbors[i];
                    if (!visited[v])
                    {
                        visited[v] = true;
                        count++;
                        q.Enqueue(v);
                    }
                }

                if (q.IsEmpty)
                {
                    break;
                }
                u = q.Dequeue();
            }
#if DEBUG_INCREMENTAL_FIND
            Log.DebugPrint("-ContinueFinding({0}, {1}), isDirty = {2}", row, column, isDirty);
#endif
        }

        public override void IncrementalFind(int row, int column)
        {
#if DEBUG_INCREMENTAL_FIND
            Log.DebugPrint("+IncrementalFind({0}, {1}), isDirty = {2}", row, column, isDirty);
#endif
            // If the finder is dirty or the user is requesting
            // a find from a different region of the level, we
            // need to perform a full calculation, otherwise
            // it's up to date.
            bool doFind = isDirty || !IsAccessible(row, column);
#if INSTRUMENT_INCREMENTAL_FIND
            RecordStatistics(isDirty, doFind);
#endif
            if (doFind)
            {
                Find(row, column);
            }
#if DEBUG_INCREMENTAL_FIND
            Log.DebugPrint("-IncrementalFind({0}, {1}), isDirty = {2}", row, column, isDirty);
#endif
        }

        public override void ForceFullCalculation()
        {
            isDirty = true;
        }

        public override bool IsAccessible(int row, int column)
        {
            return visited[row * n + column] && !Level.IsBox(data[row, column]);
        }

        public override int GetDistance(int row, int column)
        {
            return visited[row * n + column] && !Level.IsBox(data[row, column]) ? 0 : DefaultInaccessible;
        }

        public override Coordinate2D GetFirstAccessibleCoordinate()
        {
            for (int u = firstInside; u <= lastInside; u++)
            {
                if (visited[u])
                {
                    int row = u / n;
                    int column = u % n;
                    if (!Level.IsBox(data[row, column]))
                    {
                        return new Coordinate2D(row, column);
                    }
                }
            }
            return Coordinate2D.Undefined;
        }

        public override IEnumerable<Coordinate2D> AccessibleCoordinates
        {
            get
            {
                for (int u = firstInside; u <= lastInside; u++)
                {
                    if (visited[u])
                    {
                        int row = u / n;
                        int column = u % n;
                        if (!Level.IsBox(data[row, column]))
                        {
                            yield return new Coordinate2D(row, column);
                        }
                    }
                }
            }
        }

        public override int AccessibleSquares
        {
            get
            {
                return count;
            }
        }

        public override void MoveBox(Operation operation, int oldBoxRow, int oldBoxColumn, int newBoxRow, int newBoxColumn)
        {
#if DEBUG_INCREMENTAL_FIND
            Log.DebugPrint("+MoveBox({0}, {1}, {2}, {3}, {4}), isDirty = {5}", operation, oldBoxRow, oldBoxColumn, newBoxRow, newBoxColumn, isDirty);
#endif
            MoveBoxAndTryToUpdate(operation, oldBoxRow, oldBoxColumn, newBoxRow, newBoxColumn);
#if DEBUG_INCREMENTAL_FIND
            Log.DebugPrint("-MoveBox({0}, {1}, {2}, {3}, {4}), isDirty = {5}", operation, oldBoxRow, oldBoxColumn, newBoxRow, newBoxColumn, isDirty);
            if (!isDirty)
            {
                string result = LevelEncoder.CombineLevels(level.AsText, AsText);
                Log.DebugPrint("-MoveBox PathFinder:\n{0}", result);
            }
#endif
        }

        public override void MoveBoxAndIncrementalFind(Operation operation, int oldBoxRow, int oldBoxColumn, int newBoxRow, int newBoxColumn)
        {
#if DEBUG_INCREMENTAL_FIND
            Log.DebugPrint("+MoveBoxAndIncrementalFind({0}, {1}, {2}, {3}, {4}), isDirty = {5}", operation, oldBoxRow, oldBoxColumn, newBoxRow, newBoxColumn, isDirty);
#endif
            MoveBoxAndTryToUpdate(operation, oldBoxRow, oldBoxColumn, newBoxRow, newBoxColumn);
            if (isDirty)
            {
                System.Diagnostics.Debug.Assert(level.IsEmpty(findRow, findColumn));
                Find(findRow, findColumn);
            }
            System.Diagnostics.Debug.Assert(isDirty == false);
#if DEBUG_INCREMENTAL_FIND
            Log.DebugPrint("-MoveBoxAndIncrementalFind({0}, {1}, {2}, {3}, {4}), isDirty = {5}", operation, oldBoxRow, oldBoxColumn, newBoxRow, newBoxColumn, isDirty);
            string result = LevelEncoder.CombineLevels(level.AsText, AsText);
            Log.DebugPrint("-MoveBoxAndIncrementalFind PathFinder:\n{0}", result);
#endif
        }

        public override MoveList GetPath(int row, int column)
        {
            throw new InvalidOperationException("shortest path not supported by this path finder");
        }

        #endregion

        private void MoveBoxAndTryToUpdate(Operation operation, int oldBoxRow, int oldBoxColumn, int newBoxRow, int newBoxColumn)
        {
            bool shouldContinue = false;
            int parallelRow = newBoxRow - oldBoxRow;
            int parallelColumn = newBoxColumn - oldBoxColumn;

            // We cannot handle "power" moves.
            if (Math.Abs(parallelRow) + Math.Abs(parallelColumn) != 1)
            {
                isDirty = true;
                parallelRow = parallelRow > 0 ? 1 : (parallelRow < 0 ? -1 : 0);
                parallelColumn = parallelColumn > 0 ? 1 : (parallelColumn < 0 ? -1 : 0);
#if INSTRUMENT_INCREMENTAL_FIND
                PowerMovesCount++;
#endif
            }

            if (isDirty)
            {
                level.MoveBox(oldBoxRow, oldBoxColumn, newBoxRow, newBoxColumn);

                if (operation == Operation.Push)
                {
                    findRow = oldBoxRow;
                    findColumn = oldBoxColumn;
                }
                else if (operation == Operation.Pull)
                {
                    findRow = newBoxRow + parallelRow;
                    findColumn = newBoxColumn + parallelColumn;
                }
#if INSTRUMENT_INCREMENTAL_FIND
                if (operation == Operation.Push)
                {
                    DirtyPushCount++;
                }
                else if (operation == Operation.Pull)
                {
                    DirtyPullCount++;
                }
#endif
                return;
            }

#if VALIDATE_INCREMENTAL_FIND
            ValidateBeforeUpdate(operation, oldBoxRow, oldBoxColumn, newBoxRow, newBoxColumn);
#endif

            int perpendicularRow = parallelColumn;
            int perpendicularColumn = parallelRow;

            if (operation == Operation.Push)
            {
                // No areas will be closed off if:
                // 1) The new box square is inaccessible, or
                // 2) If both squares on either side of the new box square
                //    are either occupied are are accessible from the square
                //    beyond the new box square.
                bool newBoxSquareIsAccessible = IsAccessible(newBoxRow, newBoxColumn);
                shouldContinue =
                    !newBoxSquareIsAccessible ||
                    ((Level.IsWallOrBox(data[newBoxRow + perpendicularRow, newBoxColumn + perpendicularColumn]) ||
                    (Level.IsEmpty(data[newBoxRow + parallelRow, newBoxColumn + parallelColumn]) &&
                    Level.IsEmpty(data[newBoxRow + perpendicularRow + parallelRow, newBoxColumn + perpendicularColumn + parallelColumn]))) &&
                    ((Level.IsWallOrBox(data[newBoxRow - perpendicularRow, newBoxColumn - perpendicularColumn]) ||
                    (Level.IsEmpty(data[newBoxRow + parallelRow, newBoxColumn + parallelColumn]) &&
                    Level.IsEmpty(data[newBoxRow - perpendicularRow + parallelRow, newBoxColumn - perpendicularColumn + parallelColumn])))));

                level.MoveBox(oldBoxRow, oldBoxColumn, newBoxRow, newBoxColumn);

                if (shouldContinue)
                {
                    // If new box square was accessible,
                    // count decreases.
                    if (newBoxSquareIsAccessible)
                    {
                        count--;
                    }
                    visited[newBoxRow * n + newBoxColumn] = true;
                    ContinueFinding(oldBoxRow, oldBoxColumn);
                }
                else
                {
                    isDirty = true;
                    findRow = oldBoxRow;
                    findColumn = oldBoxColumn;
                }
            }
            else if (operation == Operation.Pull)
            {
                // No areas will be closed off if:
                //    If both squares on either side of the new box square
                //    are either occupied or are accessible from the
                //    new sokoban square.
                shouldContinue =
                    ((Level.IsWallOrBox(data[newBoxRow + perpendicularRow, newBoxColumn + perpendicularColumn]) ||
                    (Level.IsEmpty(data[newBoxRow + perpendicularRow + parallelRow, newBoxColumn + perpendicularColumn + parallelColumn]))) &&
                    ((Level.IsWallOrBox(data[newBoxRow - perpendicularRow, newBoxColumn - perpendicularColumn]) ||
                    (Level.IsEmpty(data[newBoxRow - perpendicularRow + parallelRow, newBoxColumn - perpendicularColumn + parallelColumn])))));

                level.MoveBox(oldBoxRow, oldBoxColumn, newBoxRow, newBoxColumn);

                if (shouldContinue)
                {
                    // New box square was by definition accessible and already visited.
                    count--;

                    // Determine whether the old box square is now accessible.
                    bool isNowAccessible =
                        IsAccessible(oldBoxRow + perpendicularRow, oldBoxColumn + perpendicularColumn) ||
                        IsAccessible(oldBoxRow - perpendicularRow, oldBoxColumn - perpendicularColumn) ||
                        IsAccessible(oldBoxRow - parallelRow, oldBoxColumn - parallelColumn);

                    if (isNowAccessible)
                    {
                        // Although we are continuing finding, it's not
                        // from where the sokoban is, it's the other side.
                        ContinueFinding(oldBoxRow, oldBoxColumn);
                    }
                    else
                    {
                        // Was a box and is now inaccessible: count unchanged.
                        visited[oldBoxRow * n + oldBoxColumn] = false;
                    }
                }
                else
                {
                    isDirty = true;
                    findRow = newBoxRow + parallelRow;
                    findColumn = newBoxColumn + parallelColumn;
                }
            }

#if VALIDATE_INCREMENTAL_FIND
            if (!isDirty)
            {
                ValidateAfterUpdate(operation, oldBoxRow, oldBoxColumn, newBoxRow, newBoxColumn);
            }
#endif

#if INSTRUMENT_INCREMENTAL_FIND
            RecordStatistics(operation, shouldContinue);
#endif

        }

#if INSTRUMENT_INCREMENTAL_FIND

        public static int PowerMovesCount = 0;
        public static int DirtyPushCount = 0;
        public static int DirtyPullCount = 0;
        public static int FindDirtyCount = 0;
        public static int FindInaccessibleCount = 0;
        public static int FindCleanCount = 0;
        public static int PushContinueCount = 0;
        public static int PushFullCount = 0;
        public static int PullContinueCount = 0;
        public static int PullFullCount = 0;

        private void RecordStatistics(bool isDirty, bool doFind)
        {
            if (isDirty)
            {
                FindDirtyCount++;
            }
            else if (doFind)
            {
                FindInaccessibleCount++;
            }
            else
            {
                FindCleanCount++;
            }
        }

        private void RecordStatistics(Operation operation, bool shouldContinue)
        {
            if (operation == Operation.Push)
            {
                if (shouldContinue)
                {
                    PushContinueCount++;
                }
                else
                {
                    PushFullCount++;
                }
            }
            else if (operation == Operation.Pull)
            {
                if (shouldContinue)
                {
                    PullContinueCount++;
                }
                else
                {
                    PullFullCount++;
                }
            }
        }
#endif

#if VALIDATE_INCREMENTAL_FIND
        public PathFinder pathFinder = null;

        private void ValidateBeforeUpdate(Operation operation, int oldBoxRow, int oldBoxColumn, int newBoxRow, int newBoxColumn)
        {
            if (pathFinder == null)
            {
                pathFinder = PathFinder.CreateInstance(level);
            }
            if (operation == Operation.Push)
            {
                int parallelRow = newBoxRow - oldBoxRow;
                int parallelColumn = newBoxColumn - oldBoxColumn;

                pathFinder.Find(oldBoxRow - parallelRow, oldBoxColumn - parallelColumn);
            }
            else if (operation == Operation.Pull)
            {
                pathFinder.Find(newBoxRow, newBoxColumn);
            }
            CheckAgainstPathFinder();
        }

        private void ValidateAfterUpdate(Operation operation, int oldBoxRow, int oldBoxColumn, int newBoxRow, int newBoxColumn)
        {
            if (pathFinder == null)
            {
                pathFinder = PathFinder.CreateInstance(level);
            }
            if (operation == Operation.Push)
            {
                pathFinder.Find(oldBoxRow, oldBoxColumn);
            }
            else if (operation == Operation.Pull)
            {
                int parallelRow = newBoxRow - oldBoxRow;
                int parallelColumn = newBoxColumn - oldBoxColumn;

                pathFinder.Find(newBoxRow + parallelRow, newBoxColumn + parallelColumn);
            }
            CheckAgainstPathFinder();
        }

        private void CheckAgainstPathFinder()
        {
            bool differs = false;
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                if (IsAccessible(coord) != pathFinder.IsAccessible(coord))
                {
                    differs = true;
                    break;
                }
            }
            if (differs)
            {
                Log.DebugPrint("Level:\n{0}", level.AsText);
                Log.DebugPrint("Incremental PathFinder:\n{0}", AsText);
                Log.DebugPrint("Full PathFinder:\n{0}", pathFinder.AsText);
                throw new Exception("assert!");
            }
        }
#endif
    }
}
