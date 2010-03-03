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

using Sokoban.Engine.Core;
using Sokoban.Engine.Levels;
using Sokoban.Engine.Utilities;

namespace Sokoban.Engine.Paths
{
    class ComparisonPathFinder : PathFinder
    {
        private PathFinder finder1;
        private PathFinder finder2;
        private int lastRow;
        private int lastColumn;

        public ComparisonPathFinder(Level level, PathFinder finder1, PathFinder finder2)
            : base(level)
        {
            this.level = level;
            this.finder1 = finder1;
            this.finder2 = finder2;
        }

        private Exception Abort(string message)
        {
            Log.DebugPrint("Level:\r\n{0}", level.AsText);
            Log.DebugPrint("Distance1:\r\n{0}", finder1.AsText);
            Log.DebugPrint("Distance2:\r\n{0}", finder1.AsText);
            return new Exception(message);
        }

        #region PathFinder Members

        public override void Find(int row, int column)
        {
            lastRow = row;
            lastColumn = column;
            finder1.Find(row, column);
            finder2.Find(row, column);
        }

        public override bool IsAccessible(int row, int column)
        {
            bool result1 = finder1.IsAccessible(row, column);
            bool result2 = finder2.IsAccessible(row, column);
            if (result1 != result2)
            {
                throw Abort("accessible mismatch");
            }

            return finder1.IsAccessible(row, column);
        }

        public override int GetDistance(int row, int column)
        {
            int result1 = finder1.GetDistance(row, column);
            int result2 = finder2.GetDistance(row, column);
            bool inaccessible1 = result1 >= finder1.Inaccessible;
            bool inaccessible2 = result2 >= finder2.Inaccessible;
            if (inaccessible1 != inaccessible2)
            {
                throw Abort("accessible mismatch");
            }
            if (!inaccessible1 && !inaccessible2)
            {
                if (result1 != result2)
                {
                    throw Abort("get distance mismatch");
                }
            }

            return finder1.GetDistance(row, column);
        }

        public override Coordinate2D GetFirstAccessibleCoordinate()
        {
            Coordinate2D result1 = finder1.GetFirstAccessibleCoordinate();
            Coordinate2D result2 = finder2.GetFirstAccessibleCoordinate();
            if (result1 != result2)
            {
                throw Abort("first accessible mismatch");
            }
            return finder1.GetFirstAccessibleCoordinate();
        }

        #endregion
    }
}
