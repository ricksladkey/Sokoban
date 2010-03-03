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

#define USE_INCREMENTAL_HASH_KEY
#undef USE_INCREMENTAL_PATH_FINDER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

using Sokoban.Engine.Collections;
using Sokoban.Engine.Core;
using Sokoban.Engine.Deadlocks;
using Sokoban.Engine.Levels;
using Sokoban.Engine.Paths;
using Sokoban.Engine.Solvers.Reference;
using Sokoban.Engine.Solvers.Value;
using Sokoban.Engine.Utilities;

namespace Sokoban.Engine.Solvers
{
    public abstract class Solver : ISolver
    {
        public static ISolver CreateInstance()
        {
            return CreateInstance(SolverAlgorithm.BruteForce);
        }

        public static ISolver CreateInstance(SolverAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case SolverAlgorithm.BruteForce:
                    return new BruteForceSolver();

                case SolverAlgorithm.LowerBound:
                    return new LowerBoundSolver();
            }
            throw new InvalidOperationException("unknown algorithm");
        }

        protected const int maxParents = 1024;
#if USE_INCREMENTAL_PATH_FINDER
        protected const bool incrementalPathFinder = true;
#else
        protected const bool incrementalPathFinder = false;
#endif

        // Primary inputs exposed as properties.
        protected Level originalLevel;
        protected bool optimizeMoves;
        protected bool optimizePushes;
        protected bool calculateDeadlocks;
        protected bool hardCodedDeadlocks;
        protected bool detectNoInfluencePushes;
        protected bool validate;
        protected bool verbose;
        protected bool collectSolutions;
        protected CancelInfo cancelInfo;
        protected string deadlocksDirectory;

        // Primary outputs exposed as properties.
        protected List<MoveList> solutions;
        protected string info;
        protected string error;

        // For performance measurements.
        protected TimeSnapshot startSnapshot;
        protected TimeSnapshot donePreparingSnapshot;
        protected TimeSnapshot stopSnapshot;

        // Solver state data.
        protected bool allocated;
        protected bool precalculatedLevel;
        protected int boxStart;
        protected int scoreLimit;
        protected int maximumNodes;
        protected int initialCapacity;
        protected int duplicates;
        protected bool foundSolution;
        protected int moveLimit;
        protected int pushLimit;

        // Level state data.
        protected Level level;
        protected Array2D<Cell> data;
        protected Coordinate2D[] boxCoordinates;
        protected int boxes;
        protected Coordinate2D[] targetCoordinates;
        protected int targets;

        // Pre-calculated solver quantities.
        protected Array2D<bool> simpleDeadlockMap;
        protected Array2D<Direction[]> pushMap;

        // Cached values that reflect the evolving level.
        protected CurrentState current;

        // The search tree data structures.
        protected NodeCollection nodes;
        protected Node root;

        // Table of visited positions.
        protected IPositionLookupTable<Node> transpositionTable;

        // Utility classes for finding deadlocks and paths.
        protected DeadlockFinder deadlockFinder;
        protected PathFinder pathFinder;
        protected int finderInaccessible;

        public Solver()
        {
            this.maximumNodes = 10000000;
            this.initialCapacity = 10000000;
            this.optimizeMoves = true;
            this.optimizePushes = true;
            this.info = "Not started";
            this.error = "";
            this.allocated = false;
            this.verbose = false;
            this.detectNoInfluencePushes = true;
            this.calculateDeadlocks = true;
            this.hardCodedDeadlocks = false;
            this.CollectSolutions = true;
            this.solutions = new List<MoveList>();
            this.deadlocksDirectory = null;
            this.cancelInfo = new CancelInfo();
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

        public bool CollectSolutions
        {
            get
            {
                return collectSolutions;
            }
            set
            {
                collectSolutions = value;
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
                if (!precalculatedLevel || calculateDeadlocks != value)
                {
                    precalculatedLevel = false;
                    calculateDeadlocks = value;
                }
            }
        }

        public bool HardCodedDeadlocks
        {
            get
            {
                return hardCodedDeadlocks;
            }
            set
            {
                if (!precalculatedLevel || hardCodedDeadlocks != value)
                {
                    precalculatedLevel = false;
                    hardCodedDeadlocks = value;
                }
            }
        }

        public Level Level
        {
            get
            {
                return originalLevel;
            }
            set
            {
                if (!precalculatedLevel || !FloorPlanAndTargetsMatch(originalLevel, value))
                {
                    precalculatedLevel = false;
                    level = new Level(value);
                }
                originalLevel = value;
            }
        }

        public int MaximumNodes
        {
            get
            {
                return maximumNodes;
            }
            set
            {
                if (allocated && maximumNodes != value)
                {
                    allocated = false;
                }
                maximumNodes = value;
            }
        }

        public int InitialCapacity
        {
            get
            {
                return initialCapacity;
            }
            set
            {
                allocated = false;
                initialCapacity = value;
            }
        }

        public bool OptimizeMoves
        {
            get
            {
                return optimizeMoves;
            }
            set
            {
                if (!precalculatedLevel || optimizeMoves != value)
                {
                    precalculatedLevel = false;
                    optimizeMoves = value;
                }
            }
        }

        public bool OptimizePushes
        {
            get
            {
                return optimizePushes;
            }
            set
            {
                optimizePushes = value;
            }
        }

        public bool DetectNoInfluencePushes
        {
            get
            {
                return detectNoInfluencePushes;
            }
            set
            {
                detectNoInfluencePushes = value;
            }
        }

        public bool Validate
        {
            get
            {
                return validate;
            }
            set
            {
                validate = value;
            }
        }

        public bool Verbose
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

        public CancelInfo CancelInfo
        {
            get
            {
                return cancelInfo;
            }
            set
            {
                cancelInfo = value;
            }
        }

        public MoveList Solution
        {
            get
            {
                return solutions.Count > 0 ? solutions[0] : null;
            }
        }

        public string Error
        {
            get
            {
                return error;
            }
        }

        protected void SetInfo(string text)
        {
            if (verbose)
            {
                Log.LogOut.WriteLine(text);
            }
            cancelInfo.Info = text;
        }

        protected void SetError(string text)
        {
            if (!verbose)
            {
                SetInfo(error);
            }
            error = text;
        }

        public bool Solve()
        {
            PrepareToSolve();

            // Check for cancel.
            if (cancelInfo.Cancel)
            {
                SetError("Solver canceled.");
                return false;
            }

            if (!verbose)
            {
                SetInfo("Verbose information not enabled.");
            }
            error = "";

            int depth = 0;
            while (true)
            {
                if (foundSolution && !collectSolutions)
                {
                    break;
                }
                if (Finished())
                {
                    break;
                }
                int lastNodeCount = nodes.Visited;
                SearchTree(root, ++depth);
                if (cancelInfo.Cancel)
                {
                    SetError("Solver canceled.");
                    break;
                }
                if (nodes.Count >= maximumNodes)
                {
                    SetError("Maximum nodes exceeded.");
                    break;
                }
                if (nodes.Visited == lastNodeCount)
                {
                    if (solutions.Count == 0)
                    {
                        SetError("All positions examined lead to dead ends.");
                    }
                    break;
                }
            }

            stopSnapshot = TimeSnapshot.Now;

            SortSolutions();

            // Populate information property.
            if (verbose)
            {
                SetFinalInfo(depth);
            }

            if (validate)
            {
                SolverValidator validator = new SolverValidator(this, nodes, root, transpositionTable);
                validator.CheckDataStructures();
            }

            return foundSolution;
        }

        protected abstract bool Finished();

        protected virtual void PrepareToSolve()
        {
            // Give some feedback because calculating deadlocks can take some time.
            SetInfo("Preparing to solve level...");

            // Start the clock.
            startSnapshot = TimeSnapshot.Now;

            // Put solver in a sane state in case there are any exceptions.
            solutions.Clear();

            // Validate parameter combinations.
            ValidateParameters();

            // Avoid recalculating as much as possible if the level hasn't changed.
            if (!precalculatedLevel)
            {
                PrecalculateLevel();

                // Check for cancellation during pre-calculation.
                if (cancelInfo.Cancel)
                {
                    return;
                }

                precalculatedLevel = true;
            }

            // Avoid reallocating these big data structures for multiple solver runs.
            if (!allocated)
            {
                nodes = new NodeCollection(initialCapacity, validate);
                transpositionTable = new TranspositionTable(nodes, initialCapacity);
                current.Parents = new Node[maxParents];

                allocated = true;
            }
            else
            {
                nodes.Clear();
                transpositionTable.Clear();
            }

            // However, the positions of the occupants might have changed.
            // This gives a performance boost when calculating deadlocks.
            level.CopyOccupantsFrom(originalLevel);
            CheckLevel();

            // The push map depends on the actual starting box positions.
            // This could be avoided if we didn't use the no-box map.
            CalculatePushMap();

            // Initialize solver variables.
            current.ParentIndex = 0;
            level.Validate = validate;
            transpositionTable.Validate = validate;
            foundSolution = false;
            moveLimit = int.MaxValue;
            pushLimit = int.MaxValue;
            duplicates = 0;
            boxStart = 0;

            // Prime the "current" values.
            current.Initialize(level, pathFinder);

            // Create the root of the search tree.
            root = new Node(nodes, current.SokobanRow, current.SokobanColumn, Direction.None, 0, 0);

            // Record the clock.
            donePreparingSnapshot = TimeSnapshot.Now;
        }

        protected bool FloorPlanAndTargetsMatch(Level a, Level b)
        {
            // If the floor plan and targets match, we don't
            // have to re-precalculate the level.
            Level aEmpty = LevelUtils.GetSubsetLevel(a, false, 0);
            Level bEmpty = LevelUtils.GetSubsetLevel(b, false, 0);
            return aEmpty.Equals(bEmpty);
        }

        protected virtual void PrecalculateLevel()
        {
            data = level.Data;
            boxCoordinates = level.BoxCoordinates;
            boxes = boxCoordinates.Length;
            targetCoordinates = new List<Coordinate2D>(level.TargetCoordinates).ToArray();
            targets = targetCoordinates.Length;
            CreateDeadlockFinder();
            simpleDeadlockMap = deadlockFinder.SimpleDeadlockMap;
            pathFinder = PathFinder.CreateInstance(level, optimizeMoves, incrementalPathFinder && !optimizeMoves);
            finderInaccessible = pathFinder.Inaccessible;
        }

        protected virtual void ValidateParameters()
        {
        }

        protected void CheckLevel()
        {
            // Do some simple checks to validate that the level is appropriate for the solver.
            // Note that we can solve levels with more targets than boxes.  This feature
            // in particular is used when calculating deadlocks using the
            // ForewardsDeadlockFinder.

            if (!level.IsClosed)
            {
                throw new InvalidOperationException("level is not closed");
            }
            if (level.Sokobans != 1)
            {
                throw new InvalidOperationException("not precisely one sokoban");
            }
            if (level.Boxes > level.Targets)
            {
                throw new InvalidOperationException("too many boxes");
            }
            if (level.IsComplete)
            {
                throw new InvalidOperationException("level is already complete");
            }
        }

        protected void CreateDeadlockFinder()
        {
            deadlockFinder = null;
            if (calculateDeadlocks && deadlocksDirectory != null)
            {
                deadlockFinder = DeadlockUtils.LoadDeadlocks(level, deadlocksDirectory);
            }
            if (deadlockFinder == null)
            {
                deadlockFinder = DeadlockFinder.CreateInstance(level, calculateDeadlocks, hardCodedDeadlocks);
                deadlockFinder.CancelInfo = cancelInfo;
                deadlockFinder.FindDeadlocks();
            }
        }

        protected void CalculatePushMap()
        {
            // Precalculate a map of possible directions in which a box
            // at a given coordinate can be pushed.

            pushMap = new Array2D<Direction[]>(level.Height, level.Width);
            Array2D<int> noBoxMap = LevelUtils.GetNoBoxMap(level, simpleDeadlockMap);
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                List<Direction> directions = new List<Direction>();
                foreach (Direction direction in Direction.Directions)
                {
                    Coordinate2D sokobanCoood = coord - direction;
                    Coordinate2D newBoxCoord = coord + direction;

                    if (!level.IsFloor(sokobanCoood))
                    {
                        // The sokoban can never reach to push this box.
                        continue;
                    }

                    if (!level.IsFloor(newBoxCoord))
                    {
                        // The box cannot be pushed in this direction.
                        continue;
                    }

                    if (simpleDeadlockMap[newBoxCoord])
                    {
                        // The level would be dead-locked if the box were pushed here.
                        continue;
                    }

                    if (noBoxMap[newBoxCoord] != 0)
                    {
                        // Pushing this box only leads to an inevitable dead-lock.
                        continue;
                    }

                    directions.Add(direction);
                }
                pushMap[coord] = directions.ToArray();
            }
        }

        protected void SortSolutions()
        {
            if (optimizePushes)
            {
                // Sort solutions on increasing order of pushes, then by length.
                solutions.Sort(delegate(MoveList a, MoveList b)
                    {
                        int aPushes = LevelUtils.SolutionPushes(a);
                        int bPushes = LevelUtils.SolutionPushes(b);
                        int result = aPushes.CompareTo(bPushes);
                        if (result != 0)
                        {
                            return result;
                        }
                        return a.Count.CompareTo(b.Count);
                    });
            }
            else
            {
                // Sort solutions in increasing order of length.
                solutions.Sort(delegate(MoveList a, MoveList b)
                    {
                        return a.Count.CompareTo(b.Count);
                    });
            }
        }

        protected void SetFinalInfo(int depth)
        {
            info = "";
            if (error != "")
            {
                info += error + "\r\n";
            }
            foreach (MoveList s in solutions)
            {
                info += String.Format("solution: {0}/{1}\r\n", s.Count, LevelUtils.SolutionPushes(s));
            }
            info += String.Format("Found {0} solutions searching {1:#,##0} nodes,\r\n    with {2:#,##0} duplicates.\r\n",
                solutions.Count, nodes.Visited, duplicates);
            if (solutions.Count > 0)
            {
                // Collect some statistics about the best solution.
                MoveList solution = solutions[0];
                int moves = solution.Count;
                int pushes = LevelUtils.SolutionPushes(solution);
                int changes = LevelUtils.SolutionChanges(originalLevel, solution);
                int minBoxMoves = LevelUtils.MinimumBoxMoves(originalLevel, solution);
                int minTraversalCount = LevelUtils.MinimumTraversalCount(originalLevel, solution);

                info += String.Format("Solution moves {0}, pushes {1}, changes {2},\r\n", moves, pushes, changes);
                info += String.Format("    min-box-moves {0}, min-traversal-count {1}.\r\n", minBoxMoves, minTraversalCount);
            }
            else
            {
                info += String.Format("Searched to depth {0}.\r\n", depth);
            }
            info += String.Format("Nodes allocated {0:#,##0} (free {1:#,##0}).\r\n", nodes.Allocated, nodes.Free);
            TimeSpan delta = stopSnapshot.RealTime - startSnapshot.RealTime;
            info += String.Format("Elapsed time was {0} seconds.\r\n", delta.TotalSeconds.ToString("G6"));
            delta = stopSnapshot.TotalTime - startSnapshot.TotalTime;
            info += String.Format("Total CPU time was {0} seconds.\r\n", delta.TotalSeconds.ToString("G6"));
            delta = stopSnapshot.SystemTime - startSnapshot.SystemTime;
            info += String.Format("Kernel time was {0} seconds.\r\n", delta.TotalSeconds.ToString("G6"));
            delta = donePreparingSnapshot.RealTime - startSnapshot.RealTime;
            info += String.Format("Elapsed preparation time was {0} seconds.\r\n", delta.TotalSeconds.ToString("G6"));
            delta = stopSnapshot.RealTime - donePreparingSnapshot.RealTime;
            info += String.Format("Elapsed searching time was {0} seconds.\r\n", delta.TotalSeconds.ToString("G6"));
#if false
            if (solutions.Count > 0)
            {
                Array2D<int> traversalMap = LevelUtils.GetTraversalMap(originalLevel, solutions[0]);
                info += String.Format("\r\nTraversal map:\r\n{0}", LevelUtils.MapAsText(level, traversalMap));
            }
#endif
            SetInfo(info);
        }

        protected abstract void SearchTree(Node node, int depth);

        protected virtual bool FindChildren(Node node)
        {
            // Check all boxes for possible movement.
            for (int i = 0; i < boxes; i++)
            {
                // Rotate the order with each search so that we don't
                // always search the same box first.
                int ii = (i + boxStart) % boxes;
                int row = boxCoordinates[ii].Row;
                int column = boxCoordinates[ii].Column;
                Direction[] directions = pushMap[row, column];
                int n = directions.Length;
                for (int j = 0; j < n; j++)
                {
                    Direction direction = directions[j];
                    int v = Direction.GetVertical(direction);
                    int h = Direction.GetHorizontal(direction);

                    // Check that the new box square is not a box.
                    int newBoxRow = row + v;
                    int newBoxColumn = column + h;
                    Cell newBoxCell = data[newBoxRow, newBoxColumn];
                    if (Level.IsBox(newBoxCell))
                    {
                        continue;
                    }

#if DEBUG
                    // The push map should prevent this from happening.
                    if (simpleDeadlockMap[newBoxRow, newBoxColumn])
                    {
                        throw Abort("avoidable push");
                    }
#endif

                    // Check that we can access to push this box.
                    int sokobanRow = row - v;
                    int sokobanColumn = column - h;
                    int distance = pathFinder.GetDistance(sokobanRow, sokobanColumn);
                    if (distance >= finderInaccessible)
                    {
                        continue;
                    }

                    // Handle no influence pushes.
                    int pushes = 1;
                    if (detectNoInfluencePushes)
                    {
                        int oldBoxRow = row;
                        int oldBoxColumn = column;

                        while (!Level.IsTarget(newBoxCell) &&
                            Level.IsWall(data[oldBoxRow + h, oldBoxColumn + v]) &&
                            Level.IsWall(data[oldBoxRow - h, oldBoxColumn - v]) &&
                            ((Level.IsWall(data[newBoxRow + h, newBoxColumn + v]) &&
                            Level.IsWallOrEmpty(data[newBoxRow - h, newBoxColumn - v])) ||
                            (Level.IsWallOrEmpty(data[newBoxRow + h, newBoxColumn + v]) &&
                            Level.IsWall(data[newBoxRow - h, newBoxColumn - v]))))
                        {
                            // Continue down the tunnel.
                            pushes++;

                            oldBoxRow += v;
                            oldBoxColumn += h;
                            newBoxRow += v;
                            newBoxColumn += h;

                            // In some situations the push map cannot predict
                            // whether this might occur.
                            if (simpleDeadlockMap[newBoxRow, newBoxColumn])
                            {
                                goto skipChild;
                            }

                            newBoxCell = data[newBoxRow, newBoxColumn];
                            if (!Level.IsSokobanOrEmpty(newBoxCell))
                            {
                                goto skipChild;
                            }
                        }
                    }

                    // Add this move to the search tree.
                    Node child = new Node(nodes, sokobanRow, sokobanColumn, direction, node.Pushes + pushes, node.Moves + distance + pushes);
                    node.Add(child);
                    AddNode(node, child);

                skipChild:
                    continue;
                }
            }
            boxStart = (boxStart + 1) % boxes;

            return node.HasChildren;
        }

        protected virtual void AddNode(Node node, Node child)
        {
        }

        protected bool IsTerminal(ref MoveState state, Node node)
        {
            // This method performs four tasks:
            // - computes the hash key for the node
            // - determines whether this node is a duplicate of another in the transposition table
            // - determines whether this node is deadlocked
            // - determines which squares are accessible from this node
            // The optimal order of the tasks depends on the situation for maximum efficiency.

            return optimizeMoves ? IsTerminalWithMoves(ref state, node) : IsTerminalWithoutMoves(ref state, node);
        }

        protected bool IsTerminalWithMoves(ref MoveState state, Node node)
        {
            // Update current hash key for node.
#if USE_INCREMENTAL_HASH_KEY
            current.HashKey ^= HashKey.GetSokobanHashKey(current.SokobanRow, current.SokobanColumn);
#else
            current.HashKey = GetSlowHashKey();
#endif
#if DEBUG
            ValidateHashKey(node);
#endif

            // Look up the current hash key in the transposition table.
            Node other;
            if (transpositionTable.TryGetValue(current.HashKey, out other))
            {
                duplicates++;

                // Choose which node to pursue.
                if (optimizePushes)
                {
                    if (Node.IsEmpty(other) || other.Pushes < node.Pushes ||
                        other.Pushes == node.Pushes && other.Moves <= node.Moves)
                    {
                        // The other node is deadlocked or as good as than this one.
                        return true;
                    }
                }
                else
                {
                    if (Node.IsEmpty(other) || other.Moves <= node.Moves)
                    {
                        // The other node is deadlocked or as good as than this one.
                        return true;
                    }
                }

                // This node is better than the other one, which is not deadlocked.
                transpositionTable[current.HashKey] = node;
                node.InTable = true;
                other.InTable = false;
                if (other.Dormant)
                {
                    nodes.Release(other);
                }
                else
                {
                    other.Terminal = true;
                }

                // This is no longer a duplicate and not deadlocked, so run the finder.
#if USE_INCREMENTAL_PATH_FINDER
                state.Find(ref current);
#else
                pathFinder.Find(current.SokobanRow, current.SokobanColumn);
#endif
#if DEBUG
                ValidateNotDeadlocked();
#endif
                return false;
            }

            if (deadlockFinder.IsDeadlocked(current.SokobanRow, current.SokobanColumn))
            {
                transpositionTable.Add(current.HashKey, Node.Empty);
                return true;
            }

            // This is a brand new position, so record it.
            transpositionTable.Add(current.HashKey, node);
            node.InTable = true;

            // This is not a duplicate and not deadlocked, so run the finder.
#if USE_INCREMENTAL_PATH_FINDER
            state.Find(ref current);
#else
            pathFinder.Find(current.SokobanRow, current.SokobanColumn);
#endif
            return false;
        }

        protected bool IsTerminalWithoutMoves(ref MoveState state, Node node)
        {
            // Update current hash key for node.
#if USE_INCREMENTAL_PATH_FINDER
            state.Find(ref current);
#else
            pathFinder.Find(current.SokobanRow, current.SokobanColumn);
#endif
#if USE_INCREMENTAL_HASH_KEY
            Coordinate2D proxySokobanCoord = pathFinder.GetFirstAccessibleCoordinate();
            current.HashKey ^= HashKey.GetSokobanHashKey(proxySokobanCoord.Row, proxySokobanCoord.Column);
#else
            current.HashKey = GetSlowHashKey();
#endif
#if DEBUG
            ValidateHashKey(node);
#endif

            // Look up the current hash key in the transposition table.
            Node other;
            if (transpositionTable.TryGetValue(current.HashKey, out other))
            {
                duplicates++;

                // Choose which node to pursue.
                if (Node.IsEmpty(other) || other.Pushes <= node.Pushes)
                {
                    // The other node is deadlocked or as good as this one.
                    return true;
                }

                // This node is better than the other one, which is not deadlocked.
                transpositionTable[current.HashKey] = node;
                node.InTable = true;
                other.InTable = false;
                if (other.Dormant)
                {
                    nodes.Release(other);
                }
                else
                {
                    other.Terminal = true;
                }
#if DEBUG
                ValidateNotDeadlocked();
#endif
                return false;
            }

            if (deadlockFinder.IsDeadlocked(current.SokobanRow, current.SokobanColumn))
            {
                transpositionTable.Add(current.HashKey, Node.Empty);
                return true;
            }

            transpositionTable.Add(current.HashKey, node);
            node.InTable = true;
            return false;
        }

#if DEBUG

        protected void ValidateHashKey(Node node)
        {
            node.HashKey = current.HashKey;
            if (validate)
            {
                HashKey slowHashKey = GetSlowHashKey();
                if (current.HashKey != slowHashKey)
                {
                    throw Abort("hash key corrupted: incremental does not match full");
                }
            }
        }

        protected void ValidateNotDeadlocked()
        {
            if (validate)
            {
                if (deadlockFinder.IsDeadlocked(current.SokobanRow, current.SokobanColumn))
                {
                    throw Abort("unexpected deadlocked position");
                }
            }
        }

#endif

        protected void RemoveDescendants(Node node)
        {
            // Recursively remove all descendants.
            for (Node child = node.Child; !Node.IsEmpty(child); child = child.Sibling)
            {
                // Remove the child node.
                RemoveDescendants(child);
                node.Remove(node, child);
                RemoveNode(child);
                if (child.Terminal || !child.InTable)
                {
                    nodes.Release(child);
                }
                else
                {
                    child.Dormant = true;
                }
            }
        }

        protected virtual void RemoveNode(Node node)
        {
        }

        protected HashKey GetSlowHashKey()
        {
            // Calculate the hash key without using any state information.
            HashKey hashKey;
            if (optimizeMoves)
            {
                hashKey = HashKey.GetSokobanHashKey(current.SokobanRow, current.SokobanColumn);
            }
            else
            {
                Coordinate2D proxySokobanCoord = pathFinder.GetFirstAccessibleCoordinate();
                hashKey = HashKey.GetSokobanHashKey(proxySokobanCoord.Row, proxySokobanCoord.Column);
            }
            for (int i = 0; i < boxes; i++)
            {
                hashKey ^= HashKey.GetBoxHashKey(boxCoordinates[i].Row, boxCoordinates[i].Column);
            }
            return hashKey;
        }

        protected void CollectSolution(Node leaf)
        {
            foundSolution = true;

            // Don't bother if we're not collection solutions.
            if (!collectSolutions)
            {
                return;
            }

            // Prepare to construct the solution.
            MoveList solution = new MoveList();
            Level tempLevel = new Level(originalLevel);
            tempLevel.Validate = validate;
            PathFinder tempFinder = PathFinder.CreateInstance(tempLevel, true);

            // Iterate over the nodes from the root to the leaf.
            int previousPushes = 0;
#if false
            List<Node> nodeList = FindParents(root, leaf);
#else
            List<Node> nodeList = new List<Node>();
            for (int i = 0; i < current.ParentIndex; i++)
            {
                nodeList.Add(current.Parents[i]);
            }
            nodeList.Add(leaf);
#endif
            foreach (Node node in nodeList)
            {
                // Check whether we need to move the sokoban.
                if (node.Coordinate != tempLevel.SokobanCoordinate)
                {
                    // Move the sokoban to the box.
                    solution.AddRange(tempFinder.FindAndGetPath(node.Coordinate));
                    tempLevel.MoveSokoban(node.Coordinate);
                }

                // Check whether we need to push a box.
                int consecutivePushes = node.Pushes - previousPushes;
                if (consecutivePushes != 0)
                {
                    // Push the box one or more times.
                    OperationDirectionPair push = new OperationDirectionPair(Levels.Operation.Push, node.Direction);
                    for (int j = 0; j < consecutivePushes; j++)
                    {
                        solution.Add(push);
                        tempLevel.Move(push);
                    }
                    previousPushes = node.Pushes;
                }
            }

            // Validate the solution.
            if (!tempLevel.IsComplete)
            {
                throw Abort("constructed move list does not solve level");
            }
            if (optimizeMoves && solution.Count != leaf.Moves)
            {
                throw Abort("constructed move list does not match leaf tree in moves");
            }
            if (LevelUtils.SolutionPushes(solution) != leaf.Pushes)
            {
                throw Abort("constructed move list does not match leaf tree in pushes");
            }

            // Add the solution.
            solutions.Add(solution);

            int moves = solution.Count;
            int pushes = LevelUtils.SolutionPushes(solution);
            moveLimit = Math.Min(moveLimit, moves);
            pushLimit = Math.Min(pushLimit, pushes);

#if DEBUG
            if (verbose)
            {
                Log.DebugPrint("                            found solution: {0}/{1}", moves, pushes);
            }
#endif
        }

#if false
        protected LinkedList<Node> FindParents(Node first, Node last)
        {
            // To save space we don't have a parent field so we used to
            // have to walk the entire tree looking for matching children.
            // Now we can use the parent history.

            if (first == last)
            {
                return new LinkedList<Node>(new Node[] { first });
            }

            foreach (Node child in first.Children)
            {
                LinkedList<Node> nodeList = FindParents(child, last);
                if (nodeList != null)
                {
                    nodeList.AddFirst(first);
                    return nodeList;
                }
            }
            return null;
        }
#endif

        protected Exception Abort(string message)
        {
            string bugMessage = String.Format("bug: {0}", message);
            Log.DebugPrint(bugMessage);
            Level fullLevel = new Level(level);
            if (fullLevel.Sokobans == 0 && fullLevel.IsEmpty(current.SokobanRow, current.SokobanColumn))
            {
                fullLevel.AddSokoban(current.SokobanRow, current.SokobanColumn);
            }
            Log.DebugPrint(fullLevel.AsText);
            return new InvalidOperationException(bugMessage);
        }
    }
}
