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
    public class BruteForceSolver : Solver
    {
        protected override void ValidateParameters()
        {
            if (!optimizeMoves && !optimizePushes)
            {
                throw new InvalidOperationException("must optimize either moves or pushes");
            }
        }

        protected override bool Finished()
        {
            if (!optimizeMoves && foundSolution)
            {
                return true;
            }

            if (optimizeMoves && optimizePushes && !detectNoInfluencePushes && foundSolution)
            {
                return true;
            }

            return false;
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

            int lastNodeCount = nodes.Visited;
            Search(node);
            if (verbose)
            {
                string searching = foundSolution ? "Post-searching" : "Searching";
                string info = String.Format("{0} to depth {1},\r\n", searching, depth);
                info += String.Format("Examined {0:#,##0} new nodes ({1:#,##0} total)...",
                    nodes.Visited - lastNodeCount, nodes.Visited);
                info += String.Format("\r\nNodes allocated {0:#,##0}\r\n", nodes.Allocated);
                SetInfo(info);
            }
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

            Node previous = node;
            for (Node child = node.Child; !Node.IsEmpty(child); child = child.Sibling)
            {
                if (child.Terminal)
                {
                    // Another node displaced this node from the transposition table.
                    RemoveDescendants(child);
                    node.Remove(previous, child);
                    nodes.Release(child);
                    continue;
                }

                state.DoMove(child, ref current);

                if (child.Searched)
                {
                    Search(child);
                    if (!child.HasChildren)
                    {
                        node.Remove(previous, child);
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
                    // Search the child node.
                    child.Searched = true;

                    if (level.IsComplete)
                    {
                        child.Complete = true;
                        CollectSolution(child);
                        node.Remove(previous, child);
                        child.Dormant = true;
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
                        nodes.Release(child);
                        goto removed;
                    }
                }

                previous = child;

            removed:

                state.UndoMove(ref current);
            }

            state.FinishMoving(ref current);
        }

        protected override bool FindChildren(Node node)
        {
            if (optimizePushes ? node.Pushes >= pushLimit : node.Moves >= moveLimit)
            {
                // Once we've found a solution, ignore postions that already meet or exceed its length.
                return true;
            }

            return base.FindChildren(node);
        }
    }
}
