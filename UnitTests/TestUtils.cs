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
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

using Sokoban.Engine.Collections;
using Sokoban.Engine.Core;
using Sokoban.Engine.Deadlocks;
using Sokoban.Engine.Levels;
using Sokoban.Engine.Paths;
using Sokoban.Engine.Solvers;
using Sokoban.Engine.Utilities;

namespace Sokoban.UnitTests
{
    public static class TestUtils
    {
        static TestUtils()
        {
            Log.LogToConsole = true;
        }

        public static bool TestAreEqual<T>(T expected, T actual, IEqualityComparer<T> comparer)
        {
            return comparer.Equals(expected, actual);
        }

        public static bool TestAreEqual<T>(T expected, T actual)
        {
            return TestAreEqual(expected, actual, EqualityComparer<T>.Default);
        }

        public static void AreEqual<T>(T expected, T actual, string message)
        {
            NUnit.Framework.Assert.AreEqual(expected, actual, message);
        }

        public static void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message)
        {
            List<T> expectedList = new List<T>(expected);
            List<T> actualList = new List<T>(actual);
            AreEqual(expectedList.Count, actualList.Count, "lengths are mismatched");
            int n = expectedList.Count;
            List<string> messageList = new List<string>();
            for (int i = 0; i < n; i++)
            {
                if (!TestAreEqual(expectedList[i], actualList[i]))
                {
                    string detail = String.Format(message, i + 1);
                    string fullMessage = String.Format("\r\n{0} (expected {1} != actual {2})",
                        detail, expectedList[i], actualList[i]);
                    messageList.Add(fullMessage);
                }
            }
            if (messageList.Count > 0)
            {
                Assert(false, StringList(messageList));
            }
        }

        public static void Assert(bool value, string message)
        {
            AreEqual(true, value, message);
        }

        public static string StringList<T>(IEnumerable<T> list)
        {
            string result = "";
            string sep = "";
            foreach (T item in list)
            {
                result += sep + item.ToString();
                sep = ", ";
            }
            return result;
        }

        public static int SolutionMoves(MoveList solution)
        {
            if (solution == null)
            {
                return -1;
            }
            return solution.Count;
        }

        public static List<int> SolutionMoves(IEnumerable<MoveList> solutions)
        {
            List<MoveList> list = new List<MoveList>(solutions);
            return list.ConvertAll<int>(delegate(MoveList solution) { return SolutionMoves(solution); });
        }

        public static int SolutionPushes(MoveList solution)
        {
            if (solution == null)
            {
                return -1;
            }
            int pushes = 0;
            foreach (OperationDirectionPair pair in solution)
            {
                if (pair.Operation == Operation.Push)
                {
                    pushes++;
                }
            }
            return pushes;
        }

        public static List<int> SolutionPushes(IEnumerable<MoveList> solutions)
        {
            List<MoveList> list = new List<MoveList>(solutions);
            return list.ConvertAll<int>(delegate(MoveList solution) { return SolutionPushes(solution); });
        }

        public static LevelSet LoadLevelSet(string levelSetFile)
        {
            using (TextReader reader = File.OpenText(TestData.DataDirectory + levelSetFile))
            {
                string levelSetName = levelSetFile;
                levelSetName = Path.GetFileNameWithoutExtension(levelSetFile);
                LevelSet levelSet = new LevelSet(reader);
                for (int i = 0; i < levelSet.Count; i++)
                {
                    levelSet[i].Name = String.Format("{0} Level {1}", levelSetName, i + 1);
                }
                return levelSet;
            }
        }

        public static Level LoadLevelSetLevel(string levelSetFile, int level)
        {
            LevelSet levelSet = LoadLevelSet(levelSetFile);
            return levelSet[level - 1];
        }

        public static MoveList QuickSolve(Level level, bool optimizeMoves, bool optimizePushes, bool useLowerBound)
        {
            ISolver solver = Solver.CreateInstance(useLowerBound ? SolverAlgorithm.BruteForce : SolverAlgorithm.BruteForce);
            Log.DebugPrint("Solving level {0}", level.Name);
            solver.Level = level;
            solver.OptimizeMoves = optimizeMoves;
            solver.OptimizePushes = optimizePushes;
            solver.Verbose = true;
            solver.Validate = true;
            solver.Solve();
            return solver.Solution;
        }

        public static List<MoveList> QuickSolve(IEnumerable<Level> levels, bool optimizeMoves, bool optimizePushes, bool useLowerBound)
        {
            return QuickSolve(levels, optimizeMoves, optimizePushes, useLowerBound, true);
        }

        public static List<MoveList> QuickSolve(IEnumerable<Level> levels, bool optimizeMoves, bool optimizePushes, bool useLowerBound, bool reuseSolver)
        {
            if (reuseSolver)
            {
                List<MoveList> solutions = new List<MoveList>();
                ISolver solver = Solver.CreateInstance(useLowerBound ? SolverAlgorithm.BruteForce : SolverAlgorithm.BruteForce);
                int index = 0;
                foreach (Level level in levels)
                {
                    Log.DebugPrint("Solving level {0}", index + 1);
                    solver.Level = level;
                    solver.OptimizeMoves = optimizeMoves;
                    solver.OptimizePushes = optimizePushes;
                    solver.Verbose = true;
                    solver.Validate = true;
                    DateTime start = DateTime.Now;
                    solver.Solve();
                    TimeSpan elapsed = DateTime.Now - start;
                    solutions.Add(solver.Solution);
                    Log.DebugPrint("solution took {0} seconds", elapsed.TotalSeconds);
                    index++;
                }
                return solutions;
            }
            else
            {
                List<Level> list = new List<Level>(levels);
                return list.ConvertAll<MoveList>(delegate(Level level) { return QuickSolve(level, optimizeMoves, optimizePushes, useLowerBound); });
            }
        }

        public static void ValidateExpectedMovesResults(string label, IEnumerable<int> pushes, IEnumerable<int> moves)
        {
            List<int> pushList = new List<int>(pushes);
            List<int> moveList = new List<int>(moves);
            TestUtils.AreEqual(moveList.Count, pushList.Count, "moves mismatch");
            int n = pushList.Count;
            for (int i = 0; i < n; i++)
            {
                TestUtils.Assert(pushList[i] >= moveList[i], String.Format("level {0}: optimal pushList less than optimal moveList", i + 1));
                if (pushList[i] != moveList[i])
                {
                    Log.DebugPrint("{0} level {1}, optimal pushList {2}, optimal moveList {3}",
                        label, i + 1, pushList[i], moveList[i]);
                }
            }
        }
    }
}
