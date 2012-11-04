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
    [TestFixture]
    public class SolverTests
    {
        [Test]
        public void SolverInstanceTest()
        {
            ISolver solver = Solver.CreateInstance();
        }

        [Test]
        public void MinicosmosComparisonTest()
        {
            TestUtils.ValidateExpectedMovesResults("Minicosmos", TestData.MinicosmosPushesExpectedMoves, TestData.MinicosmosMovesExpectedMoves);
        }

        [Test]
        public void MinicosmosPushesMovesBruteForceTest()
        {
            LevelSet levelSet = TestUtils.LoadLevelSet(TestData.MinicosmosLevelSetFile);
            List<MoveList> solutions = TestUtils.QuickSolve(levelSet, true, true, false);
            List<int> moves = TestUtils.SolutionMoves(solutions);
            List<int> pushes = TestUtils.SolutionPushes(solutions);
            Log.DebugPrint("moves = {0}", TestUtils.StringList(moves));
            Log.DebugPrint("pushes = {0}", TestUtils.StringList(pushes));
            TestUtils.AreEqual(TestData.MinicosmosPushesExpectedMoves, moves, "level {0}: solution moves incorrect");
            TestUtils.AreEqual(TestData.MinicosmosPushesExpectedPushes, pushes, "level {0}: solution pushes incorrect");
        }

        [Test]
        public void MinicosmosPushesBruteForceTest()
        {
            LevelSet levelSet = TestUtils.LoadLevelSet(TestData.MinicosmosLevelSetFile);
            List<MoveList> solutions = TestUtils.QuickSolve(levelSet, false, true, false);
            List<int> pushes = TestUtils.SolutionPushes(solutions);
            Log.DebugPrint("pushes = {0}", TestUtils.StringList(pushes));
            TestUtils.AreEqual(TestData.MinicosmosPushesExpectedPushes, pushes, "level {0}: solution pushes incorrect");
        }

        [Test]
        public void MinicosmosMovesBruteForceTest()
        {
            LevelSet levelSet = TestUtils.LoadLevelSet(TestData.MinicosmosLevelSetFile);
            List<MoveList> solutions = TestUtils.QuickSolve(levelSet, true, false, false);
            List<int> moves = TestUtils.SolutionMoves(solutions);
            Log.DebugPrint("moves = {0}", TestUtils.StringList(moves));
            TestUtils.AreEqual(TestData.MinicosmosMovesExpectedMoves, moves, "level {0}: solution moves incorrect");
        }

        [Test]
        public void MinicosmosPushesMovesLowerBoundTest()
        {
            LevelSet levelSet = TestUtils.LoadLevelSet(TestData.MinicosmosLevelSetFile);
            List<MoveList> solutions = TestUtils.QuickSolve(levelSet, true, true, true);
            List<int> moves = TestUtils.SolutionMoves(solutions);
            List<int> pushes = TestUtils.SolutionPushes(solutions);
            Log.DebugPrint("moves = {0}", TestUtils.StringList(moves));
            Log.DebugPrint("pushes = {0}", TestUtils.StringList(pushes));
            TestUtils.AreEqual(TestData.MinicosmosPushesExpectedMoves, moves, "level {0}: solution moves incorrect");
            TestUtils.AreEqual(TestData.MinicosmosPushesExpectedPushes, pushes, "level {0}: solution pushes incorrect");
        }

        [Test]
        public void MinicosmosPushesLowerBoundTest()
        {
            LevelSet levelSet = TestUtils.LoadLevelSet(TestData.MinicosmosLevelSetFile);
            List<MoveList> solutions = TestUtils.QuickSolve(levelSet, false, true, true);
            List<int> pushes = TestUtils.SolutionPushes(solutions);
            Log.DebugPrint("pushes = {0}", TestUtils.StringList(pushes));
            TestUtils.AreEqual(TestData.MinicosmosPushesExpectedPushes, pushes, "level {0}: solution pushes incorrect");
        }

        [Test]
        public void MinicosmosMovesLowerBoundTest()
        {
            LevelSet levelSet = TestUtils.LoadLevelSet(TestData.MinicosmosLevelSetFile);
            List<MoveList> solutions = TestUtils.QuickSolve(levelSet, true, false, true);
            List<int> moves = TestUtils.SolutionMoves(solutions);
            Log.DebugPrint("moves = {0}", TestUtils.StringList(moves));
            TestUtils.AreEqual(TestData.MinicosmosMovesExpectedMoves, moves, "level {0}: solution moves incorrect");
        }

        [Test]
        public void Minicosmos7Test()
        {
            Level level = TestUtils.LoadLevelSetLevel(TestData.MinicosmosLevelSetFile, 7);
            MoveList solution = TestUtils.QuickSolve(level, true, true, false);
            Log.DebugPrint("solution: {0}", SolutionEncoder.EncodedSolution(solution));
            TestUtils.AreEqual(67, TestUtils.SolutionMoves(solution), "moves incorrect");
            TestUtils.AreEqual(17, TestUtils.SolutionPushes(solution), "pushes incorrect");
        }

        [Test]
        public void MicrocosmosComparisonTest()
        {
            TestUtils.ValidateExpectedMovesResults("Microcosmos", TestData.MicrocosmosPushesExpectedMoves, TestData.MicrocosmosMovesExpectedMoves);
        }

        [Test]
        public void MicrocosmosPushesMovesBruteForceTest()
        {
            LevelSet levelSet = TestUtils.LoadLevelSet(TestData.MicrocosmosLevelSetFile);
            List<MoveList> solutions = TestUtils.QuickSolve(levelSet, true, true, false);
            List<int> moves = TestUtils.SolutionMoves(solutions);
            List<int> pushes = TestUtils.SolutionPushes(solutions);
            Log.DebugPrint("moves = {0}", TestUtils.StringList(moves));
            Log.DebugPrint("pushes = {0}", TestUtils.StringList(pushes));
            TestUtils.AreEqual(TestData.MicrocosmosPushesExpectedMoves, moves, "level {0}: solution moves incorrect");
            TestUtils.AreEqual(TestData.MicrocosmosPushesExpectedPushes, pushes, "level {0}: solution pushes incorrect");
        }

        [Test]
        public void MicrocosmosPushesBruteForceTest()
        {
            LevelSet levelSet = TestUtils.LoadLevelSet(TestData.MicrocosmosLevelSetFile);
            List<MoveList> solutions = TestUtils.QuickSolve(levelSet, false, true, false);
            List<int> pushes = TestUtils.SolutionPushes(solutions);
            Log.DebugPrint("pushes = {0}", TestUtils.StringList(pushes));
            TestUtils.AreEqual(TestData.MicrocosmosPushesExpectedPushes, pushes, "level {0}: solution pushes incorrect");
        }

        [Test]
        public void MicrocosmosMovesBruteForceTest()
        {
            LevelSet levelSet = TestUtils.LoadLevelSet(TestData.MicrocosmosLevelSetFile);
            List<MoveList> solutions = TestUtils.QuickSolve(levelSet, true, false, false);
            List<int> moves = TestUtils.SolutionMoves(solutions);
            Log.DebugPrint("moves = {0}", TestUtils.StringList(moves));
            TestUtils.AreEqual(TestData.MicrocosmosMovesExpectedMoves, moves, "level {0}: solution moves incorrect");
        }

        [Test]
        public void MicrocosmosPushesMovesLowerBoundTest()
        {
            LevelSet levelSet = TestUtils.LoadLevelSet(TestData.MicrocosmosLevelSetFile);
            List<MoveList> solutions = TestUtils.QuickSolve(levelSet, true, true, true);
            List<int> moves = TestUtils.SolutionMoves(solutions);
            List<int> pushes = TestUtils.SolutionPushes(solutions);
            Log.DebugPrint("moves = {0}", TestUtils.StringList(moves));
            Log.DebugPrint("pushes = {0}", TestUtils.StringList(pushes));
            TestUtils.AreEqual(TestData.MicrocosmosPushesExpectedMoves, moves, "level {0}: solution moves incorrect");
            TestUtils.AreEqual(TestData.MicrocosmosPushesExpectedPushes, pushes, "level {0}: solution pushes incorrect");
        }

        [Test]
        public void MicrocosmosPushesLowerBoundTest()
        {
            LevelSet levelSet = TestUtils.LoadLevelSet(TestData.MicrocosmosLevelSetFile);
            List<MoveList> solutions = TestUtils.QuickSolve(levelSet, false, true, true);
            List<int> pushes = TestUtils.SolutionPushes(solutions);
            Log.DebugPrint("pushes = {0}", TestUtils.StringList(pushes));
            TestUtils.AreEqual(TestData.MicrocosmosPushesExpectedPushes, pushes, "level {0}: solution pushes incorrect");
        }

        [Test]
        public void MicrocosmosMovesLowerBoundTest()
        {
            LevelSet levelSet = TestUtils.LoadLevelSet(TestData.MicrocosmosLevelSetFile);
            List<MoveList> solutions = TestUtils.QuickSolve(levelSet, true, false, true);
            List<int> moves = TestUtils.SolutionMoves(solutions);
            Log.DebugPrint("moves = {0}", TestUtils.StringList(moves));
            TestUtils.AreEqual(TestData.MicrocosmosMovesExpectedMoves, moves, "level {0}: solution moves incorrect");
        }

        [Test]
        public void Nabokosmos1Test()
        {
            Level level = TestUtils.LoadLevelSetLevel(TestData.NabokosmosLevelSetFile, 1);
            MoveList solution = TestUtils.QuickSolve(level, true, true, false);
            TestUtils.AreEqual(70, solution.Count, "moves incorrect");
        }

        [Test]
        public void SolveTwiceTest()
        {
            Level level = TestUtils.LoadLevelSetLevel(TestData.NabokosmosLevelSetFile, 1);
            ISolver solver = Solver.CreateInstance();
            solver.Level = level;
            solver.OptimizeMoves = false;
            solver.OptimizePushes = true;
            solver.Validate = true;
            solver.Solve();
            MoveList solution1 = solver.Solution;
            solver.Level = level;
            solver.Solve();
            MoveList solution2 = solver.Solution;
            Log.DebugPrint("solution1: {0}", solution1);
            Log.DebugPrint("solution2: {0}", solution2);
            TestUtils.AreEqual<OperationDirectionPair>(solution1, solution2, "solving twice with the same solver");
        }

        [Test]
        public void Nabokosmos2PushesMovesBruteForceTest()
        {
            Level level = TestUtils.LoadLevelSetLevel(TestData.NabokosmosLevelSetFile, 2);
            MoveList solution = TestUtils.QuickSolve(level, true, true, false);
            TestUtils.AreEqual(150, TestUtils.SolutionMoves(solution), "level: moves incorrect");
            TestUtils.AreEqual(36, TestUtils.SolutionPushes(solution), "level: pushes incorrect");
        }

        [Test]
        public void Nabokosmos2PushesBruteForceTest()
        {
            Level level = TestUtils.LoadLevelSetLevel(TestData.NabokosmosLevelSetFile, 2);
            MoveList solution = TestUtils.QuickSolve(level, false, true, false);
            TestUtils.AreEqual(36, TestUtils.SolutionPushes(solution), "level: pushes incorrect");
        }

        [Test]
        public void Nabokosmos2MovesBruteForceTest()
        {
            Level level = TestUtils.LoadLevelSetLevel(TestData.NabokosmosLevelSetFile, 2);
            MoveList solution = TestUtils.QuickSolve(level, true, false, false);
            TestUtils.AreEqual(120, TestUtils.SolutionMoves(solution), "level: moves incorrect");
        }

        [Test]
        public void Nabokosmos2PushesMovesLowerBoundTest()
        {
            Level level = TestUtils.LoadLevelSetLevel(TestData.NabokosmosLevelSetFile, 2);
            MoveList solution = TestUtils.QuickSolve(level, true, true, true);
            TestUtils.AreEqual(150, TestUtils.SolutionMoves(solution), "level: moves incorrect");
            TestUtils.AreEqual(36, TestUtils.SolutionPushes(solution), "level: pushes incorrect");
        }

        [Test]
        public void Nabokosmos2PushesLowerBoundTest()
        {
            Level level = TestUtils.LoadLevelSetLevel(TestData.NabokosmosLevelSetFile, 2);
            MoveList solution = TestUtils.QuickSolve(level, false, true, true);
            TestUtils.AreEqual(36, TestUtils.SolutionPushes(solution), "level: pushes incorrect");
        }

        [Test]
        public void Nabokosmos2MovesLowerBoundTest()
        {
            Level level = TestUtils.LoadLevelSetLevel(TestData.NabokosmosLevelSetFile, 2);
            MoveList solution = TestUtils.QuickSolve(level, true, false, true);
            TestUtils.AreEqual(120, TestUtils.SolutionMoves(solution), "level: moves incorrect");
        }
    }
}
