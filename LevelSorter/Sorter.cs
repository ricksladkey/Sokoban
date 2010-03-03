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

namespace Sokoban.LevelSorter
{
    public class Sorter
    {
        public class LevelInfo
        {
            public Level Level;
            public int Moves;
            public int Pushes;
            public int Changes;
            public int InsideSquares;
            public int MinimumBoxMoves;
        }

        Level design;
        private bool checkFirstLevelOnly;
        private int verbose;
        private bool reuseSolver;
        private int moveLimit;
        private int changeLimit;
        private int pushLimit;
        private int boxMoveLimit;
        private bool rejectSokobanOnTarget;
        private bool rejectDeadEnds;
        private bool rejectCapturedTargets;
        private bool parseStatistics;
        private bool normalizeLevels;
        private bool validateNormalization;
        private int nodes;
        private string outputFile;

        TransformationInvariantLevelSet set;
        List<LevelInfo> results;
        private ISolver persistentSolver;

        public string OutputFile
        {
            get
            {
                return outputFile;
            }
            set
            {
                outputFile = value;
            }
        }

        public int Verbose
        {
            get
            {
                return verbose;
            }
            set
            {
                verbose = value;
            }
        }

        public Level Design
        {
            get
            {
                return design;
            }
            set
            {
                design = value;
            }
        }

        public bool NormalizeLevels
        {
            get
            {
                return normalizeLevels;
            }
            set
            {
                normalizeLevels = value;
            }
        }

        public bool ValidateNormalization
        {
            get
            {
                return validateNormalization;
            }
            set
            {
                validateNormalization = value;
            }
        }

        public bool ParseStatistics
        {
            get
            {
                return parseStatistics;
            }
            set
            {
                parseStatistics = value;
            }
        }

        public bool CheckFirstLevelOnly
        {
            get
            {
                return checkFirstLevelOnly;
            }
            set
            {
                checkFirstLevelOnly = value;
            }
        }

        public bool RejectSokobanOnTarget
        {
            get
            {
                return rejectSokobanOnTarget;
            }
            set
            {
                rejectSokobanOnTarget = value;
            }
        }

        public bool ReuseSolver
        {
            get
            {
                return reuseSolver;
            }
            set
            {
                reuseSolver = value;
            }
        }

        public bool RejectDeadEnds
        {
            get
            {
                return rejectDeadEnds;
            }
            set
            {
                rejectDeadEnds = value;
            }
        }

        public bool RejectCapturedTargets
        {
            get
            {
                return rejectCapturedTargets;
            }
            set
            {
                rejectCapturedTargets = value;
            }
        }

        public int Nodes
        {
            get
            {
                return nodes;
            }
            set
            {
                nodes = value;
            }
        }

        public int MoveLimit
        {
            get
            {
                return moveLimit;
            }
            set
            {
                moveLimit = value;
            }
        }

        public int ChangeLimit
        {
            get
            {
                return changeLimit;
            }
            set
            {
                changeLimit = value;
            }
        }

        public int PushLimit
        {
            get
            {
                return pushLimit;
            }
            set
            {
                pushLimit = value;
            }
        }

        public int BoxMoveLimit
        {
            get
            {
                return boxMoveLimit;
            }
            set
            {
                boxMoveLimit = value;
            }
        }

        public Sorter()
        {
            parseStatistics = true;
            checkFirstLevelOnly = true;
            verbose = 0;
            reuseSolver = true;
            moveLimit = 0;
            changeLimit = 0;
            pushLimit = 0;
            boxMoveLimit = 0;
            rejectSokobanOnTarget = false;
            rejectDeadEnds = false;
            rejectCapturedTargets = false;
            normalizeLevels = true;
            validateNormalization = true;
            nodes = 20000000;
            outputFile = "LevelSorter.xsb";

            persistentSolver = reuseSolver ? Solver.CreateInstance() : null;
            set = new TransformationInvariantLevelSet();
            results = new List<LevelInfo>();

            // Don't include the sokoban in the set.
            set.IncludeSokoban = false;
        }

        public void AddLevelSet(string filename)
        {
            if (verbose >= 1)
            {
                Log.DebugPrint("Adding level set: {0}", filename);
            }

            // Load level set.
            LevelSet levelSet = null;
            try
            {
                levelSet = new LevelSet(filename);
            }
            catch (Exception ex)
            {
                Log.DebugPrint("Exception loading level set: {0}", ex.Message);
                return;
            }

            bool reportedFile = false;
            int solved = 0;
            int added = 0;

            // Process each level.
            int levelNumber = 0;
            foreach (Level level in levelSet)
            {
                // In case we need to know which level caused a problem.
                levelNumber++;

                if (verbose >= 2)
                {
                    Log.DebugPrint("Adding level: {0}", levelNumber);
                }

                // Ignore pathological levels.
                if (!level.IsTraditional)
                {
                    continue;
                }

                // Filter based on design.
                if (design != null && !LevelUtils.MatchesDesign(level, design))
                {
                    if (checkFirstLevelOnly)
                    {
                        if (reportedFile)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Design differed after the first level");
                        }
                        return;
                    }
                    continue;
                }

                // Filter based on level criteria.
                if (rejectDeadEnds && LevelUtils.HasDeadEnds(level))
                {
                    continue;
                }
                if (rejectCapturedTargets && LevelUtils.HasCapturedTargets(level))
                {
                    continue;
                }

                // Report file for first level we use the solver on.
                if (!reportedFile)
                {
                    Log.DebugPrint("Loading {0}...", filename);
                    reportedFile = true;
                }

                int moves = 0;
                int pushes = 0;
                int changes = 0;
                int minBoxMoves = 0;

                Level finalLevel = level;
                bool parsedStatistics = false;
                if (parseStatistics && !String.IsNullOrEmpty(level.Name))
                {
                    // Get stats from the level name string.
                    string stats = level.Name;
                    string[] fields = stats.Split(',');
                    if (fields.Length >= 3)
                    {
                        moves = ParseTrailingInteger("moves", fields[0]);
                        pushes = ParseTrailingInteger("pushes", fields[1]);
                        changes = ParseTrailingInteger("changes", fields[2]);
                        if (fields.Length >= 4)
                        {
                            minBoxMoves = ParseTrailingInteger("min", fields[3]);
                        }
                        if (moves != -1 && pushes != -1 && changes != -1)
                        {
                            parsedStatistics = true;
                        }
                    }
                }

                if (!parsedStatistics)
                {
                    // Solve normally.
                    Console.Write(".");
                    solved++;
                    if (solved % 70 == 0)
                    {
                        Console.WriteLine();
                    }

                    if (normalizeLevels)
                    {
                        finalLevel = LevelUtils.NormalizeLevel(finalLevel);
                    }

                    MoveList finalMoveList = FinalSolve(finalLevel);
                    Validate(level, finalLevel, finalMoveList);

                    if (finalMoveList == null)
                    {
                        continue;
                    }

                    moves = finalMoveList.Count;
                    pushes = LevelUtils.SolutionPushes(finalMoveList);
                    changes = LevelUtils.SolutionChanges(finalLevel, finalMoveList);
                    minBoxMoves = LevelUtils.MinimumBoxMoves(finalLevel, finalMoveList);
                }

                // Filter based on solution criteria.
                if (moves < moveLimit)
                {
                    continue;
                }
                if (changes < changeLimit)
                {
                    continue;
                }
                if (pushes < pushLimit)
                {
                    continue;
                }
                if (minBoxMoves < boxMoveLimit)
                {
                    continue;
                }

                // Check for duplicates including possible chiral pair.
                Level chiralLevel = LevelUtils.NormalizeLevel(LevelUtils.GetMirroredLevel(finalLevel));
                if (set.Contains(finalLevel) || set.Contains(chiralLevel))
                {
                    continue;
                }
                set.Add(finalLevel, levelNumber);
                set.Add(chiralLevel, levelNumber);

                // Add it the results.
                LevelInfo info = new LevelInfo();
                info.Level = finalLevel;
                info.Moves = moves;
                info.Pushes = pushes;
                info.Changes = changes;
                info.InsideSquares = finalLevel.InsideSquares;
                info.MinimumBoxMoves = minBoxMoves;
                results.Add(info);
                added++;
            }
            if (solved > 0)
            {
                Console.WriteLine();
            }
            if (added > 0)
            {
                Console.WriteLine("Levels added: {0}", added);
            }
        }

        private void Validate(Level level, Level finalLevel, MoveList finalMoveList)
        {
            if (!normalizeLevels)
            {
                return;
            }
            if (!validateNormalization)
            {
                return;
            }
            if (level == finalLevel)
            {
                return;
            }
            bool printLevels = false;
            MoveList moveList = FinalSolve(level);
            if (moveList == null)
            {
                return;
            }
            if (finalMoveList == null)
            {
                Console.WriteLine();
                Console.WriteLine("Normalization made level unsolvable");
                printLevels = true;
            }
            else if (LevelUtils.SolutionChanges(level, moveList) != LevelUtils.SolutionChanges(finalLevel, finalMoveList))
            {
                Console.WriteLine();
                Console.WriteLine("Normalization changed the solution");
                printLevels = true;
            }
            if (printLevels)
            {
                Console.WriteLine();
                Console.WriteLine("Before normalization:");
                Console.WriteLine(level.AsText);
                Console.WriteLine("After normalization:");
                Console.WriteLine(finalLevel.AsText);
                Console.WriteLine();
            }
        }

        private int ParseTrailingInteger(string fieldName, string text)
        {
            string[] fields = text.Trim().Split(' ');
            if (fields.Length != 3 || fields[0] != fieldName)
            {
                return -1;
            }
            return Int32.Parse(fields[fields.Length - 1]);
        }

        public void Sort()
        {
            // Sort on changes.
            results.Sort(delegate(LevelInfo a, LevelInfo b)
                {
                    int result = b.Changes.CompareTo(a.Changes);
                    if (result != 0)
                    {
                        return result;
                    }
                    result = b.Pushes.CompareTo(a.Pushes);
                    if (result != 0)
                    {
                        return result;
                    }
                    result = b.Moves.CompareTo(a.Moves);
                    if (result != 0)
                    {
                        return result;
                    }
                    result = b.MinimumBoxMoves.CompareTo(a.MinimumBoxMoves);
                    if (result != 0)
                    {
                        return result;
                    }
                    result = b.Level.GetHashCode().CompareTo(a.Level.GetHashCode());
                    return result;
                });

            // Save results.
            Console.WriteLine("Total levels: {0}", results.Count);
            try
            {
                using (TextWriter writer = File.CreateText(outputFile))
                {
                    writer.WriteLine("LevelSorter");
                    writer.WriteLine();

                    foreach (LevelInfo info in results)
                    {
                        writer.WriteLine("moves = {0}, pushes = {1}, changes = {2}, min = {3}", info.Moves, info.Pushes, info.Changes, info.MinimumBoxMoves);
                        writer.WriteLine(info.Level.AsText);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception saving results: {0}", ex.Message);
            }
        }

        private MoveList Solve(Level level, bool optimizeMoves, bool optimizePushes)
        {
            try
            {
                ISolver solver = reuseSolver ? persistentSolver : Solver.CreateInstance();
                solver.OptimizeMoves = optimizeMoves;
                solver.OptimizePushes = optimizePushes;
                solver.MaximumNodes = nodes;
                solver.Level = level;
                if (solver.Solve())
                {
                    return solver.Solution;
                }
            }
            catch (Exception ex)
            {
                Log.DebugPrint("Solver/{0}/{1} exception: {2}", optimizeMoves, optimizePushes, ex.Message);
            }
            return null;
        }

        private MoveList FastSolve(Level level)
        {
            return Solve(level, false, false);
        }

        private MoveList FinalSolve(Level level)
        {
            return Solve(level, true, true);
        }
    }
}
