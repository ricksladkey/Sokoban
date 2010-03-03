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
    public class SolverValidator
    {
        private Solver solver;
        private NodeCollection nodes;
        private Node root;

        private IPositionLookupTable<Node> transpositionTable;

        private CurrentState current;

        public SolverValidator(Solver solver, NodeCollection nodes, Node root, IPositionLookupTable<Node> transpositionTable)
        {
            this.solver = solver;
            this.nodes = nodes;
            this.root = root;
            this.transpositionTable = transpositionTable;
        }

        private void PrintTree(Node node)
        {
            Print(node);
            foreach (Node child in node.Children)
            {
                PrintTree(child);
            }
        }

        private void PrintTreeWithLevels(Node node)
        {
            Level level = new Level(solver.Level);
            current.Initialize(level, PathFinder.CreateInstance(level, false, true));

            PathFinder pathFinder = PathFinder.CreateInstance(level);
            PrintTreeWithLevels(node, level, pathFinder);
        }

        private void PrintTreeWithLevels(Node node, Level level, PathFinder pathFinder)
        {
            Print(node);
            HashKey hashKey = GetPrintHashKey(level, pathFinder);
            if (transpositionTable.ContainsKey(hashKey))
            {
                Node other = transpositionTable[hashKey];
                Log.DebugPrint("table node: {0}", Node.IsEmpty(other) ? -1 : other.ID);
            }
            else
            {
                Log.DebugPrint("not in table");
            }
            Print(level);

            MoveState state = new MoveState();
            state.PrepareToMove(node, ref current);

            foreach (Node child in node.Children)
            {
                state.DoMove(child, ref current);

                PrintTreeWithLevels(child, level, pathFinder);

                state.UndoMove(ref current);
            }

            state.FinishMoving(ref current);
        }

        private HashKey GetPrintHashKey(Level level, PathFinder pathFinder)
        {
            if (solver.OptimizeMoves)
            {
                return level.GetOccupantsHashKey();
            }
            Coordinate2D sokobanCoord = level.SokobanCoordinate;
            pathFinder.Find();
            Coordinate2D proxySokobanCoord = pathFinder.GetFirstAccessibleCoordinate();
            level.MoveSokoban(proxySokobanCoord);
            HashKey hashKey = level.GetOccupantsHashKey();
            level.MoveSokoban(sokobanCoord);
            return hashKey;
        }

        public void CheckDataStructures()
        {
            bool success = true;

            nodes.ClearFlags();
            nodes.MarkFree();
            MarkInTree(root);

            // Build an inverse transposition table that maps nodes to hash keys.
            Hashtable<Node, HashKey> inverseTranspositionTable = new Hashtable<Node, HashKey>(transpositionTable.Count);
            foreach (HashKey hashKey in transpositionTable.Keys)
            {
                Node node = transpositionTable[hashKey];
                if (!Node.IsEmpty(node))
                {
                    if (inverseTranspositionTable.ContainsKey(node))
                    {
                        Log.DebugPrint("one node associated with multiple hash keys: {0}", node);
                        success = false;
                    }
                    else
                    {
                        inverseTranspositionTable.Add(node, hashKey);
                    }
                }
            }

            int terminalPositionCount = 0;
            foreach (HashKey hashKey in transpositionTable.Keys)
            {
                Node node = transpositionTable[hashKey];
                if (Node.IsEmpty(node))
                {
                    terminalPositionCount++;
                    continue;
                }
                if (node.Free)
                {
                    Log.DebugPrint("free node found in transposition table: {0}", node);
                    success = false;
                }
                if (node.InTree && node.Dormant)
                {
                    Log.DebugPrint("dormant node found in tree: {0}", node);
                    success = false;
                }
                if (!node.InTree && !node.Dormant)
                {
                    Log.DebugPrint("node neither in tree nor dormant: {0}", node);
                    success = false;
                }
            }

            int inTreeCount = 0;
            int dormantCount = 0;
            int freeCount = 0;
            int terminalInTreeCount = 0;
            foreach (Node node in nodes.AllocatedNodes)
            {
                if (node.Free)
                {
                    if (node.InTree)
                    {
                        Log.DebugPrint("free node found in tree: {0}", node);
                        success = false;
                    }

                    freeCount++;
                    continue;
                }

                int inTree = node.InTree ? 1 : 0;
                int dormant = node.Dormant ? 1 : 0;

                inTreeCount += inTree;
                dormantCount += dormant;

                if (inTree + dormant != 1)
                {
                    Log.DebugPrint("node flags inconsistent: {0}", node);
                    success = false;
                }

                bool inTable = node.InTable;
                if (inTable != inverseTranspositionTable.ContainsKey(node))
                {
                    Log.DebugPrint("in-table flag inconsistent with table: {0}", node);
                    success = false;
                }

                if (inTable)
                {
                    HashKey hashKey = inverseTranspositionTable[node];
                    if (transpositionTable.ContainsKey(hashKey))
                    {
                        Node other = transpositionTable[hashKey];
                        if (other != node)
                        {
                            if (node.Terminal)
                            {
                                if (node.InTree)
                                {
                                    terminalInTreeCount++;
                                }
                                else
                                {
                                    Log.DebugPrint("terminal dormant node found: {0}", node);
                                    success = false;
                                }
                            }
                            else
                            {
                                Log.DebugPrint("in-tree or dormant node not found in table: {0}", node);
                                success = false;
                            }
                        }
                    }
                }
                else if (node.Dormant)
                {
                    if (!node.Complete)
                    {
                        Log.DebugPrint("dormant node not in table: {0}", node);
                        success = false;
                    }
                }
                else if (node.Searched)
                {
                    if (node.Terminal)
                    {
                        if (node.InTree)
                        {
                            terminalInTreeCount++;
                        }
                        else
                        {
                            Log.DebugPrint("terminal dormant node found: {0}", node);
                            success = false;
                        }
                    }
                    else if (!node.Complete)
                    {
                        Log.DebugPrint("searched node not in table: {0}", node);
                        success = false;
                    }
                }
                else if (node.HasChildren)
                {
                    Log.DebugPrint("node with children not in table: {0}", node);
                    success = false;
                }
            }
            Log.DebugPrint("nodes: in tree {0}, dormant {1}, free {2}", inTreeCount, dormantCount, freeCount);
            Log.DebugPrint("nodes: terminal but not yet removed {0}", terminalInTreeCount);
            Log.DebugPrint("nodes: terminal positions {0}", terminalPositionCount);

            nodes.ClearFlags();

            if (!success)
            {
                throw Abort("inconsistencies found");
            }
        }

        private void MarkInTree(Node node)
        {
            node.InTree = true;
            foreach (Node child in node.Children)
            {
                MarkInTree(child);
            }
        }

        private void Print(Node node)
        {
            Log.DebugPrint(node.ToString());
        }

        private void Print(Level level)
        {
            level.AddSokoban(current.SokobanCoordinate);
            Log.DebugPrint(level.AsText);
            level.RemoveSokoban();
        }

        protected Exception Abort(string message)
        {
            string bugMessage = String.Format("bug: {0}", message);
            Log.DebugPrint(bugMessage);
            return new InvalidOperationException(bugMessage);
        }
    }
}
