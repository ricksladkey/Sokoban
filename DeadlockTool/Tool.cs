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

namespace Sokoban.DeadlockTool
{
    public class Tool
    {
        private string deadlocksDirectory;
        private bool forceCalculate;

        public Tool()
        {
            deadlocksDirectory = "Deadlocks\\";
            forceCalculate = false;
        }

        public string DeadlocksDirectory
        {
            get
            {
                return deadlocksDirectory;
            }
            set
            {
                deadlocksDirectory = value;
            }
        }

        public bool ForceCalculate
        {
            get
            {
                return forceCalculate;
            }
            set
            {
                forceCalculate = value;
            }
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
            string deadlockFilename = DeadlockUtils.GetDeadlockFilename(level, deadlocksDirectory);
            if (!forceCalculate && File.Exists(deadlockFilename))
            {
                Log.DebugPrint("level {0} already in cache as {1}", index + 1, deadlockFilename);
                ShowCachedDeadlocks(deadlockFilename, level);
                return;
            }
            Log.DebugPrint("calculating deadlocks for level {0}...", index + 1);
            TimeSnapshot start = TimeSnapshot.Now;
            DeadlockFinder deadlockFinder = DeadlockFinder.CreateInstance(level, true);
            deadlockFinder.FindDeadlocks();
            TimeSnapshot end = TimeSnapshot.Now;
            DeadlockUtils.ShowDeadlocks(Console.Out, false, level, deadlockFinder.Deadlocks);
            Log.DebugPrint("calculating deadlocks took {0} seconds", (end.RealTime - start.RealTime).TotalSeconds);
#if false
            Log.DebugPrint("calculating deadlocks took {0} total CPU seconds", (end.TotalTime - start.TotalTime).TotalSeconds);
            Log.DebugPrint("calculating deadlocks took {0} cycles", (end.PerformanceCounter - start.PerformanceCounter));
            Console.ReadKey();
#endif
            DeadlockUtils.SaveDeadlocks(deadlockFinder, deadlocksDirectory);
            Log.DebugPrint("saved deadlocks for level {0} to {1}", index + 1, deadlockFilename);
        }

        private void ShowCachedDeadlocks(string deadlockFilename, Level level)
        {
            DeadlockFinder deadlockFinder = DeadlockUtils.LoadDeadlocks(level, deadlocksDirectory);
            DeadlockUtils.ShowDeadlocks(Console.Out, false, level, deadlockFinder.Deadlocks);
        }
    }
}
