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
    public struct MoveState
    {
        public int OldSokobanRow;
        public int OldSokobanColumn;
        public int OldBoxRow;
        public int OldBoxColumn;
        public int NewBoxRow;
        public int NewBoxColumn;
        public int ParentPushes;
#if USE_INCREMENTAL_HASH_KEY
        public HashKey OldHashKey;
#endif

        public void PrepareToMove(Node node, ref CurrentState current)
        {
            // Save the parent (theoretically could overflow).
            current.Parents[current.ParentIndex++] = node;

            // Save the parent's sokoban coordinate.
            OldSokobanRow = current.SokobanRow;
            OldSokobanColumn = current.SokobanColumn;

            // Record the parent's pushes.
            ParentPushes = node.Pushes;

#if USE_INCREMENTAL_PATH_FINDER
#if true
            // Force a full calculation when switching
            // from full to incremental.
            bool oldIncremental = current.Incremental;
            current.Incremental = node.HasChildren && !node.Child.Searched;
            if (!oldIncremental && current.Incremental)
            {
                current.PathFinder.ForceFullCalculation();
            }
#endif
#endif

#if USE_INCREMENTAL_HASH_KEY
            // Save the parent's hash key.
            OldHashKey = current.HashKey;
#endif
        }

        public void DoMove(Node child, ref CurrentState current)
        {
            Direction direction = child.Direction;
            int pushes = child.Pushes - ParentPushes;

            // Calculate old and new squares.
            int v = Direction.GetVertical(direction);
            int h = Direction.GetHorizontal(direction);
            OldBoxRow = child.Row + v;
            OldBoxColumn = child.Column + h;
            NewBoxRow = OldBoxRow + pushes * v;
            NewBoxColumn = OldBoxColumn + pushes * h;
            current.SokobanRow = NewBoxRow - v;
            current.SokobanColumn = NewBoxColumn - h;

            // Perform the moves and pushes associated with this child.
#if USE_INCREMENTAL_PATH_FINDER
            if (!current.Incremental)
            {
                current.PathFinder.ForceFullCalculation();
            }
            current.PathFinder.MoveBox(Operation.Push, OldBoxRow, OldBoxColumn, NewBoxRow, NewBoxColumn); 
#else
            current.Level.MoveBox(OldBoxRow, OldBoxColumn, NewBoxRow, NewBoxColumn);
#endif
#if USE_INCREMENTAL_HASH_KEY
            // Calculate an incremental update of hash key.
            current.HashKey = OldHashKey ^
                HashKey.GetBoxHashKey(OldBoxRow, OldBoxColumn) ^
                HashKey.GetBoxHashKey(NewBoxRow, NewBoxColumn);
#endif
        }

        public void UndoMove(ref CurrentState current)
        {
            // Restore original position.
#if USE_INCREMENTAL_PATH_FINDER
            if (!current.Incremental)
            {
                current.PathFinder.ForceFullCalculation();
            }
            current.PathFinder.MoveBox(Operation.Pull, NewBoxRow, NewBoxColumn, OldBoxRow, OldBoxColumn);
#else
            current.Level.MoveBox(NewBoxRow, NewBoxColumn, OldBoxRow, OldBoxColumn);
#endif

            // The current sokoban coordinate and current hash key are now temporarily incorrect.
        }

        public void FinishMoving(ref CurrentState current)
        {
            // Restore the parent.
            --current.ParentIndex;

            // Restore the parent's sokoban coordinate.
            current.SokobanRow = OldSokobanRow;
            current.SokobanColumn = OldSokobanColumn;

#if USE_INCREMENTAL_HASH_KEY
            // Restore the parent's hash key.
            current.HashKey = OldHashKey;
#endif
#if USE_INCREMENTAL_PATH_FINDER
            current.Incremental = false;
#endif
        }

        public void Find(ref CurrentState current)
        {
#if USE_INCREMENTAL_PATH_FINDER
            if (!current.Incremental)
            {
                current.PathFinder.ForceFullCalculation();
            }
            current.PathFinder.IncrementalFind(current.SokobanRow, current.SokobanColumn);
#else
            current.PathFinder.Find(current.SokobanRow, current.SokobanColumn);
#endif
        }
    }
}
