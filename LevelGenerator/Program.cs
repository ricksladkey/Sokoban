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
using System.Diagnostics;

using Sokoban.Engine.Collections;
using Sokoban.Engine.Core;
using Sokoban.Engine.Deadlocks;
using Sokoban.Engine.Levels;
using Sokoban.Engine.Paths;
using Sokoban.Engine.Solvers;
using Sokoban.Engine.Utilities;

namespace Sokoban.LevelGenerator
{
    class Program
    {
        // Previously used desing strings.
        public static string DesignString()
        {
            string designString = null;

            // LOMA5 constrained.
            designString =
                "#####|#?-##|##$$#|#-$.#|#?..#|#####" + "," +
                "#####|#?#-#|#-$$#|##$.#|#?..#|#####";

            // LOMA4 constrained.
            designString =
                "###|#.#|#$#|#*#|#$#|#.#|###" + "," +
                "#####|#?.?#|#?$?#|##*##|#?$?#|#?.?#|#####";

            // LOMA7 constrained.
            designString =
                "#####|#?$?#|#$*.#|#?.?#|#####" + "," +
                "######|#??#?#|#??$?#|##$*.#|#??.?#|######";

            // LOMA1 constrained.
            designString =
                "####|#$.#|#$.#|#$.#|####" + "," +
                "#####|#?-?#|##$.#|#-$.#|##$.#|#?-?#|#####" + "," +
                "#####|#?#?#|#-$.#|##$.#|#-$.#|#?#?#|#####";

            // LOMA9 constrained.
            designString =
                "####|#$*#|#*.#|####" + "," +
                "#####|#?#-#|#-$*#|##*.#|#####";

            // LOMA10 constrained.
            designString =
                "#####|#$$?#|#$-.#|#?..#|#####" + "," +
                "######|#?#-?#|#-$$?#|##$-.#|#?-..#|######";

            // LOMA3 constrained.
            designString =
                "###|#.#|#*#|#*#|#$#|###" + "," +
                "#####|#?.?#|#-*-#|##*##|#-$-#|#####" + "," +
                "#####|#?.?#|##*##|#-*-#|##$##|#?-?#|#####";

            // LOMA10 follow-up search.
            designString =
                "########|#?# ##?#|##-$$-?#|##-$-.-#|####..-#|#??--#-#|#??----#|#??--??#|########";

            // LOMA8 constrained.
            designString =
                "#####|#?*?#|#.?$#|#?*?#|#####" + "," +
                "#####|#?*?#|#.#$#|#?*?#|#####";

            return designString;
        }

        static void Main(string[] args)
        {
            Log.LogToConsole = true;
            Log.LogFile = "LevelGenerator.log";

            DateTime start = DateTime.Now;
            string[] loma = {
                "####|#$.#|#$.#|#$.#|####",
                "####|#$.#|#.$#|#$.#|####",
                "###|#.#|#*#|#*#|#$#|###",
                "###|#.#|#$#|#*#|#$#|#.#|###",
                "####|#$$#|#$.#|#..#|####",
                "#####|#?$?#|#.*.#|#?$?#|#####",
                "#####|#?$?#|#$*.#|#?.?#|#####",
                "#####|#?*?#|#.?$#|#?*?#|#####",
                "####|#$*#|#*.#|####",
                "#####|#$$?#|#$-.#|#?..#|#####",
            };
            Generator generator = new Generator();
            Generator.AlgorithmType algorithm = Generator.AlgorithmType.Unspecified;
            List<Level> designs = new List<Level>();

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
                    generator.OutputFile = stringValue;
                    i++;
                    continue;
                }

                if (arg == "--reject-sokoban-on-target")
                {
                    generator.RejectSokobanOnTarget = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--reject-dead-ends")
                {
                    generator.RejectDeadEnds = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--reject-captured-targets")
                {
                    generator.RejectCapturedTargets = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--reuse-solver")
                {
                    generator.ReuseSolver = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--move-sokoban")
                {
                    generator.MoveSokoban = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--threads")
                {
                    generator.Threads = intValue;
                    i++;
                    continue;
                }

                if (arg == "--min-size")
                {
                    generator.MinimumSize = intValue;
                    i++;
                    continue;
                }

                if (arg == "--box-move-limit")
                {
                    generator.BoxMoveLimit = intValue;
                    i++;
                    continue;
                }

                if (arg == "--boxes")
                {
                    generator.Boxes = intValue;
                    i++;
                    continue;
                }

                if (arg == "--displaced")
                {
                    generator.Displaced = intValue;
                    i++;
                    continue;
                }

                if (arg == "--distance")
                {
                    generator.Distance = intValue;
                    i++;
                    continue;
                }

                if (arg == "--islands")
                {
                    generator.Islands = intValue;
                    i++;
                    continue;
                }

                if (arg == "--max-size")
                {
                    generator.MaximumSize = intValue;
                    i++;
                    continue;
                }

                if (arg == "--seed")
                {
                    generator.Seed = intValue;
                    i++;
                    continue;
                }

                if (arg == "--growth")
                {
                    generator.Growth = intValue;
                    i++;
                    continue;
                }

                if (arg == "--density")
                {
                    generator.Density = intValue;
                    i++;
                    continue;
                }

                if (arg == "--move-limit")
                {
                    generator.MoveLimit = intValue;
                    i++;
                    continue;
                }

                if (arg == "--push-limit")
                {
                    generator.PushLimit = intValue;
                    i++;
                    continue;
                }

                if (arg == "--change-limit")
                {
                    generator.ChangeLimit = intValue;
                    i++;
                    continue;
                }

                if (arg == "--count")
                {
                    generator.Count = intValue;
                    i++;
                    continue;
                }

                if (arg == "--design-string")
                {
                    try
                    {
                        foreach (string field in stringValue.Split(','))
                        {
                            Level design = new Level(field);
                            designs.Add(design);
                        }
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
                    LevelSet levelSet = null;
                    try
                    {
                        levelSet = new LevelSet(stringValue);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception loading design file: {0}", ex.Message);
                        Environment.Exit(1);
                    }
                    foreach (Level design in levelSet)
                    {
                        designs.Add(design);
                    }
                    i++;
                    continue;
                }

                if (arg == "--loma-design")
                {
                    int index = intValue;
                    Level design = new Level(loma[index - 1]);
                    designs.Add(design);
                    i++;
                    continue;
                }

                if (arg == "--algorithm")
                {
                    if (stringValue == "none")
                    {
                        algorithm = Generator.AlgorithmType.Unspecified;
                    }
                    else
                    {
                        algorithm |= (Generator.AlgorithmType)Enum.Parse(typeof(Generator.AlgorithmType), stringValue, true);
                    }
                    i++;
                    continue;
                }

                if (arg == "--nodes")
                {
                    generator.Nodes = intValue;
                    i++;
                    continue;
                }

                if (arg == "--calculate-deadlocks")
                {
                    generator.CalculateDeadlocks = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--verbose")
                {
                    generator.Verbose = intValue;
                    i++;
                    continue;
                }

                if (arg == "--clear-design")
                {
                    generator.ClearDesign = boolValue;
                    i++;
                    continue;
                }

                if (arg == "--use-entire-level")
                {
                    generator.UseEntireLevel = boolValue;
                    i++;
                    continue;
                }

                Console.WriteLine("unsupported argument: {0}", arg);
                Environment.Exit(1);
            }
            if (algorithm == Generator.AlgorithmType.Unspecified)
            {
                algorithm = generator.Algorithm;
            }
            if (designs.Count == 0)
            {
                designs.Add(new Level("###|# #|###"));
            }

            generator.Algorithm = algorithm;
            generator.Designs = designs;

            generator.Generate();

            DateTime finish = DateTime.Now;
            Console.WriteLine(String.Format("Generate took {0} seconds.", finish - start));
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
