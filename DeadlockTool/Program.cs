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

namespace Sokoban.DeadlockTool
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

                if (arg == "--deadlock-directory")
                {
                    tool.DeadlocksDirectory = stringValue;
                    i++;
                    continue;
                }

                if (arg == "--level-number")
                {
                    levelIndex = intValue - 1;
                    i++;
                    continue;
                }

                if (arg == "--force-calculate")
                {
                    tool.ForceCalculate = boolValue;
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
                    if (levelIndex != -1)
                    {
                        tool.ProcessLevel(file, levelIndex);
                    }
                    else
                    {
                        tool.ProcessLevelSet(file);
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
