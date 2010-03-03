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

#define USE_INCREMENTAL_PATH_FINDER

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
    public class BackwardsSubsetSolver : SubsetSolver
    {
        private struct Move
        {
            public int Row;
            public int Column;
            public Direction Direction;

            public Move(int row, int column, Direction direction)
            {
                Row = row;
                Column = column;
                Direction = direction;
            }

            public override string ToString()
            {
                return String.Format("Move: ({0}, {1}) {2}", Row, Column, Direction);
            }
        }

#if USE_INCREMENTAL_PATH_FINDER
        private const bool incremental = true;
#else
        private const bool incremental = false;
#endif

        private Level originalLevel;

        private Coordinate2D[] boxCoordinates;
        private int boxes;

        private bool useReliablePositionSet;
        private bool assessPossibility;
        private int capacity;

        private Array2D<Direction[]> pullMap;

        private IPositionSet visited;

        private Coordinate2D[] regions;
        bool[] solvedArray;

        public BackwardsSubsetSolver(Level originalLevel, Level level, int capacity, bool assessPossibility)
            : base(level, incremental)
        {
            this.originalLevel = originalLevel;

            this.boxCoordinates = level.BoxCoordinates;
            this.boxes = boxCoordinates.Length;

            this.assessPossibility = assessPossibility;
            this.capacity = capacity;

            useReliablePositionSet = true;

            // Create a set of visited positions,
            // assuming three sokoban squares per position.
            if (useReliablePositionSet)
            {
                // A useReliablePositionSet position set does not need to be validated.
                visited = ReliablePositionSet.CreateInstance(level, 3 * capacity);
            }
            else
            {
                // An unreliable position set might miss some achievable
                // positions and as a result deadlocks calculated using
                // it need to be validated as truly unsolvable.
                visited = new UnreliablePositionSet(level, 3 * capacity);
            }

            // Create scratch arrays for solving.
            regions = new Coordinate2D[level.InsideSquares];
            solvedArray = new bool[level.InsideSquares];
        }

        public override void PrepareToSolve()
        {
            // Pre-calculate a map of directions a box can be pulled in.
            CalculatePullMap();

            // For all sized subsets of targets, try to visit as
            // many predecessors as possible while avoiding duplicates.
            VisitAllPulls();

            // Give feedback to the user.
            string info =
                "Calculating deadlocks...\r\n" +
                "    Preparing backwards subset solver\r\n";
            info += string.Format("    Visited {0} positions for {1} boxes, capacity {2}.", visited.Count, level.Boxes, capacity);
            cancelInfo.Info = info;
        }

        private void CalculatePullMap()
        {
            // Pre-calculate a map of possible directions in which a box
            // at a given coordinate can be pulled.

            pullMap = new Array2D<Direction[]>(level.Height, level.Width);
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                List<Direction> directions = new List<Direction>();
                foreach (Direction direction in Direction.Directions)
                {
                    Coordinate2D sokobanCoood = coord + 2 * direction;
                    Coordinate2D newBoxCoord = coord + direction;

                    if (!level.IsFloor(sokobanCoood))
                    {
                        // The sokoban can never reach to pull this box.
                        continue;
                    }

                    if (!level.IsFloor(newBoxCoord))
                    {
                        // The box cannot be pulled in this direction.
                        continue;
                    }

                    directions.Add(direction);
                }
                pullMap[coord] = directions.ToArray();
            }
        }

        private void VisitAllPulls()
        {
            // Recursively visit all predecessors.  This
            // algorithm would be implemented most
            // naturally as a depth-first recursive
            // search, but doing so easily exhausts
            // the call stack.

            // Create a stack of moves that need to be processed.
            IStack<Move> moves = new SegmentedStack<Move>();

            // For all subsets of boxes on targets of the specified size.
            foreach (Coordinate2D[] targets in CoordinateUtils.GetCoordinateSets(level.TargetCoordinates, level.Boxes))
            {
                // Check for cancel.
                if (cancelInfo.Cancel)
                {
                    return;
                }

                // Move the boxes into position.
                level.MoveBoxes(targets);

                // Add moves from all possible starting sokoban squares.
                foreach (Coordinate2D coord in regionFinder.Coordinates)
                {
                    pathFinder.Find(coord);
                    FindPulls(moves);
                }

                // Record position of pulls with no predecessors.
                int index = moves.Count;

                // While the stack is not empty.
                while (!moves.IsEmpty)
                {
                    // Check for cancel.
                    if (cancelInfo.Cancel)
                    {
                        return;
                    }

                    // Pop the next move off the stack.
                    Move move = moves.Pop();
                    int row = move.Row;
                    int column = move.Column;
                    Coordinate2D direction = move.Direction;

                    // If the move was previously searched, then undo it.
                    if (row < 0)
                    {
                        row = -row;
                        pathFinder.MoveBox(Operation.Push, row, column, row - direction.Row, column - direction.Column);
                        continue;
                    }

                    // Re-add the move as searched to undo it later.
                    moves.Push(new Move(-row, column, move.Direction));

                    // Check for move with no predecessor.
                    if (moves.Count <= index)
                    {
                        // Force a full search in preperation for
                        // an incremental search.
                        pathFinder.ForceFullCalculation();
                    }

                    // Execute the move and put the virtual sokoban on its normalized square.
                    pathFinder.MoveBoxAndIncrementalFind(Operation.Pull, row - direction.Row, column - direction.Column, row, column);
                    Coordinate2D sokobanCoord = pathFinder.GetFirstAccessibleCoordinate();

                    // If previously visited continue; otherwise add it to the set.
                    if (!visited.Add(sokobanCoord))
                    {
                        continue;
                    }

                    // Find all possible pulls from the current position and add them to the stack.
                    FindPulls(moves);
                }
            }
        }

        private void FindPulls(IStack<Move> moves)
        {
            // Check all boxes for potential pulls.
            for (int i = 0; i < boxes; i++)
            {
                int row = boxCoordinates[i].Row;
                int column = boxCoordinates[i].Column;
                Direction[] directions = pullMap[row, column];
                int n = directions.Length;
                for (int j = 0; j < n; j++)
                {
                    Direction direction = directions[j];
                    int v = Direction.GetVertical(direction);
                    int h = Direction.GetHorizontal(direction);

                    // Check that the new box square is not a box;
                    // It can't be anything else because of the pull map.
                    int newBoxRow = row + v;
                    int newBoxColumn = column + h;
                    if (!pathFinder.IsAccessible(newBoxRow, newBoxColumn))
                    {
                        continue;
                    }

                    // Check that we can access to pull this box.
                    int sokobanRow = newBoxRow + v;
                    int sokobanColumn = newBoxColumn + h;
                    if (!pathFinder.IsAccessible(sokobanRow, sokobanColumn))
                    {
                        continue;
                    }

                    // Add this move to the stack.
                    moves.Push(new Move(newBoxRow, newBoxColumn, direction));
                }
            }
        }

        public override void Solve()
        {
            // Find out whether this set is solvable from
            // all possible sokoban positions or from no
            // sokoban positions.
            solvedAll = true;
            solvedNone = true;

            // Try all possible sokoban starting squares.
            int n = 0;
            foreach (Region region in regionFinder.Regions)
            {
                Coordinate2D coord = region.Coordinate;
                int count = region.Count;

                // Check whether we want to assess positions for
                // possibility.
                if (assessPossibility)
                {
                    // Ignoring impossible positions sometimes produces
                    // surprising but correct results.  The primary
                    // consequence of not taken them into account
                    // is surplus conditional deadlocks.
                    if (!IsPossiblePosition(coord, count))
                    {
                        continue;
                    }
                }

                // Record and count the starting sokoban regions.
                regions[n] = coord;

                // Try to solve the level from this position.
                bool solved = visited.Contains(coord);

                solvedAll = solvedAll && solved;
                solvedNone = solvedNone && !solved;

                // Record which sokoban coordinates that can solve this set.
                solvedArray[n++] = solved;
            }

            // Prepare the sokoban map only if some but not all positions solve the position.
            if (!solvedAll && !solvedNone)
            {
                // Clear the sokoban map.
                sokobanMap.SetAll(false);

                // Iterate over the solved sokoban coordinates.
                for (int i = 0; i < n; i++)
                {
                    if (solvedArray[i])
                    {
                        pathFinder.Find(regions[i]);
                        foreach (Coordinate2D coord in pathFinder.AccessibleCoordinates)
                        {
                            sokobanMap[coord] = true;
                        }
                    }
                }
            }
        }

        private bool IsPossiblePosition(Coordinate2D coord, int accessibleSquares)
        {
            // Find accessible coordinates from this square.
            pathFinder.Find(coord);

            // If the sokoban started in this region, it is
            // theoretically possible for it to be confined
            // in a small area.
            if (pathFinder.IsAccessible(originalLevel.SokobanCoordinate))
            {
                // If the sokoban is confined to a single square
                // then this position is only possible if
                // all the boxes are also in their starting
                // positions.
                if (accessibleSquares == 1)
                {
                    for (int i = 0; i < boxes; i++)
                    {
                        if (!originalLevel.IsBox(boxCoordinates[i]))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            // If there are four or fewer squares in the
            // region, it is not possible for the sokoban
            // to close itself in if it wasn't already
            // there at the beginning of the level.
            if (accessibleSquares <= 4)
            {
                return false;
            }

            // If there are five or six squares, the position
            // is possible only if there is a box that can
            // be pulled in to open a door to the outside.
            if (accessibleSquares <= 6)
            {
                if (CanPullDoor())
                {
                    return true;
                }
                return false;
            }

            return true;
        }

        private bool CanPullDoor()
        {
            // Try to find a box that can be pulled in
            // and will open up a door to the outside.
            foreach (Coordinate2D boxCoord in level.BoxCoordinates)
            {
                // Check whether the box contacts both the
                // interior and exterior.
                bool hasInteriorFace = false;
                bool hasExteriorFace = false;
                foreach (Coordinate2D neighbor in boxCoord.FourNeighbors)
                {
                    if (level.IsEmpty(neighbor))
                    {
                        if (pathFinder.IsAccessible(neighbor))
                        {
                            hasInteriorFace = true;
                        }
                        else
                        {
                            hasExteriorFace = true;
                        }
                    }
                }
                if (!hasInteriorFace || !hasExteriorFace)
                {
                    continue;
                }

                // Try to pull it in.
                foreach (Direction direction in Direction.Directions)
                {
                    if (pathFinder.IsAccessible(boxCoord + direction) &&
                        pathFinder.IsAccessible(boxCoord + 2 * direction))
                    {
                        if (CanPullDoor(boxCoord + direction, direction))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool CanPullDoor(Coordinate2D boxCoord, Direction direction)
        {
            Coordinate2D evacuatedCoord = boxCoord - direction;
            Coordinate2D sokobanCoord = boxCoord + direction;

            // If the evacuated square contacts the interior on
            // one of its sides and the sokoban square also
            // contacts another interior square on one of its
            // sides then a path has been opened up to the outside.
            Direction perpendicular = Direction.Rotate(direction);
            if (pathFinder.IsAccessible(evacuatedCoord - perpendicular) ||
                pathFinder.IsAccessible(evacuatedCoord + perpendicular))
            {
                if (pathFinder.IsAccessible(sokobanCoord - perpendicular) ||
                    pathFinder.IsAccessible(sokobanCoord + perpendicular))
                {
                    return true;
                }
            }

            // Otherwise keep trying to pull the box further in
            // while avoiding the direction we just came from.
            Direction oppositeDirection = Direction.GetOpposite(direction);
            foreach (Direction otherDirection in Direction.Directions)
            {
                if (otherDirection == oppositeDirection)
                {
                    continue;
                }

                if (pathFinder.IsAccessible(boxCoord + otherDirection) &&
                    pathFinder.IsAccessible(boxCoord + 2 * otherDirection))
                {
                    if (CanPullDoor(boxCoord + otherDirection, otherDirection))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
