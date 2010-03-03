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

namespace Sokoban.LevelSorter
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.LogToConsole = true;

            DateTime start = DateTime.Now;
            Sorter sorter = new Sorter();

            // Parse options.
            int i = 0;
            while (i < args.Length)
            {
                string arg = args[i];
                i++;
                string stringValue = i < args.Length ? args[i] : "";
                int intValue = AsInteger(stringValue);
                bool boolValue = intValue != 0;

                if (arg == "--output-file")
                {
                    sorter.OutputFile = stringValue;
                    i++;
                    continue;
                }

                if (arg == "--design-string")
                {
                    try
                    {
                        sorter.Design = new Level(stringValue);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception parsing design: {0}", ex.Message);
                        Environment.Exit(1);
                    }
                    i++;
                    continue;
                }

                if (arg == "--design-file")
                {
                    try
                    {
                        LevelSet levelSet = new LevelSet(stringValue);
                        sorter.Design = levelSet[0];
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception loading design file: {0}", ex.Message);
                        Environment.Exit(1);
                    }
                    i++;
                    continue;
                }

                if (arg == "--nodes")
                {
                    sorter.Nodes = intValue;
                    i++;
                    continue;
                }

                if (arg == "--verbose")
                {
                    sorter.Verbose = intValue;
                    i++;
                    continue;
                }

                if (arg == "--reject-sokoban-on-target")
                {
                    sorter.RejectSokobanOnTarget = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--reject-dead-ends")
                {
                    sorter.RejectDeadEnds = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--reject-captured-targets")
                {
                    sorter.RejectCapturedTargets = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--move-limit")
                {
                    sorter.MoveLimit = intValue;
                    i++;
                    continue;
                }

                if (arg == "--push-limit")
                {
                    sorter.PushLimit = intValue;
                    i++;
                    continue;
                }

                if (arg == "--change-limit")
                {
                    sorter.ChangeLimit = intValue;
                    i++;
                    continue;
                }

                if (arg == "--box-move-limit")
                {
                    sorter.BoxMoveLimit = intValue;
                    i++;
                    continue;
                }

                if (arg == "--check-first-level-only")
                {
                    sorter.CheckFirstLevelOnly = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--parse-statistics")
                {
                    sorter.ParseStatistics = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--normalize-levels")
                {
                    sorter.NormalizeLevels = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--validate-normalization")
                {
                    sorter.ValidateNormalization = boolValue;
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
                    sorter.AddLevelSet(file);
                }
            }

            sorter.Sort();

            DateTime finish = DateTime.Now;
            Console.WriteLine(String.Format("Sort took {0} seconds.", finish - start));
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
