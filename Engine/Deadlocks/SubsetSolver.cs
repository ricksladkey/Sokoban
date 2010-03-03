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
using Sokoban.Engine.Levels;
using Sokoban.Engine.Paths;
using Sokoban.Engine.Utilities;

namespace Sokoban.Engine.Deadlocks
{
    public abstract class SubsetSolver
    {
        protected Level level;

        protected PathFinder pathFinder;
        protected RegionFinder regionFinder;

        protected bool solvedAll;
        protected bool solvedNone;

        protected Array2D<bool> sokobanMap;

        protected CancelInfo cancelInfo;

        protected SubsetSolver(Level level)
            : this(level, false)
        {
        }

        protected SubsetSolver(Level level, bool incremental)
        {
            this.level = level;

            pathFinder = PathFinder.CreateInstance(level,false, incremental);
            regionFinder = RegionFinder.CreateInstance(level);

            sokobanMap = new Array2D<bool>(level.Height, level.Width);
            cancelInfo = new CancelInfo();
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

        public abstract void PrepareToSolve();

        public bool SolvedAll
        {
            get
            {
                return solvedAll;
            }
        }

        public bool SolvedNone
        {
            get
            {
                return solvedNone;
            }
        }

        public Array2D<bool> SokobanMap
        {
            get
            {
                return sokobanMap;
            }
        }

        public abstract void Solve();
    }
}
