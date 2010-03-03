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
using System.Threading;

using Sokoban.Engine.Collections;
using Sokoban.Engine.Core;
using Sokoban.Engine.Levels;
using Sokoban.Engine.Paths;
using Sokoban.Engine.Solvers;
using Sokoban.Engine.Utilities;

namespace Sokoban.LevelGenerator
{
    public class Generator
    {
        [Flags]
        public enum AlgorithmType
        {
            Unspecified = 0,
            Buckshot = 1,
            Blob = 2,
            All = Buckshot | Blob,
        }

        private class DesignInfo
        {
            public Array2D<Cell> Data;
            public Array2D<bool> Fixed;
            public Coordinate2D SokobanCoordinate;
            public int Boxes;
        }

        private class Logger
        {
            private bool multiThreaded;
            private StringBuilder text = new StringBuilder();

            public Logger(bool multiThreaded)
            {
                this.multiThreaded = multiThreaded;
            }

            public void DebugPrint(string format, params object[] args)
            {
                if (!multiThreaded)
                {
                    Log.DebugPrint(format, args);
                    return;
                }

                text.Append(string.Format(format, args));
                text.AppendLine();
            }

            public void Flush()
            {
                if (!multiThreaded || text.Length == 0)
                {
                    return;
                }

                Log.Write(text.ToString());
            }
        }

        private class State
        {
            public bool MultiThreaded;
            public int Index;
            public int Seed;
            public Random Random;
            public ISolver Solver;
            public Logger Log;
            public int CurrentDesign;
            public int CurrentSize;
            public AlgorithmType CurrentAlgorithm;
        }

        public class LevelInfo
        {
            public Level Level;
            public int Moves;
            public int Pushes;
            public int Changes;
            public int InsideSquares;
            public int MinimumBoxMoves;
            public MoveList MoveList;
        }

        private int threads;
        private int verbose;
        private int seed;
        private int generationCount;
        private int minSize;
        private int maxSize;
        private int boxes;
        private int displaced;
        private int distance;
        private int islands;
        private int growth;
        private int density;
        private bool clearDesign;
        private bool rejectSokobanOnTarget;
        private bool useEntireLevel;
        private bool reuseSolver;
        private bool moveSokoban;
        private AlgorithmType algorithm;
        private AlgorithmType[] algorithms;
        private int moveLimit;
        private int changeLimit;
        private int pushLimit;
        private int boxMoveLimit;
        private List<Level> designs;
        private List<LevelInfo> results;
        private TransformationInvariantLevelSet set;
        private Random topRandom;
        private bool rejectDeadEnds;
        private bool rejectCapturedTargets;
        private int nodes;
        private bool calculateDeadlocks;
        private string outputFile;
        private Semaphore semaphore;
        private int designCount;
        private DesignInfo[] designInfo;
        private Queue<ISolver> solverQueue;

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

        public int Threads
        {
            get
            {
                return threads;
            }
            set
            {
                threads = value;
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

        public bool MoveSokoban
        {
            get
            {
                return moveSokoban;
            }
            set
            {
                moveSokoban = value;
            }
        }

        public bool ClearDesign
        {
            get
            {
                return clearDesign;
            }
            set
            {
                clearDesign = value;
            }
        }

        public bool UseEntireLevel
        {
            get
            {
                return useEntireLevel;
            }
            set
            {
                useEntireLevel = value;
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

        public AlgorithmType Algorithm
        {
            get
            {
                return algorithm;
            }
            set
            {
                algorithm = value;
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

        public bool CalculateDeadlocks
        {
            get
            {
                return calculateDeadlocks;
            }
            set
            {
                calculateDeadlocks = value;
            }
        }

        public int Seed
        {
            get
            {
                return seed;
            }
            set
            {
                seed = value;
            }
        }

        public int Growth
        {
            get
            {
                return growth;
            }
            set
            {
                growth = value;
            }
        }

        public int Density
        {
            get
            {
                return density;
            }
            set
            {
                density = value;
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

        public int Count
        {
            get
            {
                return generationCount;
            }
            set
            {
                generationCount = value;
            }
        }

        public int Boxes
        {
            get
            {
                return boxes;
            }
            set
            {
                boxes = value;
            }
        }

        public int Displaced
        {
            get
            {
                return displaced;
            }
            set
            {
                displaced = value;
            }
        }

        public int Distance
        {
            get
            {
                return distance;
            }
            set
            {
                distance = value;
            }
        }

        public int Islands
        {
            get
            {
                return islands;
            }
            set
            {
                islands = value;
            }
        }

        public int MinimumSize
        {
            get
            {
                return minSize;
            }
            set
            {
                minSize = value;
            }
        }

        public int MaximumSize
        {
            get
            {
                return maxSize;
            }
            set
            {
                maxSize = value;
            }
        }

        public List<Level> Designs
        {
            get
            {
                return designs;
            }
            set
            {
                designs = value;
            }
        }

        public List<LevelInfo> Results
        {
            get
            {
                return results;
            }
        }

        public Generator()
        {
            seed = -1;
            generationCount = 0;
            boxes = 0;
            minSize = 20;
            maxSize = 40;
            displaced = 1;
            distance = 1;
            growth = 7;
            moveLimit = 40;
            pushLimit = 20;
            changeLimit = 10;
            boxMoveLimit = 1;
            density = 25;
            algorithm = AlgorithmType.All;
            nodes = 1000000;
            calculateDeadlocks = false;
            verbose = 0;
            threads = 0;
            reuseSolver = true;
            rejectDeadEnds = true;
            rejectCapturedTargets = false;
            rejectSokobanOnTarget = false;
            moveSokoban = true;
            outputFile = "LevelGenerator.xsb";
            solverQueue = new Queue<ISolver>();
        }

        private void Initialize()
        {
            // Initialize the top random generator.
            if (seed == -1)
            {
                seed = new Random().Next();
            }
            topRandom = new Random(seed);

            // Initialize number of threads.
            if (threads == 0)
            {
                threads = Environment.ProcessorCount;
            }

            // Initialize results.
            results = new List<LevelInfo>();

            // Create the level set.
            set = new TransformationInvariantLevelSet();

            // Don't include the sokoban in the set.
            set.IncludeSokoban = false;

            // Create the algorithms array.
            List<AlgorithmType> algorithmList = new List<AlgorithmType>();
            foreach (AlgorithmType type in new AlgorithmType[]{ AlgorithmType.Blob, AlgorithmType.Buckshot })
            {
                if ((algorithm & type) != 0)
                {
                    algorithmList.Add(type);
                }
            }
            algorithms = algorithmList.ToArray();

            // Clear the output file.
            State state = new State();
            state.Log = new Logger(false);
            SaveResults(state);
            state.Log.Flush();
        }

        private MoveList Solve(State state, Level level, bool optimizeMoves, bool optimizePushes)
        {
            ISolver solver = state.Solver;
            solver.OptimizeMoves = optimizeMoves;
            solver.OptimizePushes = optimizePushes;
            solver.MaximumNodes = nodes;
            solver.CalculateDeadlocks = calculateDeadlocks;
            solver.Level = level;

            try
            {
                if (solver.Solve())
                {
                    return solver.Solution;
                }
            }
            catch (Exception ex)
            {
                state.Log.DebugPrint("Solver/{0}/{1} exception: {2}", optimizeMoves, optimizePushes, ex.Message);
            }

            return null;
        }

        private ISolver GetSolver()
        {
            if (!reuseSolver)
            {
                return CreateSolver();
            }

            lock (solverQueue)
            {
                if (solverQueue.Count == 0)
                {
                    return CreateSolver();
                }

                return solverQueue.Dequeue();
            }
        }

        private void ReleaseSolver(ISolver solver)
        {
            if (!reuseSolver)
            {
                return;
            }

            lock (solverQueue)
            {
                solverQueue.Enqueue(solver);
            }
        }

        private ISolver CreateSolver()
        {
            return Solver.CreateInstance(SolverAlgorithm.BruteForce);
        }

        private MoveList FastSolve(State state, Level level)
        {
            return Solve(state, level, false, true);
        }

        private MoveList FinalSolve(State state, Level level)
        {
            return Solve(state, level, true, true);
        }

        public bool Generate()
        {
            Initialize();

            designInfo = new DesignInfo[designs.Count];

            designCount = 0;
            foreach (Level design in designs)
            {
                // Extract design data.
                Array2D<Cell> designData = new Array2D<Cell>(design.Data);
                if (useEntireLevel)
                {
                    designData.Replace(Cell.Outside, Cell.Wall);
                }

                // Create a map of all squares that cannot be altered, initialized to false.
                Array2D<bool> designFixed = new Array2D<bool>(designData.Height, designData.Width);
                Coordinate2D sokobanCoord = Coordinate2D.Undefined;
                int designBoxes = 0;
                foreach (Coordinate2D coord in designData.NonPerimeterCoordinates)
                {
                    if (clearDesign)
                    {
                        if (designData[coord] != Cell.Wall)
                        {
                            designData[coord] = Cell.Empty;
                        }
                    }
                    if (designData[coord] != Cell.Undefined)
                    {
                        // Record squares that should not be changed.
                        designFixed[coord] = true;
                    }
                    if (Level.IsSokoban(designData[coord]))
                    {
                        // Remove sokoban if present but record its coordinate.
                        designData[coord] &= ~Cell.Sokoban;
                        sokobanCoord = coord;
                    }
                    if (Level.IsBox(designData[coord]))
                    {
                        // Count boxes included in design.
                        designBoxes++;
                    }
                }

                // Store in design array.
                DesignInfo info = new DesignInfo();
                info.Data = designData;
                info.Fixed = designFixed;
                info.SokobanCoordinate = sokobanCoord;
                info.Boxes = designBoxes;
                designInfo[designCount] = info;
                designCount++;
            }

            // Now generate levels.
            if (threads == 1)
            {
                return GenerateLevels();
            }
            return MultiThreadedGenerateLevels();
        }

        private bool GenerateLevels()
        {
            // Prepare for single-threaded generation.
            int currentSeed = seed;

            // Loop until we have generated the desired number of levels.
            for (int index = 0; generationCount == 0 || index < generationCount; index++)
            {
                State state = new State();
                state.MultiThreaded = false;
                state.Index = index;
                state.Seed = currentSeed;
                GenerateOneLevel(state);
                currentSeed = topRandom.Next();
            }

            // Return success if we found any levels
            // that matched the specified criteria.
            return results.Count > 0;
        }

        private bool MultiThreadedGenerateLevels()
        {
            // Prepare for multi-threaded generation.
            int currentSeed = seed;
            semaphore = new Semaphore(threads, threads);
            WaitCallback callback = new WaitCallback(ThreadGenerateOneLevel);

            // Loop until we have generated the desired number of levels.
            for (int index = 0; generationCount == 0 || index < generationCount; index++)
            {
                // Wait for a processor to become available.
                semaphore.WaitOne();

                // The entire output of the generation process
                // depends solely on the random seed value.
                State state = new State();
                state.MultiThreaded = true;
                state.Index = index;
                state.Seed = currentSeed;

                // Queue the work item to a thread.
                ThreadPool.QueueUserWorkItem(callback, state);

                // Advance the top level random number generator.
                currentSeed = topRandom.Next();
            }

            //  Allow all threads to finish.
            for (int i = 0; i < threads; i++)
            {
                semaphore.WaitOne();
            }

            // Return success if we found any levels
            // that matched the specified criteria.
            return results.Count > 0;
        }

        private void ThreadGenerateOneLevel(object state)
        {
            GenerateOneLevel(state as State);
            semaphore.Release();
        }

        private void GenerateOneLevel(State state)
        {
            // Record the current seed to facilitate duplicating results.
            state.Random = new Random(state.Seed);
            state.Solver = GetSolver();
            state.Log = new Logger(state.MultiThreaded);

            try
            {
                UnsafeGenerateOneLevel(state);
            }
            finally
            {
                ReleaseSolver(state.Solver);
                state.Log.Flush();
            }
        }

        private void UnsafeGenerateOneLevel(State state)
        {
            // Randomly select a supplied design.
            state.CurrentDesign = state.Random.Next(designCount);
            Array2D<Cell> designData = designInfo[state.CurrentDesign].Data;
            Array2D<bool> designFixed = designInfo[state.CurrentDesign].Fixed;

            // Choose a size at random between the minimum and maximum limits.
            int size = minSize + state.Random.Next(maxSize - minSize + 1);
            state.CurrentSize = size;

            int designRow;
            int designColumn;
            Level level = null;
            if (useEntireLevel)
            {
                designRow = 0;
                designColumn = 0;
                level = new Level(new Array2D<Cell>(designData));
            }
            else
            {
                level = GenerateLevel(state, size, designData, designFixed, out designRow, out designColumn);
            }

            // Make sure will have something to solve.
            if (designInfo[state.CurrentDesign].Boxes == 0 && boxes == 0)
            {
                state.Log.DebugPrint("level generated with no boxes");
                return;
            }

            // Add islands not included in design.
            for (int i = 0; i < islands; i++)
            {
                AddIsland(state, level);
            }

            // Add boxes not included in design.
            if (designInfo[state.CurrentDesign].Boxes < boxes)
            {
                int boxesToAdd = boxes - designInfo[state.CurrentDesign].Boxes;
                int displacedBoxes = boxesToAdd >= displaced ? displaced : boxesToAdd;
                int inplaceBoxes = boxesToAdd - displacedBoxes;
                for (int i = 0; i < displacedBoxes; i++)
                {
                    AddDisplacedBox(state, level);
                }
                for (int i = 0; i < inplaceBoxes; i++)
                {
                    AddInplaceBox(state, level);
                }
            }

            if (verbose >= 2)
            {
                state.Log.DebugPrint("design = {0}, size = {1}, algorithm = {2}", state.CurrentDesign, state.CurrentSize, state.CurrentAlgorithm);
                state.Log.DebugPrint(level.AsText);
            }

            // Create a map of squares that have already been tried
            // as the starting sokoban coordinate, initialized to false.
            Array2D<bool> tried = new Array2D<bool>(level.Height, level.Width);
            Coordinate2D sokobanCoord = designInfo[state.CurrentDesign].SokobanCoordinate;

            if (!sokobanCoord.IsUndefined)
            {
                // Only try specified sokoban coordinate.
                tried.SetAll(true);
                tried[sokobanCoord.Row + designRow, sokobanCoord.Column + designColumn] = false;
            }

            // For all accessible squares.
            PathFinder finder = PathFinder.CreateInstance(level);
            while (true)
            {
                // Find an untried starting square.
                bool foundSquare = false;
                foreach (Coordinate2D coord in level.InsideCoordinates)
                {
                    if (level.IsEmpty(coord) && !tried[coord])
                    {
                        sokobanCoord = coord;
                        foundSquare = true;
                        break;
                    }
                }
                if (!foundSquare)
                {
                    break;
                }

                // Put sokoban on untried empty square.
                level.AddSokoban(sokobanCoord);

                // Find all squares accessible from this one.
                finder.Find();

                // Mark all accessible squares as tried.
                foreach (Coordinate2D coord in finder.AccessibleCoordinates)
                {
                    tried[coord] = true;
                }

                MoveList moveList = FastSolve(state, level);
                if (moveList != null)
                {
                    int pushes = LevelUtils.SolutionPushes(moveList);
                    if (verbose >= 1)
                    {
                        state.Log.DebugPrint("raw: moves = {0}, pushes = {1}", moveList.Count, pushes);
                    }
                    if (pushes > pushLimit)
                    {
                        // First clean up the level which might have a large number
                        // of excess squares that would stress the final solver.
                        Level cleanLevel = LevelUtils.CleanLevel(level, moveList);

                        // Now get a clean solution using move optimization.
                        MoveList cleanMoveList = FinalSolve(state, cleanLevel);

                        // Although this does have a solution, the solver
                        // could fail for other reasons (out of memory, etc).
                        if (cleanMoveList != null)
                        {
                            // Finally check whether there are any squares we can remove.
                            AddSmallestLevel(state, cleanLevel, cleanMoveList);
                        }
                    }
                }
                else
                {
                    if (verbose >= 2)
                    {
                        state.Log.DebugPrint("no solution");
                    }
                }

                level.RemoveSokoban();
            }
        }

        private void AddIsland(State state, Level level)
        {
            Coordinate2D coord = GetRandomJustFloorSquare(state, level);
            if (coord.IsUndefined)
            {
                state.Log.DebugPrint("nowhere to add an island");
                return;
            }
            level[coord] = Cell.Wall;
        }

        private void AddDisplacedBox(State state, Level level)
        {
            if (level.Targets > level.Boxes)
            {
                AddDisplacedBoxNoTarget(state, level);
                return;
            }

            Coordinate2D boxCoord = GetRandomJustFloorSquare(state, level);
            if (boxCoord.IsUndefined)
            {
                state.Log.DebugPrint("no available squares to add squares to");
                return;
            }
            level[boxCoord] = Cell.Box;
            int limit = distance == 0 ? int.MaxValue : distance;
            List<Coordinate2D> coordList = new List<Coordinate2D>();
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                if (!level.IsTarget(coord))
                {
                    int targetDistance = Coordinate2D.GetDiagonalDistance(coord, boxCoord);
                    if (targetDistance <= limit)
                    {
                        coordList.Add(coord);
                    }
                }
            }
            if (coordList.Count == 0)
            {
                state.Log.DebugPrint("no available squares to add squares to");
                return;
            }
            Coordinate2D targetCoord = coordList[state.Random.Next(coordList.Count)];
            level[targetCoord] |= Cell.Target;
        }

        private void AddDisplacedBoxNoTarget(State state, Level level)
        {
            if (distance == 0)
            {
                AddRandomBox(state, level);
                return;
            }

            List<Coordinate2D> coordList = new List<Coordinate2D>();
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                // Skip occupied coordinates.
                if (!level.IsEmpty(coord))
                {
                    continue;
                }

                // Find nearest target.
                int nearestTargetDistance = int.MaxValue;
                foreach (Coordinate2D targetCoord in level.TargetCoordinates)
                {
                    int targetDistance = Coordinate2D.GetDiagonalDistance(coord, targetCoord);
                    nearestTargetDistance = Math.Min(nearestTargetDistance, targetDistance);
                }

                // Check whether the nearest target is near enough.
                if (nearestTargetDistance <= (distance == 0 ? int.MaxValue : distance))
                {
                    coordList.Add(coord);
                }
            }
            if (coordList.Count == 0)
            {
                state.Log.DebugPrint("no available squares to add squares to");
                return;
            }
            Coordinate2D boxCoord = coordList[state.Random.Next(coordList.Count)];
            level[boxCoord] |= Cell.Box;
        }

        private void AddRandomBox(State state, Level level)
        {
            Coordinate2D boxCoord = GetRandomEmptySquare(state, level);
            level[boxCoord] |= Cell.Box;
        }

        private void AddInplaceBox(State state, Level level)
        {
            Coordinate2D coord = GetRandomEmptySquare(state, level);
            if (coord.IsUndefined)
            {
                state.Log.DebugPrint("no available squares to add squares to");
                return;
            }
            level[coord] = Cell.Box | Cell.Target;
        }

        private Coordinate2D GetRandomJustFloorSquareAvoidingPerimeter(State state, Level level)
        {
            List<Coordinate2D> coordList = new List<Coordinate2D>();
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                if (level.IsJustFloor(coord))
                {
                    bool onPerimeter = false;
                    foreach (Coordinate2D neighbor in coord.FourNeighbors)
                    {
                        if (level.IsWall(neighbor))
                        {
                            onPerimeter = true;
                            break;
                        }
                    }
                    if (!onPerimeter)
                    {
                        coordList.Add(coord);
                    }
                }
            }
            if (coordList.Count == 0)
            {
                return Coordinate2D.Undefined;
            }
            return coordList[state.Random.Next(coordList.Count)];
        }

        private Coordinate2D GetRandomJustFloorSquare(State state, Level level)
        {
            List<Coordinate2D> coordList = new List<Coordinate2D>();
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                if (level.IsJustFloor(coord))
                {
                    coordList.Add(coord);
                }
            }
            if (coordList.Count == 0)
            {
                return Coordinate2D.Undefined;
            }
            return coordList[state.Random.Next(coordList.Count)];
        }

        private Coordinate2D GetRandomEmptySquare(State state, Level level)
        {
            List<Coordinate2D> coordList = new List<Coordinate2D>();
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                if (level.IsEmpty(coord))
                {
                    coordList.Add(coord);
                }
            }
            if (coordList.Count == 0)
            {
                return Coordinate2D.Undefined;
            }
            return coordList[state.Random.Next(coordList.Count)];
        }

        private Level GenerateLevel(State state, int size, Array2D<Cell> designData, Array2D<bool> designFixed, out int designRow, out int designColumn)
        {
            Level level = null;
            designRow = 0;
            designColumn = 0;

            if (size == 0)
            {
                designRow = -1;
                designColumn = -1;
                level = new Level(designData.GetSubarray(1, 1, designData.Height - 2, designData.Width - 2));
                if (level.IsClosed)
                {
                    state.Log.DebugPrint("design with no additional squares is not closed");
                }
            }

            // Choose algorithm at random.
            state.CurrentAlgorithm = algorithms[state.Random.Next(algorithms.Length)];

            switch (state.CurrentAlgorithm)
            {
                case AlgorithmType.Buckshot:
                    level = GenerateLevelBuckshot(state, size, designData, designFixed, out designRow, out designColumn);
                    break;

                case AlgorithmType.Blob:
                    level = GenerateLevelBlob(state, size, designData, designFixed, out designRow, out designColumn);
                    break;
            }

            return level;
        }

        private Level GenerateLevelBlob(State state, int size, Array2D<Cell> designData, Array2D<bool> designFixed, out int designRow, out int designColumn)
        {
            // Inflate size for extra cells to be subtracted.
            int blobSize = size * (density + 100) / 100;
            // Create larger work array and corresponding fixed map.
            Array2D<Cell> array = new Array2D<Cell>(2 * size, 2 * size);
            array.SetAll(Cell.Outside);
            Array2D<bool> arrayFixed = new Array2D<bool>(array.Height, array.Width);

            // Adjust design data so that outside or undefined cells are empty.
            Array2D<Cell> designDataCopy = new Array2D<Cell>(designData);
            designDataCopy.Replace(Cell.Outside, Cell.Empty);
            designDataCopy.Replace(Cell.Undefined, Cell.Empty);

            // Copy design into middle of work array.
            designRow = size;
            designColumn = size;
            designDataCopy.CopyTo(array, designRow + 1, designColumn + 1, 1, 1, designData.Height - 2, designData.Width - 2);
            designFixed.CopyTo(arrayFixed, designRow + 1, designColumn + 1, 1, 1, designData.Height - 2, designData.Width - 2);

            // Set intial boundaries.
            int rowMin = array.Height;
            int colMin = array.Width;
            int rowMax = -1;
            int colMax = -1;
            int count = 0;
            foreach (Coordinate2D coord in array.Coordinates)
            {
                if (!Level.IsOutside(array[coord]))
                {
                    rowMin = Math.Min(rowMin, coord.Row);
                    colMin = Math.Min(colMin, coord.Column);
                    rowMax = Math.Max(rowMax, coord.Row);
                    colMax = Math.Max(colMax, coord.Column);
                    count++;
                }
            }

            while (count < blobSize)
            {
                // Choose an edge at random.
                int edge = state.Random.Next(4);
                int row = 0;
                int column = 0;
                int limit = 0;
                int hIncr = 0;
                int vIncr = 0;
                switch (edge)
                {
                    case 0:
                        row = rowMin - 1;
                        column = colMin + state.Random.Next(colMax - colMin + 1);
                        limit = rowMax - rowMin + 1;
                        vIncr = 1;
                        hIncr = 0;
                        break;
                    case 1:
                        row = rowMax + 1;
                        column = colMin + state.Random.Next(colMax - colMin + 1);
                        limit = rowMax - rowMin + 1;
                        vIncr = -1;
                        hIncr = 0;
                        break;
                    case 2:
                        row = rowMin + state.Random.Next(rowMax - rowMin + 1);
                        column = colMin - 1;
                        limit = colMax - colMin + 1;
                        vIncr = 0;
                        hIncr = 1;
                        break;
                    case 3:
                        row = rowMin + state.Random.Next(rowMax - rowMin + 1);
                        column = colMax + 1;
                        limit = colMax - colMin + 1;
                        vIncr = 0;
                        hIncr = -1;
                        break;
                }

                // Search along a line until we hit a empty or fixed cell.
                bool found = false;
                for (int i = 0; i < limit; i++)
                {
                    if (array[row + vIncr, column + hIncr] != Cell.Outside || arrayFixed[row + vIncr, column + hIncr])
                    {
                        if (!arrayFixed[row, column])
                        {
                            found = true;
                        }
                        break;
                    }
                    row += vIncr;
                    column += hIncr;
                }
                
                // If we didn't find anything, try again.
                if (!found)
                {
                    continue;
                }

                // Don't allow the level to grow outside the array.
                if (row < 1 || row >= array.Height - 1 || column < 1 || column >= array.Width - 1)
                {
                    continue;
                }

                // Add the new square and update the boundaries.
                array[row, column] = Cell.Empty;
                rowMin = Math.Min(rowMin, row);
                colMin = Math.Min(colMin, column);
                rowMax = Math.Max(rowMax, row);
                colMax = Math.Max(colMax, column);
                count++;
            }

            int attemptsLeft = 2 * (count - size);
            while (count > size && attemptsLeft > 0)
            {
                // Choose a new square at random.
                int row = rowMin + state.Random.Next(rowMax - rowMin + 1);
                int column = colMin + state.Random.Next(colMax - colMin + 1);
                Coordinate2D coord = new Coordinate2D(row, column);
                if (!array.IsValid(coord))
                {
                    continue;
                }

                // Avoid existing walls and outside areas.
                if (!Level.IsInside(array[coord]))
                {
                    continue;
                }

                // We might get into an infinite loop.
                attemptsLeft--;

                // Avoid fixed cells.
                if (arrayFixed[coord])
                {
                    continue;
                }

                // Avoid cells on the perimeter.
                bool isAdjacent = false;
                foreach (Coordinate2D neighbor in coord.EightNeighbors)
                {
                    if (array[neighbor] == Cell.Outside)
                    {
                        isAdjacent = true;
                        break;
                    }
                }
                if (isAdjacent)
                {
                    continue;
                }

                // Remove the cell.
                array[coord] = Cell.Wall;
                count--;

            }

            // Extract the constructed level.
            Array2D<Cell> subarray = array.GetSubarray(rowMin - 1, colMin - 1, rowMax - rowMin + 3, colMax - colMin + 3);
            subarray.Replace(Cell.Outside, Cell.Wall);
            Level level = new Level(subarray);

            // Adjust design coordinate.
            designRow -= rowMin - 1;
            designColumn -= colMin - 1;

            return level;
        }

        private Level GenerateLevelBuckshot(State state, int size, Array2D<Cell> designData, Array2D<bool> designFixed, out int designRow, out int designColumn)
        {
            // Create larger work array and corresponding fixed map.
            Array2D<Cell> array = new Array2D<Cell>(2 * size + designData.Height, 2 * size + designData.Width);
            array.SetAll(Cell.Wall);
            Array2D<bool> arrayFixed = new Array2D<bool>(array.Height, array.Width);

            // Adjust design data so that undefined cells are walls.
            Array2D<Cell> designDataCopy = new Array2D<Cell>(designData);
            designDataCopy.Replace(Cell.Undefined, Cell.Wall);

            // Copy design into middle of work array.
            designRow = size;
            designColumn = size;
            designDataCopy.CopyTo(array, designRow, designColumn, 0, 0, designData.Height, designData.Width);
            designFixed.CopyTo(arrayFixed, designRow, designColumn, 0, 0, designData.Height, designData.Width);

            // Set intial boundaries.
            int rowMin = array.Height;
            int colMin = array.Width;
            int rowMax = -1;
            int colMax = -1;
            int count = 0;
            foreach (Coordinate2D coord in array.Coordinates)
            {
                if (!Level.IsWall(array[coord]))
                {
                    rowMin = Math.Min(rowMin, coord.Row);
                    colMin = Math.Min(colMin, coord.Column);
                    rowMax = Math.Max(rowMax, coord.Row);
                    colMax = Math.Max(colMax, coord.Column);
                    count++;
                }
            }

            while (count < size)
            {
                // Choose a new square at random.
                int row = rowMin - growth + state.Random.Next(rowMax - rowMin + 1 + 2 * growth);
                int column = colMin - growth + state.Random.Next(colMax - colMin + 1 + 2 * growth);
                Coordinate2D coord = new Coordinate2D(row, column);
                if (!array.IsValid(coord))
                {
                    continue;
                }

                // Avoid fixed cells.
                if (arrayFixed[coord])
                {
                    continue;
                }

                // Avoid existing squares.
                if (!Level.IsWall(array[coord]))
                {
                    continue;
                }

                // Ensure the new square is adjacent to an existing square.
                bool isAdjacent = false;
                foreach (Coordinate2D neighbor in coord.FourNeighbors)
                {
                    if (!Level.IsWall(array[neighbor]))
                    {
                        isAdjacent = true;
                        break;
                    }
                }
                if (!isAdjacent)
                {
                    continue;
                }

                // Add the new square and update the boundaries.
                array[coord] = Cell.Empty;
                rowMin = Math.Min(rowMin, row);
                colMin = Math.Min(colMin, column);
                rowMax = Math.Max(rowMax, row);
                colMax = Math.Max(colMax, column);
                count++;
            }

            // Extract the constructed level.
            Array2D<Cell> subarray = array.GetSubarray(rowMin - 1, colMin - 1, rowMax - rowMin + 3, colMax - colMin + 3);
            Level level = new Level(subarray);

            // Adjust design coordinate.
            designRow -= rowMin - 1;
            designColumn -= colMin - 1;

            return level;
        }

        private void AddSmallestLevel(State state, Level level, MoveList moveList)
        {
            // Clean it up.
            Level finalLevel = LevelUtils.NormalizeLevel(LevelUtils.CleanLevel(level, moveList));
            if (verbose >= 2)
            {
                state.Log.DebugPrint("Prior to square removal:");
                state.Log.DebugPrint(finalLevel.AsText);
            }

            // Check for duplicates including possible chiral pair.
            Level chiralLevel = LevelUtils.NormalizeLevel(LevelUtils.GetMirroredLevel(finalLevel));
            if (set.Contains(finalLevel) || set.Contains(chiralLevel))
            {
                Reject(state, level, "duplicate level");
                return;
            }
            set.Add(finalLevel, state.Index);
            set.Add(chiralLevel, state.Index);

            // Re-solve with new level.
            MoveList finalMoveList = FinalSolve(state, finalLevel);

            // Fail if the solver failed.
            if (finalMoveList == null)
            {
                Reject(state, level, "final solver failed");
                return;
            }

            // We can check moves and changes now that we've used the optimizing solver.
            if (finalMoveList.Count < moveLimit)
            {
                Reject(state, level, "less than move limit");
                return;
            }
            if (LevelUtils.SolutionChanges(finalLevel, finalMoveList) < changeLimit)
            {
                Reject(state, level, "less than change limit");
                return;
            }

            // Ensure that all boxes must move enough.
            if (LevelUtils.MinimumBoxMoves(finalLevel, finalMoveList) < boxMoveLimit)
            {
                Reject(state, level, "less than box move limit");
                return;
            }

            // Check whether any squares can be removed.
            CheckRemove(state, finalLevel, finalMoveList);
        }

        private void CheckRemove(State state, Level level, MoveList moveList)
        {
            int pushes = LevelUtils.SolutionPushes(moveList);

            bool removedASquare = false;
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                if (level.IsJustFloor(coord))
                {
                    // Remove the square.
                    Level subLevel = new Level(level);
                    subLevel[coord] = Cell.Wall;

                    // Ensure that this does not corrupt the level.
                    if (!LevelUtils.AllBoxesAndTargetsAreAccessible(level))
                    {
                        continue;
                    }

                    // See if the level is still soluble.
                    MoveList subMoveList = FastSolve(state, subLevel);
                    if (subMoveList != null)
                    {
                        removedASquare = true;

                        if (verbose >= 2)
                        {
                            state.Log.DebugPrint("Removing square {0}", coord);
                        }
                        if (verbose >= 3)
                        {
                            state.Log.DebugPrint("Checking for more square removal:");
                            state.Log.DebugPrint(subLevel.AsText);
                        }

                        // Recursively check for more squares than can be removed.
                        AddSmallestLevel(state, subLevel, subMoveList);
                    }
                }
            }

            // At least one smaller level was soluble with the same boxes and targets.
            if (!removedASquare)
            {
                AddLevel(state, level, moveList);
            }
        }

        private void Reject(State state, Level level, string message)
        {
            if (verbose >= 2)
            {
                state.Log.DebugPrint("rejecting {0}:", message);
                state.Log.DebugPrint(level.AsText);
                state.Log.DebugPrint("");
            }
            else if (verbose >= 1)
            {
                state.Log.DebugPrint("rejecting {0}", message);
            }
        }

        private void AddLevel(State state, Level level, MoveList moveList)
        {
            // Optionally reject levels with dead-ends.
            if (rejectDeadEnds && LevelUtils.HasDeadEnds(level))
            {
                Reject(state, level, "level with dead ends");
                return;
            }

            // Optionally reject levels with captured targets.
            if (rejectCapturedTargets && LevelUtils.HasCapturedTargets(level))
            {
                Reject(state, level, "level with captured targets");
                return;
            }

            // Optionally move the sokoban away from the first box it pushes.
            MoveList finalMoveList = moveList;
            if (moveSokoban)
            {
                // Record old sokoban coordinate.
                Coordinate2D oldSokobanCoord = level.SokobanCoordinate;

                // Find accessible coordinates.
                PathFinder finder = PathFinder.CreateInstance(level);
                finder.Find();

                if (rejectSokobanOnTarget)
                {
                    // Move sokoban to first accessible non-target square.
                    foreach (Coordinate2D coord in finder.AccessibleCoordinates)
                    {
                        if (!level.IsTarget(coord))
                        {
                            level.MoveSokoban(coord);
                            break;
                        }
                    }
                    if (level.IsTarget(level.SokobanCoordinate))
                    {
                        Reject(state, level, "level with sokoban on target");
                        return;
                    }
                }
                else
                {
                    // Move sokoban to first accessible square.
                    foreach (Coordinate2D coord in finder.AccessibleCoordinates)
                    {
                        level.MoveSokoban(coord);
                        break;
                    }
                }

                // Solve one last time if the sokoban moved.
                if (oldSokobanCoord != level.SokobanCoordinate)
                {
                    finalMoveList = FinalSolve(state, level);
                    if (finalMoveList == null)
                    {
                        Reject(state, level, "final solver failed");
                        return;
                    }
                }
            }

            int moves = finalMoveList.Count;
            int pushes = LevelUtils.SolutionPushes(finalMoveList);
            int changes = LevelUtils.SolutionChanges(level, finalMoveList);
            int minBoxMoves = LevelUtils.MinimumBoxMoves(level, finalMoveList);

            // Add level to results.
            LevelInfo info = new LevelInfo();
            info.Level = level;
            info.Moves = moves;
            info.Pushes = pushes;
            info.Changes = changes;
            info.InsideSquares = level.InsideSquares;
            info.MinimumBoxMoves = minBoxMoves;
            info.MoveList = finalMoveList;

            AddResult(state, info);
        }

        private void AddResult(State state, LevelInfo info)
        {
            lock (this)
            {
                UnsafeAddResult(state, info);
            }
        }

        private void UnsafeAddResult(State state, LevelInfo info)
        {
            // Add the result.
            results.Add(info);

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

            // Save sorted results.
            SaveResults(state);

            // Print out final result.
            Level level = info.Level;
            MoveList finalMoveList = info.MoveList;
            state.Log.DebugPrint("seed = {0}, design = {1}, size = {2}, algorithm = {3}", state.Seed, state.CurrentDesign, state.CurrentSize, state.CurrentAlgorithm);
            state.Log.DebugPrint("height = {0}, width = {1}, inside = {2}", level.Height, level.Width, info.InsideSquares);
            state.Log.DebugPrint("moves = {0}, pushes = {1}, changes = {2}, min = {3}", info.Moves, info.Pushes, info.Changes, info.MinimumBoxMoves);
            state.Log.DebugPrint("solution = {0}", SolutionEncoder.EncodedSolution(finalMoveList));
            state.Log.DebugPrint(level.AsText);
            state.Log.DebugPrint("");

        }

        private void SaveResults(State state)
        {
            try
            {
                using (TextWriter writer = File.CreateText(outputFile))
                {
                    writer.WriteLine("LevelGenerator");
                    writer.WriteLine();
                    writer.WriteLine("Algorithm = {0}", Algorithm);
                    writer.WriteLine("MinimumSize = {0}", MinimumSize);
                    writer.WriteLine("MaximumSize = {0}", MaximumSize);
                    writer.WriteLine("MoveLimit = {0}", MoveLimit);
                    writer.WriteLine("PushLimit = {0}", PushLimit);
                    writer.WriteLine("ChangeLimit = {0}", ChangeLimit);
                    writer.WriteLine("Seed = {0}", Seed);
                    writer.WriteLine("Growth = {0}", Growth);
                    writer.WriteLine("Density = {0}", Density);
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
                state.Log.DebugPrint("Exception saving results: {0}", ex.Message);
            }
        }
    }
}
