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
using System.Diagnostics;

using Sokoban.Engine.Collections;
using Sokoban.Engine.Core;
using Sokoban.Engine.Deadlocks;
using Sokoban.Engine.Levels;
using Sokoban.Engine.Paths;
using Sokoban.Engine.Solvers;
using Sokoban.Engine.Utilities;

namespace Sokoban.SolverTool
{
    public class Tool
    {
        private ISolver solver;

        public Tool()
        {
            this.solver = null;
        }

        public void ProcessLevelSet(string filename)
        {
            Log.DebugPrint("Processing level set {0}", filename);
            LevelSet levelSet = new LevelSet(filename);
            int i = 0;
            foreach (Level level in levelSet)
            {
                ProcessLevel(level, i);
                i++;
            }
        }

        public void ProcessLevel(string filename, int index)
        {
            Log.DebugPrint("Processing level {0} in file {1}", index + 1, filename);
            LevelSet levelSet = new LevelSet(filename);
            ProcessLevel(levelSet[index], index);
        }

        private void ProcessLevel(Level level, int index)
        {
            Log.DebugPrint("solving level {0}...", index + 1);
            TimeSnapshot start = TimeSnapshot.Now;
            solver = Solver.CreateInstance();
            solver.Verbose = true;
            solver.Level = level;
            Log.DebugPrint("solver.OptimizeMoves: {0}", solver.OptimizeMoves);
            Log.DebugPrint("solver.OptimizePushes: {0}", solver.OptimizePushes);
            Log.DebugPrint("solver.CalculateDeadlocks: {0}", solver.CalculateDeadlocks);
            Log.DebugPrint("solver.InitialCapacity: {0}", solver.InitialCapacity);
            Log.DebugPrint("solver.MaximumNodes: {0}", solver.MaximumNodes);
            bool solved = solver.Solve();
            TimeSnapshot end = TimeSnapshot.Now;
            Log.DebugPrint("solving took {0} seconds", (end.RealTime - start.RealTime).TotalSeconds);
            Log.DebugPrint(solver.CancelInfo.Info);
            if (solved)
            {
                Log.DebugPrint("solution: {0}", solver.Solution.AsText);
            }
#if false
            Log.DebugPrint("solving took {0} total CPU seconds", (end.TotalTime - start.TotalTime).TotalSeconds);
            Log.DebugPrint("solving took {0} cycles", (end.PerformanceCounter - start.PerformanceCounter));
            Console.ReadKey();
#endif
        }
    }
}
