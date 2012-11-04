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

using Sokoban.Engine.Collections;
using Sokoban.Engine.Core;
using Sokoban.Engine.Levels;
using Sokoban.Engine.Utilities;

namespace Sokoban.Engine.Deadlocks
{
    public class ForewardsDeadlockFinder : TableBasedDeadlockFinder
    {
        private const int maxSetSize = 100;

        private bool allDeadlocks;

        private int minimumPairsBoxes;
        private int minimumTriplesBoxes;
        private int minimumAllTriplesBoxes;

        private bool estimate;
        private int estimateCount;
        private int actualCount;
        private Level subsetLevel;

        public ForewardsDeadlockFinder(Level level, bool allDeadlocks)
            : base(level)
        {
            this.allDeadlocks = allDeadlocks;
        }

        public void FindeDeadlocks()
        {
            // Initialize counts.
            estimateCount = 0;
            actualCount = 0;

            if (allDeadlocks)
            {
                // For testing calculated deadlocks on small levels or for caching deadlocks.
                minimumPairsBoxes = 2;
                minimumTriplesBoxes = 3;
                minimumAllTriplesBoxes = 3;
            }
            else
            {
                // Provides the best performance for normal use.
                minimumPairsBoxes = 4;
                minimumTriplesBoxes = 6;
                minimumAllTriplesBoxes = 10;
            }

            // Perform two passes.
            for (int i = 0; i < 2; i++)
            {
                // Estimate the number of solver calls on the first pass.
                estimate = i == 0;

                // Find all deadlocked pairs for levels with a sufficient number of boxes.
                if (level.Boxes >= minimumPairsBoxes)
                {
                    FindDeadlockedSets(2);
                }
                else if (level.Boxes >= 2)
                {
                    // Find all frozen deadlocked pairs.
                    FindFrozenDeadlockedSets(2);
                }

                // Find deadlocked triples for levels with a sufficient number of boxes.
                if (level.Boxes >= minimumTriplesBoxes)
                {
                    if (level.Boxes >= minimumAllTriplesBoxes)
                    {
                        // Find all deadlocked triples.
                        FindDeadlockedSets(3);
                    }
                    else
                    {
                        // Find one adjacent triples.
                        FindDeadlockedSets(3, 1);
                    }
                }
                else if (level.Boxes >= 3)
                {
                    // Find all frozen deadlocked triples.
                    FindFrozenDeadlockedSets(3);
                }

                for (int size = 4; size <= maxSetSize && size <= level.Boxes; size++)
                {
                    // Find all sized frozen deadlocked sets.
                    FindFrozenDeadlockedSets(size);
                }
            }

            Log.DebugPrint("estimated = {0}, actual = {1}", estimateCount, actualCount);
        }

        private void FindDeadlockedSets(int size)
        {
            // Find all deadlocked sets of the specifed size
            // with no adjacency conditions.
            FindDeadlockedSets(size, 0);
        }

        private void FindDeadlockedSets(int size, int minimumAdjacent)
        {
            // If only estimating, calculate the estimate;
            if (estimate)
            {
                int nonAdjacent = (int)Math.Pow(freeCoordinates.Length, size - minimumAdjacent);
                int adjacent = (int)Math.Pow(8, minimumAdjacent);
                AddEstimate("solved", size, nonAdjacent + adjacent);
                return;
            }

            // Create a subset level with the boxes placed anywhere and no sokoban.
            subsetLevel = LevelUtils.GetSubsetLevel(level, false, size);

            // Create a frozen deadlock finder for the subset level.
            DeadlockFinder frozenFinder = new FrozenDeadlockFinder(subsetLevel, simpleDeadlockMap);

            // Create the deadlock solver.
            SubsetSolver subsetSolver = new ForewardsSubsetSolver(subsetLevel);
            subsetSolver.CancelInfo = cancelInfo;
            subsetSolver.PrepareToSolve();

            // Iterate over all unique sized sets of free inside coordinates.
            foreach (Coordinate2D[] coords in CoordinateUtils.GetAdjacentCoordinateSets(freeCoordinates, size, minimumAdjacent, false))
            {
                // Check for cancel.
                if (cancelInfo.Cancel)
                {
                    return;
                }

                // Bump the actual count.
                ++actualCount;

                // Move the boxes into postion.
                subsetLevel.MoveBoxes(coords);

                // Skip sets that are already complete.
                if (subsetLevel.IsComplete)
                {
                    continue;
                }

                // Don't bother trying to solve deadlocks that
                // the frozen deadlock finder can find.
                if (frozenFinder.IsDeadlocked())
                {
                    // But do add the deadlock if there is
                    // no unconditional proper subset that
                    // is deadlocked.
                    if (!IsAnyProperSubsetDeadlocked(true, coords))
                    {
                        AddDeadlock(coords);
                    }
                    continue;
                }

                // If any subset is deadlocked then skip this set.
                if (IsAnyProperSubsetDeadlocked(coords))
                {
                    continue;
                }

#if DEBUG
                // Verify minimum adjacency.
                if (minimumAdjacent > 0 && CountAdjacent(false, coords) < minimumAdjacent)
                {
                    throw new Exception("set enumerator violated adjacency");
                }
#endif

                if (IsPositionSolvable(subsetSolver, coords))
                {
                    // Skip a priori solvable levels.
                    continue;
                }

                // Try to solve the level.
                subsetSolver.Solve();

                // If not solved add the deadlocked set.
                if (subsetSolver.SolvedNone)
                {
                    // This is an unconditional deadlock set.
                    AddDeadlock(coords);
                }
                else if (!subsetSolver.SolvedAll)
                {
                    // This deadlock depends on the sokoban.
                    AddDeadlock(subsetSolver.SokobanMap, coords);
                }
            }

            // Finish adding deadlocks.
            PromoteDeadlocks();
        }

        private void AddEstimate(string type, int size, int count)
        {
            Log.DebugPrint("estimating type {0} of size {1} as {2}", type, size, count);
            estimateCount += count;
        }

#if DEBUG
        private int CountAdjacent(bool fourNeighborAdjacent, params Coordinate2D[] coords)
        {
            // Count the number of adjacent pairs.
            int adjacent = 0;
            for (int i = 0; i < coords.Length - 1; i++)
            {
                for (int j = i + 1; j < coords.Length; j++)
                {
                    int distance = fourNeighborAdjacent ?
                        Coordinate2D.GetOrthogonalDistance(coords[i], coords[j]) :
                        Coordinate2D.GetDiagonalDistance(coords[i], coords[j]);
                    if (distance == 1)
                    {
                        adjacent++;
                    }
                }
            }
            return adjacent;
        }
#endif

        private bool IsAnyBoxIndependent(params Coordinate2D[] coords)
        {
            foreach (Coordinate2D coord in coords)
            {
                if (IsBoxIndependent(coord, coords))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsPositionSolvable(SubsetSolver subsetSolver, params Coordinate2D[] coords)
        {
            bool solvable = false;
#if DEBUG
            string tag = null;
#endif

#if false
            if (coords.Length >= 2)
            {
                // Only try levels that create discontiguous regions.
                if (!solvable && subsetSolver.IsInsideConnected)
                {
                    tag = "contiguous";
                    solvable = true;
                }

                // XXX: Not reliable; needs research.
                if (!solvable && IsAnyBoxIndependent(coords))
                {
                    tag = "independent box";
                    solvable = true;
                }
            }
#endif

#if DEBUG
            if (solvable)
            {
                // Verify that the position is solvable.
                subsetSolver.Solve();
                if (!subsetSolver.SolvedAll)
                {
                    Log.DebugPrint(subsetLevel.AsText);
                    throw new Exception(String.Format("set tagged {0} not solvable", tag));
                }
            }
#endif

            return solvable;
        }

        private bool IsBoxIndependent(Coordinate2D coord, params Coordinate2D[] coords)
        {
            foreach (Coordinate2D neighbor in coord.EightNeighbors)
            {
                if (level.IsWall(neighbor))
                {
                    return false;
                }
            }
            foreach (Coordinate2D otherCoord in coords)
            {
                if (otherCoord != coord && Coordinate2D.GetDiagonalDistance(coord, otherCoord) <= 1)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
