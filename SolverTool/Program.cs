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
using System.Text.RegularExpressions;
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
    class Program
    {
        static void Main(string[] args)
        {
            Log.LogToConsole = true;

            DateTime start = DateTime.Now;
            Tool tool = new Tool();
            int levelIndex = -1;

            // Parse options.
            int i = 0;
            while (i < args.Length)
            {
                string arg = args[i];
                i++;
                string stringValue = i < args.Length ? args[i] : "";
                int intValue = AsInteger(stringValue);
                bool boolValue = intValue != 0;

                if (arg == "--solver-algorithm")
                {
                    tool.SolverAlgorithm = (SolverAlgorithm)Enum.Parse(typeof(SolverAlgorithm), stringValue);
                    i++;
                    continue;
                }

                if (arg == "--reuse-solver")
                {
                    tool.ReuseSolver = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--repetitions")
                {
                    tool.Repetitions = intValue;
                    i++;
                    continue;
                }

                if (arg == "--deadlocks-directory")
                {
                    tool.DeadlocksDirectory = stringValue;
                    i++;
                    continue;
                }

                if (arg == "--collect-solutions")
                {
                    tool.CollectSolutions = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--calculate-deadlocks")
                {
                    tool.CalculateDeadlocks = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--hard-coded-deadlocks")
                {
                    tool.HardCodedDeadlocks = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--level")
                {
                    tool.Level = new Level(LevelEncoder.DecodeLevel(stringValue));
                    i++;
                    continue;
                }

                if (arg == "--maximum-nodes")
                {
                    tool.MaximumNodes = intValue;
                    i++;
                    continue;
                }

                if (arg == "--initial-capacity")
                {
                    tool.InitialCapacity = intValue;
                    i++;
                    continue;
                }

                if (arg == "--optimize-moves")
                {
                    tool.OptimizeMoves = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--optimize-pushes")
                {
                    tool.OptimizePushes = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--detect-no-influence-pushes")
                {
                    tool.DetectNoInfluencePushes = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--validate")
                {
                    tool.Validate = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--verbose")
                {
                    tool.Verbose = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--level-number")
                {
                    levelIndex = intValue - 1;
                    i++;
                    continue;
                }

                if (arg[0] == '-')
                {
                    Console.WriteLine("unsupported argument: {0}", arg);
                    Environment.Exit(1);
                }

                i--;
                break;
            }

            tool.Initialize();

            if (i == args.Length && tool.Level == null)
            {
                Console.WriteLine("nothing to solve");
                Environment.Exit(1);
            }

            if (tool.Level != null)
            {
                tool.Solve();
            }

            while (i < args.Length)
            {
                string arg = args[i++];

                List<string> fileList = new List<string>();
                try
                {
                    string dir = Path.GetDirectoryName(arg);
                    if (String.IsNullOrEmpty(dir))
                    {
                        dir = Directory.GetCurrentDirectory();
                    }
                    fileList.AddRange(Directory.GetFiles(Path.GetFullPath(dir), Path.GetFileName(arg)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception globbing {0}: {1}", arg, ex.Message);
                    Environment.Exit(1);
                }
                foreach (string file in fileList)
                {
                    try
                    {
                        if (levelIndex != -1)
                        {
                            tool.ProcessLevel(file, levelIndex);
                        }
                        else
                        {
                            tool.ProcessLevelSet(file);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception processing file {0}: {1}", arg, ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        Environment.Exit(1);
                    }
                }
            }
        }

        private static int AsInteger(string s)
        {
            if (Regex.IsMatch(s, "^-?[0-9]+$"))
            {
                return Int32.Parse(s);
            }
            else
            {
                return 0;
            }
        }
    }
}
