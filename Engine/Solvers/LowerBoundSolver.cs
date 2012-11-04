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

#if DEBUG
#undef DEBUG_SCORES
#define VALIDATE_LOWER_BOUND_MONOTONIC
#undef VALIDATE_SCORE_COUNTS
#endif

using System;
using System.Collections.Generic;
using System.Text;

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
    public class LowerBoundSolver : Solver
    {
        // Lower bound solver pre-calculated quantities.
        private Array2D<int>[] targetDistanceMaps;
        private Array2D<int[]> nearestTargetsMap;
        private Array2D<int> nearestTargetDistanceMap;
        private Array2D<int> targetIndexMap;

        // Lower bound work arrays.
        private int[] boxTarget;
        private int[] targetBox;
        private int[] distance;

        // Lower bound solver state.
        private int[] scoreCounts;
        private int scoreTotal;
        private int bestLowerBound;
        private Level bestLevel;

        // State variables while finding children.
        private int score;
        private MoveState state;

        public int GetLowerBound()
        {
            PrepareToSolve();

            return GetLowerBound(root);
        }

        protected override bool Finished()
        {
            return false;
        }

        protected override void PrecalculateLevel()
        {
            base.PrecalculateLevel();

            CalculateLowerBoundMaps();
        }

        protected override void PrepareToSolve()
        {
            base.PrepareToSolve();

            if (scoreCounts == null)
            {
                scoreCounts = new int[Node.MaxScore + 1];
            }
            else
            {
                Array.Clear(scoreCounts, 0, scoreCounts.Length);
            }

            scoreTotal = 0;

            // Avoid work when operating quietly.
            if (verbose)
            {
                bestLowerBound = int.MaxValue;
                bestLevel = new Level(originalLevel);
            }
        }

        protected void CalculateLowerBoundMaps()
        {
            // Create the pre-calculated lower bound data structures.

            // Create work arrays for matching lower bound function.
            boxTarget = new int[boxes];
            targetBox = new int[targets];
            distance = new int[boxes];

            // Create an empty level with path finder for measuring distances.
            Level emptyLevel = LevelUtils.GetEmptyLevel(originalLevel);
            PathFinder emptyPathFinder = PathFinder.CreateInstance(emptyLevel, true);

            // Create the target distance maps and target index map.
            targetDistanceMaps = new Array2D<int>[targets];
            targetIndexMap = new Array2D<int>(level.Height, level.Width);
            targetIndexMap.SetAll(-1);
            for (int i = 0; i < targets; i++)
            {
                targetDistanceMaps[i] = new Array2D<int>(level.Height, level.Width);
                targetIndexMap[targetCoordinates[i]] = i;
            }

            // For each inside square calculate the sorted list of nearest targets and
            // the distance to the nearest target.
            nearestTargetsMap = new Array2D<int[]>(level.Height, level.Width);
            nearestTargetDistanceMap = new Array2D<int>(level.Height, level.Width);
            foreach (Coordinate2D boxCoord in level.InsideCoordinates)
            {
                List<int> nearestTargets = new List<int>();
                for (int j = 0; j < targets; j++)
                {
                    Coordinate2D targetCoord = targetCoordinates[j];
                    targetDistanceMaps[j][boxCoord] = emptyPathFinder.FindAndGetDistance(boxCoord, targetCoord);
                    nearestTargets.Add(j);
                }
                nearestTargets.Sort(delegate(int index1, int index2)
                    {
                        int distance1 = targetDistanceMaps[index1][boxCoord];
                        int distance2 = targetDistanceMaps[index2][boxCoord];
                        return distance1.CompareTo(distance2);
                    });
                nearestTargetDistanceMap[boxCoord] = targetDistanceMaps[nearestTargets[0]][boxCoord];
                nearestTargetsMap[boxCoord] = nearestTargets.ToArray();
            }
        }

        protected override void SearchTree(Node node, int depth)
        {
            // If the root hasn't been searched yet, manually expand it.
            if (!node.Searched)
            {
                MoveState state = new MoveState();
                node.Searched = true;
                HashKey hashKey = current.HashKey;
                bool result = IsTerminal(ref state, node) || !FindChildren(node);
                current.HashKey = hashKey;
            }

            if (foundSolution && !optimizeMoves/* && !optimizePushes */)
            {
                return;
            }

            // Check for cancel.
            string info = null;
            int lastNodeCount = nodes.Visited;
            while (!cancelInfo.Cancel)
            {
                if (root.Score == Node.MaxScore)
                {
                    return;
                }

                CalculateScoreLimit();
                if (scoreTotal == 0)
                {
                    break;
                }

                if (verbose)
                {
                    string searching = foundSolution ? "Post-searching" : "Searching";
                    info = String.Format("{0} with root score {1} and score limit {2},\r\n", searching, root.Score, scoreLimit);
                }
                int n = nodes.Visited;
                Search(node);
                if (verbose)
                {
                    info += String.Format("Examined {0:#,##0} new nodes ({1:#,##0} total)...",
                        nodes.Visited - lastNodeCount, nodes.Visited);
                    info += String.Format("\r\nNodes allocated {0:#,##0}\r\n", nodes.Allocated);
                    info += String.Format("Best lower bound: {0}, best level:\r\n{1}", bestLowerBound, bestLevel.AsText);
                    SetInfo(info);
                }
                if (nodes.Visited > n)
                {
                    break;
                }
            }
        }

        private void CalculateScoreLimit()
        {
            if (scoreTotal == 0)
            {
                return;
            }

            if (optimizeMoves || optimizePushes)
            {
                scoreLimit = root.Score;
            }
            else
            {
                // Calculate score limit from score count histogram.
                int subTotal = 0;
                int subTotalLimit = scoreTotal / 10;
                if (subTotalLimit == 0)
                {
                    subTotalLimit = 1;
                }
                for (scoreLimit = 0; scoreLimit < scoreCounts.Length; scoreLimit++)
                {
                    int count = scoreCounts[scoreLimit];
                    subTotal += count;
                    if (subTotal >= subTotalLimit)
                    {
                        break;
                    }
                }
            }

#if DEBUG_SCORES
            Log.DebugPrint("");
            Log.DebugPrint("**************************");
            PrintTree(root);
            for (int i = 0; i < scoreCounts.Length; i++)
            {
                if (scoreCounts[i] != 0)
                {
                    Log.DebugPrint("{0}: {1}", i, scoreCounts[i]);
                }
            }
            Log.DebugPrint("score limit: {0}, score total: {1}", scoreLimit, scoreTotal);
#endif

#if VALIDATE_SCORE_COUNTS
            // This is too expensive for general use.
            int openCount = 0;
            foreach (Node node in nodes)
            {
                if (!node.Searched)
                {
                    openCount++;
                }
            }
            if (openCount != scoreTotal)
            {
                throw Abort("score total incorrect");
            }
#endif
        }

        private void Search(Node node)
        {
            // Check for cancel.
            if (cancelInfo.Cancel)
            {
                return;
            }

            MoveState state = new MoveState();
            state.PrepareToMove(node, ref current);

            int score = Node.MaxScore;
            Node previous = node;
            for (Node child = node.Child; !Node.IsEmpty(child); child = child.Sibling)
            {
                if (child.Terminal)
                {
                    // Another node displaced this node from the transposition table.
                    RemoveDescendants(child);
                    RemoveScore(child);
                    node.Remove(previous, child);
                    nodes.Release(child);
                    continue;
                }

                if (child.Score > scoreLimit)
                {
                    if (child.Score < score)
                    {
                        score = child.Score;
                    }
                    previous = child;
                    continue;
                }

                state.DoMove(child, ref current);

                if (child.Searched)
                {
                    Search(child);
                    if (!child.HasChildren)
                    {
                        node.Remove(previous, child);
                        RemoveScore(child);
                        if (child.InTable)
                        {
                            child.Dormant = true;
                        }
                        else
                        {
                            nodes.Release(child);
                        }
                        goto removed;
                    }
                }
                else
                {
                    // Remove the unsearched score.
#if false
                    int oldScore = child.Score;
#endif
                    RemoveScore(child);

                    // Search the child node.
                    child.Searched = true;

                    if (level.IsComplete)
                    {
                        child.Complete = true;
                        CollectSolution(child);
                        node.Remove(previous, child);
                        child.Dormant = true;
                        RemoveScore(child);
                        goto removed;
                    }

                    if (IsTerminal(ref state, child) || !FindChildren(child))
                    {
                        node.Remove(previous, child);
                        if (child.InTable)
                        {
                            // Node had no children so it is effectively deadlocked.
                            transpositionTable[current.HashKey] = Node.Empty;
                        }
                        RemoveScore(child);
                        nodes.Release(child);
                        goto removed;
                    }

#if false
                    // XXX: This works but doesn't seem to help.
                    if (child.Score == oldScore)
                    {
                        Search(child);
                        if (!child.HasChildren)
                        {
                            node.Remove(nodes, previous, child);
                            RemoveScore(child);
                            if (child.InTable)
                            {
                                child.Dormant = true;
                            }
                            else
                            {
                                child.Release(nodes);
                            }
                            goto removed;
                        }
                    }
#endif
                }

                if (child.Score < score)
                {
                    score = child.Score;
                }

                previous = child;

            removed:

                state.UndoMove(ref current);
            }
            SetScore(node, score);

            state.FinishMoving(ref current);
        }

        protected override bool FindChildren(Node node)
        {
            if (optimizePushes ? node.Score > pushLimit : node.Score > moveLimit)
            {
                // Once we've found a solution, ignore postions garanteed to exceed its length.
                return true;
            }

            state.PrepareToMove(node, ref current);

            score = Node.MaxScore;

            base.FindChildren(node);

            state.FinishMoving(ref current);

            node.Score = score;
            return node.HasChildren;
        }

        protected override void AddNode(Node node, Node child)
        {
            state.DoMove(child, ref current);

            int baseScore = 0;
            if (optimizePushes)
            {
                baseScore = child.Pushes;
            }
            else if (optimizeMoves)
            {
                baseScore = child.Moves;
            }
            SetScore(child, baseScore + GetLowerBound(child));
            AddScore(child);
#if VALIDATE_LOWER_BOUND_MONOTONIC
            if (child.Score < node.Score)
            {
                throw Abort("score not monotonically increasing");
            }
#endif
            if (child.Score < score)
            {
                score = child.Score;
            }

            state.UndoMove(ref current);
        }

        private void SetScore(Node node, int score)
        {
#if VALIDATE_LOWER_BOUND_MONOTONIC
            if (score < node.Score)
            {
                throw Abort("score not monotonically increasing");
            }
#endif
            node.Score = score;
        }

        private void RemoveScore(Node node)
        {
            if (!node.Searched)
            {
                scoreCounts[node.Score]--;
                scoreTotal--;
#if DEBUG_SCORES
                Log.DebugPrint("---- removing score {0} for node {1}", node.Score, node.Index);
#endif
            }
#if DEBUG_SCORES
            else
            {
                Log.DebugPrint("---- not removing score {0} for node {1}", node.Score, node.Index);
            }
#endif
        }

        private void AddScore(Node node)
        {
#if DEBUG_SCORES
            Log.DebugPrint("---- adding score {0} for node {1}", node.Score, node.Index);
#endif
            scoreCounts[node.Score]++;
            scoreTotal++;
        }

        private int GetLowerBound(Node child)
        {
            int lowerBound = GetSimpleLowerBound();

#if false
            int lowerBound = GetSimpleLowerBound();

            int lowerBound = GetTakenLowerBound();

            int lowerBound = GetMatchingLowerBound();

            int lowerBound = 0;
#endif

#if DEBUG
            child.LowerBound = lowerBound;
#endif

            if (verbose && lowerBound < bestLowerBound)
            {
                bestLowerBound = lowerBound;
                bestLevel = new Level(level);
                bestLevel.AddSokoban(current.SokobanRow, current.SokobanColumn);
            }
            return lowerBound;
        }

        private int GetSimpleLowerBound()
        {
            // Calculate the distance to nearest target,
            // disregarding boxes going to the same target.

            // This lower bound is admissible and monotonic.  In other
            // words, it never overestimates the distance to the solution
            // and making a move never decreases the overall estimate
            // of the solution length.

            // A consequence of using this lower bound is that boxes
            // become congested near the first available target
            // since the distance to the nearest target doesn't change.

            int lowerBound = 0;
            for (int i = 0; i < boxes; i++)
            {
                lowerBound += nearestTargetDistanceMap[boxCoordinates[i].Row, boxCoordinates[i].Column];
            }
            return lowerBound;
        }

        private int GetTakenLowerBound()
        {
            // Calculate the distance to nearest target,
            // disregarding boxes going to the same target
            // except for targets already taken by a box.

            // This lower bound is admissible but not monotonic.  In other
            // words, it never overestimates the distance to the solution
            // but it sometimes switches from accurate to underestimating.

            // A consequence of using this lower bound is that the
            // solver avoids filling the very first remote target
            // because doing so increases the distance of all the
            // distant boxes.

            // Determine which targets are already taken.
            UInt64 targetUsed = 0u;
            for (int i = 0; i < boxes; i++)
            {
                int index = targetIndexMap[boxCoordinates[i].Row, boxCoordinates[i].Column];
                if (index >= 0)
                {
                    targetUsed |= (UInt64)1u << index;
                }
            }

            // Look up the nearest non-taken target for each box.
            int lowerBound = 0;
            for (int i = 0; i < boxes; i++)
            {
                int boxRow = boxCoordinates[i].Row;
                int boxColumn = boxCoordinates[i].Column;
                if (!Level.IsTarget(data[boxRow, boxColumn]))
                {
                    int[] nearestTargets = nearestTargetsMap[boxRow, boxColumn];
                    for (int j = 0; j < targets; j++)
                    {
                        int targetIndex = nearestTargets[j];
                        if ((targetUsed & ((UInt64)1u << targetIndex)) == 0)
                        {
                            lowerBound += targetDistanceMaps[targetIndex][boxRow, boxColumn];
                            break;
                        }
                    }
                }
            }

            return lowerBound;
        }

        private int GetMatchingLowerBound()
        {

            // Choose an arbitrary matching.
            for (int i = 0; i < boxes; i++)
            {
                boxTarget[i] = i;
                distance[i] = targetDistanceMaps[i][boxCoordinates[i]];
            }
            for (int i = 0; i < boxes; i++)
            {
                targetBox[i] = i;
            }
            for (int i = boxes; i < targets; i++)
            {
                targetBox[i] = -1;
            }

        retry:

            // Continue swapping boxes with targets until there are no more changes.
            for (int thisBox = 0; thisBox < boxes; thisBox++)
            {
                int thisTarget = boxTarget[thisBox];
                for (int otherTarget = 0; otherTarget < targets; otherTarget++)
                {
                    if (thisTarget == otherTarget)
                    {
                        continue;
                    }
                    int otherBox = targetBox[otherTarget];
                    if (otherBox == -1)
                    {
                        int thisNewDistance = targetDistanceMaps[otherTarget][boxCoordinates[thisBox]];
                        if (thisNewDistance < distance[thisBox])
                        {
                            boxTarget[thisBox] = otherTarget;
                            targetBox[thisTarget] = otherBox;
                            targetBox[otherTarget] = thisBox;
                            distance[thisBox] = thisNewDistance;
                            goto retry;
                        }
                    }
                    else
                    {
                        int thisNewDistance = targetDistanceMaps[otherTarget][boxCoordinates[thisBox]];
                        int otherNewDistance = targetDistanceMaps[thisTarget][boxCoordinates[otherBox]];
                        if (thisNewDistance + otherNewDistance < distance[thisBox] + distance[otherBox])
                        {
                            boxTarget[thisBox] = otherTarget;
                            boxTarget[otherBox] = thisTarget;
                            targetBox[thisTarget] = otherBox;
                            targetBox[otherTarget] = thisBox;
                            distance[thisBox] = thisNewDistance;
                            distance[otherBox] = otherNewDistance;
                            goto retry;
                        }
                    }
                }
            }

            int lowerBound = 0;
            for (int i = 0; i < boxes; i++)
            {
                lowerBound += distance[i];
            }
            return lowerBound;
        }

        private int GetOldMatchingLowerBound()
        {
            // Calculate the distance to nearest target,
            // preventing multiple boxes going to the same target.
            // XXX: Flawed.
            int lowerBound = 0;
            int boxUsed = 0;
            int targetUsed = 0;
            for (int i = 0; i < boxes; i++)
            {
                int box = 0;
                int target = 0;
                int distance = int.MaxValue;
                for (int j = 0; j < boxes; j++)
                {
                    if ((boxUsed & (1 << j)) != 0)
                    {
                        continue;
                    }
                    int boxRow = boxCoordinates[j].Row;
                    int boxColumn = boxCoordinates[j].Column;
                    for (int k = 0; k < targets; k++)
                    {
                        if ((targetUsed & (1 << k)) != 0)
                        {
                            continue;
                        }
                        int newDistance = targetDistanceMaps[k][boxRow, boxColumn];
                        if (newDistance < distance)
                        {
                            box = j;
                            target = k;
                            distance = newDistance;
                        }
                    }
                }
#if DEBUG
                if (distance == int.MaxValue)
                {
                    Abort("couldn't match box to target");
                }
#endif
                boxUsed |= (1 << box);
                targetUsed |= (1 << target);
                lowerBound += distance;
#if false
                Log.DebugPrint("box at ({0}, {1}), target at ({2}, {3}): distance {4}",
                    boxCoordinates[box].Row, boxCoordinates[box].Column,
                    targetCoordinates[target].Row, targetCoordinates[target].Column,
                    distance);
#endif
            }
#if false
            Log.DebugPrint("lower bound = {0}", lowerBound);
#endif
            return lowerBound;
        }

        protected override void RemoveNode(Node node)
        {
            RemoveScore(node);
        }
    }
}
