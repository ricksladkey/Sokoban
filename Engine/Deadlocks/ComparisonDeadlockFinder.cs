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
using Sokoban.Engine.Utilities;

namespace Sokoban.Engine.Deadlocks
{
    public class ComparisonDeadlockFinder : DeadlockFinder
    {
        private DeadlockFinder finder1;
        private DeadlockFinder finder2;
        private bool detectMisses1;
        private bool detectMisses2;

        public ComparisonDeadlockFinder(Level level, DeadlockFinder finder1, DeadlockFinder finder2, bool detectMisses1, bool detectMisses2)
            : base(level)
        {
            this.finder1 = finder1;
            this.finder2 = finder2;
            this.detectMisses1 = detectMisses1;
            this.detectMisses2 = detectMisses2;
        }

        public override bool IsDeadlocked(int sokobanRow, int sokobanColumn)
        {
            // IsDeadlocked is not required to also detect simple deadlocks.
            if (IsSimplyDeadlocked())
            {
                return finder1.IsDeadlocked(sokobanRow, sokobanColumn);
            }

            bool result1 = finder1.IsDeadlocked(sokobanRow, sokobanColumn);
            bool result2 = finder2.IsDeadlocked(sokobanRow, sokobanColumn);

            if (detectMisses1 && !result1 && result2)
            {
                Log.DebugPrint("==========================================");
                level.AddSokoban(sokobanRow, sokobanColumn);
                Log.DebugPrint(level.AsText);
                level.RemoveSokoban();
                throw new Exception("first deadlock missed second deadlock");
            }
            if (detectMisses2 && result1 && !result2)
            {
                Log.DebugPrint("==========================================");
                level.AddSokoban(sokobanRow, sokobanColumn);
                Log.DebugPrint(level.AsText);
                level.RemoveSokoban();
                throw new Exception("second deadlock missed first deadlock");
            }

            return result1;
        }
    }
}
