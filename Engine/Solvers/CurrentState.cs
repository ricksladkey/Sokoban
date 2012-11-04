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
    public struct CurrentState
    {
        public Level Level;
        public PathFinder PathFinder;
        public bool Incremental;
        public int ParentIndex;
        public Node[] Parents;
        public int SokobanRow;
        public int SokobanColumn;
        public HashKey HashKey;

        public Coordinate2D SokobanCoordinate
        {
            get
            {
                return new Coordinate2D(SokobanRow, SokobanColumn);
            }
        }

        public void Initialize(Level level, PathFinder pathFinder)
        {
            Level = level;
            PathFinder = pathFinder;
            SokobanRow = level.SokobanRow;
            SokobanColumn = level.SokobanColumn;
            level.RemoveSokoban();
            HashKey = level.GetOccupantsHashKey();
        }
    }
}
