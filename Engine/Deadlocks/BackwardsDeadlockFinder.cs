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

namespace Sokoban.Engine.Deadlocks
{
    public class BackwardsDeadlockFinder : TableBasedDeadlockFinder
    {
        private const int maximumBoxes = 4;
        private const int capacityLimit = 1000000;

        private bool assessPossibility;

        public BackwardsDeadlockFinder(Level level)
            : this(level, false)
        {
        }

        public BackwardsDeadlockFinder(Level level, bool assessPossibility)
            : base(level)
        {
            this.assessPossibility = assessPossibility;
        }

        public override void FindDeadlocks()
        {
            // Find frozen deadlocks first.
            base.FindDeadlocks();

            // Iterate over all set sizes between two and the number of boxes.
            for (int size = 2; size <= maximumBoxes && size < level.Targets; size++)
            {
                // Check for cancel.
                if (cancelInfo.Cancel)
                {
                    return;
                }

                // Give feedback to the user.
                string info =
                    "Calculating deadlocks...\r\n" +
                    string.Format("    Finding deadlocks of size {0}.", size);
                cancelInfo.Info = info;

                // Find all deadlocked sets using the backwards solver.
                FindDeadlockedSets(size);
            }
        }

        private void FindDeadlockedSets(int size)
        {
            // Create a subset level with the boxes placed anywhere and no sokoban.
            Level subsetLevel = LevelUtils.GetSubsetLevel(level, false, size);

            // Create the subset solver.
            int capacity = CoordinateUtils.GetNumberOfSets(freeCoordinates.Length, size);
            if (capacity >= capacityLimit)
            {
                // No point in failing due to memory exhaustion.
                return;
            }
            SubsetSolver subsetSolver = new BackwardsSubsetSolver(level, subsetLevel, capacity, assessPossibility);
            subsetSolver.CancelInfo = cancelInfo;
            subsetSolver.PrepareToSolve();

            // Give feedback to the user.
            string info =
                "Calculating deadlocks...\r\n" +
                string.Format("    Examining all positions of size {0} for deadlocks.", size);
            cancelInfo.Info = info;

            // Iterate over all unique sized sets of free inside coordinates.
            foreach (Coordinate2D[] coords in CoordinateUtils.GetCoordinateSets(freeCoordinates, size))
            {
                // Check for cancel.
                if (cancelInfo.Cancel)
                {
                    return;
                }

                // Move the boxes into position.
                subsetLevel.MoveBoxes(coords);

                // Skip sets that are already complete.
                if (subsetLevel.IsComplete)
                {
                    continue;
                }

                // If any subset is deadlocked then skip this set.
                if (IsDeadlocked(false, subsetLevel))
                {
                    continue;
                }

                // Try to solve the position.
                subsetSolver.Solve();

                // If not solved add the deadlocked set.
                if (subsetSolver.SolvedNone)
                {
                    // This is an unconditional deadlocked set.
                    AddDeadlock(coords);
                }
                else if (!subsetSolver.SolvedAll)
                {
                    Array2D<bool> map = subsetSolver.SokobanMap;

                    // This deadlock depends on the sokoban.
                    AddDeadlock(map, coords);
                }
            }

            // Finish adding deadlocks.
            PromoteDeadlocks();
        }
    }
}
