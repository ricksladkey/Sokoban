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

#undef DEBUG_LOWER_BOUND

using System;
using System.Collections.Generic;
using System.Text;

using Sokoban.Engine.Collections;
using Sokoban.Engine.Core;
using Sokoban.Engine.Levels;
using Sokoban.Engine.Solvers;

namespace Sokoban.Engine.Deadlocks
{
    class ForewardsSubsetSolver : SubsetSolver
    {
        private Solver solver;

        public ForewardsSubsetSolver(Level level)
            : base(level)
        {
            // Create a solver that won't try to create us recursively.
#if DEBUG_LOWer_BOUND
            solver = Solver.CreateInstance(SolverAlgorithm.BruteForce);
#else
            solver = Solver.CreateInstance(SolverAlgorithm.LowerBound);
#endif
            solver.Level = level;
            solver.CollectSolutions = false;
            solver.OptimizeMoves = false;
            solver.OptimizePushes = true;
            solver.CalculateDeadlocks = false;
            solver.Verbose = false;
            solver.Validate = false;
        }

        public override void PrepareToSolve()
        {
        }

        public override void Solve()
        {
            // A map of all squares where the sokoban solves the set.
            sokobanMap.SetAll(false);

            // Find out whether this set is solvable from
            // all possible sokoban positions or from no
            // sokoban positions.
            solvedAll = true;
            solvedNone = true;

            // Interate over all disconnected regions.
            foreach (Coordinate2D sokobanCoord in regionFinder.Coordinates)
            {
                // Move the sokoban to the new untried square and
                // record all accessible squares as tried.
                level.AddSokoban(sokobanCoord);

                // Try to solve the level from this position.
                bool solved = solver.Solve();
                solvedAll = solvedAll && solved;
                solvedNone = solvedNone && !solved;

                // Record sokoban coordinates that can solve this set.
                if (solved)
                {
                    pathFinder.Find(sokobanCoord);
                    foreach (Coordinate2D coord in level.InsideCoordinates)
                    {
                        if (pathFinder.IsAccessible(coord))
                        {
                            sokobanMap[coord] = true;
                        }
                    }
                }

                // Remove the sokoban for the next iteration.
                level.RemoveSokoban();
            }
        }
    }
}
