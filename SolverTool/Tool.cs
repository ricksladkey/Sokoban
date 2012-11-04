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
    public class Tool : ISolver
    {
        private ISolver solver;

        public Tool()
        {
            this.solver = null;

            SolverAlgorithm = SolverAlgorithm.BruteForce;
            ReuseSolver = true;
            Repetitions = 1;
            DeadlocksDirectory = null;
            CollectSolutions = true;
            CalculateDeadlocks = true;
            HardCodedDeadlocks = false;
            MaximumNodes = 40000000;
            InitialCapacity = 20000000;
            OptimizeMoves = true;
            OptimizePushes = true;
            DetectNoInfluencePushes = true;
            Validate = false;
            Verbose = true;
            Level = null;
        }

        public void Initialize()
        {
            if (Verbose)
            {
                Log.DebugPrint("SolverAlgorithm = {0}", SolverAlgorithm);
                Log.DebugPrint("DeadlocksDirectory = {0}", DeadlocksDirectory);
                Log.DebugPrint("CollectSolutions = {0}", CollectSolutions);
                Log.DebugPrint("CalculateDeadlocks = {0}", CalculateDeadlocks);
                Log.DebugPrint("HardCodedDeadlocks = {0}", HardCodedDeadlocks);
                Log.DebugPrint("MaximumNodes = {0}", MaximumNodes);
                Log.DebugPrint("InitialCapacity = {0}", InitialCapacity);
                Log.DebugPrint("OptimizeMoves = {0}", OptimizeMoves);
                Log.DebugPrint("OptimizePushes = {0}", OptimizePushes);
                Log.DebugPrint("DetectNoInfluencePushes = {0}", DetectNoInfluencePushes);
                Log.DebugPrint("Validate = {0}", Validate);
                Log.DebugPrint("Verbose = {0}", Verbose);
            }
        }


        public void ProcessLevelSet(string filename)
        {
            Log.DebugPrint("Processing level set {0}", filename);
            LevelSet levelSet = new LevelSet(filename);
            int i = 0;
            foreach (Level level in levelSet)
            {
                Log.DebugPrint("solving level {0}...", i + 1);
                ProcessLevel(level);
                i++;
            }
        }

        public void ProcessLevel(string filename, int index)
        {
            Log.DebugPrint("solving level {0} in file {1}", index + 1, filename);
            LevelSet levelSet = new LevelSet(filename);
            ProcessLevel(levelSet[index]);
        }

        private void CreateOrReuseSolver()
        {
            if (!ReuseSolver || solver == null)
            {
                CreateSolver();
            }
        }

        private void CreateSolver()
        {
            solver = Solver.CreateInstance(SolverAlgorithm);
            solver.DeadlocksDirectory = DeadlocksDirectory;
            solver.CollectSolutions = CollectSolutions;
            solver.CalculateDeadlocks = CalculateDeadlocks;
            solver.HardCodedDeadlocks = HardCodedDeadlocks;
            solver.MaximumNodes = MaximumNodes;
            solver.InitialCapacity = InitialCapacity;
            solver.OptimizeMoves = OptimizeMoves;
            solver.OptimizePushes = OptimizePushes;
            solver.DetectNoInfluencePushes = DetectNoInfluencePushes;
            solver.Validate = Validate;
            solver.Verbose = Verbose;
        }

        private void ProcessLevel(Level level)
        {
            bool solved = false;
            Log.DebugPrint(level.AsText);
            if (Repetitions > 1)
            {
                Log.DebugPrint("solving {0} times...", Repetitions);
            }
            TimeSnapshot start = TimeSnapshot.Now;
            for (int i = 0; i < Repetitions; i++)
            {
                CreateOrReuseSolver();
                solver.Level = level;
                solved = solver.Solve();
            }
            TimeSnapshot end = TimeSnapshot.Now;
            Log.DebugPrint("solving took {0} seconds", (end.RealTime - start.RealTime).TotalSeconds);
            if (Repetitions > 1)
            {
                Log.DebugPrint("solving took {0} seconds per solution", (end.RealTime - start.RealTime).TotalSeconds / Repetitions);
            }
            Log.DebugPrint(solver.CancelInfo.Info);
            if (solved)
            {
                if (solver.Solution != null)
                {
                    Log.DebugPrint("solution: {0}", solver.Solution.AsText);
                }
                else
                {
                    Log.DebugPrint("solution not collected");
                }
            }
#if false
            Log.DebugPrint("solving took {0} total CPU seconds", (end.TotalTime - start.TotalTime).TotalSeconds);
            Log.DebugPrint("solving took {0} cycles", (end.PerformanceCounter - start.PerformanceCounter));
            Console.ReadKey();
#endif
        }

        public SolverAlgorithm SolverAlgorithm { get; set; }
        public bool ReuseSolver { get; set; }
        public int Repetitions { get; set; }

        #region ISolver Members

        public string DeadlocksDirectory { get; set; }
        public bool CollectSolutions { get; set; }
        public bool CalculateDeadlocks { get; set; }
        public bool HardCodedDeadlocks { get; set; }
        public Level Level { get; set; }
        public int MaximumNodes { get; set; }
        public int InitialCapacity { get; set; }
        public bool OptimizeMoves { get; set; }
        public bool OptimizePushes { get; set; }
        public bool DetectNoInfluencePushes { get; set; }
        public bool Validate { get; set; }
        public bool Verbose { get; set; }
        public CancelInfo CancelInfo { get; set; }

        public MoveList Solution
        {
            get
            {
                return solver.Solution;
            }
        }

        public string Error
        {
            get
            {
                return solver.Error;
            }
        }

        public bool Solve()
        {
            ProcessLevel(Level);
            return solver.Solution != null;
        }

        #endregion
    }
}
