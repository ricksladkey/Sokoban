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
    public static class NodeHelper
    {
        public static string AsText(Node node)
        {
            if (node.ID == -1)
            {
                return "Node: Empty";
            }
            int siblingID = Node.IsEmpty(node.Sibling) ? -1 : node.Sibling.ID;
            string line = String.Format("Node {0}: ({1}, {2}, {3}) {4}/{5}/{6} s {7} c",
                node.ID, node.Row, node.Column, node.Direction, node.Moves, node.Pushes, node.Score, siblingID);
            foreach (Node child in node.Children)
            {
                line += String.Format(" {0}", child.ID);
            }
            line += " flags: ";
            if (node.Complete)
            {
                line += "c";
            }
            if (node.Searched)
            {
                line += "s";
            }
            if (node.Terminal)
            {
                line += "t";
            }
            if (node.Dormant)
            {
                line += "d";
            }
            if (node.InTable)
            {
                line += "i";
            }
            if (node.InTree)
            {
                line += "I";
            }
            if (node.Free)
            {
                line += "F";
            }
            return line;
        }
    }
}
