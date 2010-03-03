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
using System.IO;
using System.Diagnostics;

using Sokoban.Engine.Core;
using Sokoban.Engine.Deadlocks;
using Sokoban.Engine.Paths;

namespace Sokoban.Engine.Levels
{
    public static class LevelUtils
    {
        public static Level GetRotatedLevel(Level level)
        {
            Level result = new Level(level);
            result.Rotate();
            return result;
        }

        public static Level GetMirroredLevel(Level level)
        {
            Level result = new Level(level);
            result.Mirror();
            return result;
        }

        public static Level GetEmptyLevel(Level level)
        {
            Array2D<Cell> newData = new Array2D<Cell>(level.Data);
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                newData[coord] = Cell.Empty;
            }
            return new Level(newData);
        }

        public static Level GetSubsetLevel(Level level, bool withSokoban, int boxes)
        {
            // Validate the desired numberes of boxes.
            if (boxes > level.Boxes)
            {
                throw new InvalidOperationException("too many boxes");
            }

            // Remove all the undesired occupants.
            int removeCount = level.Boxes - boxes;
            Array2D<Cell> newData = new Array2D<Cell>(level.Data);
            int removed = 0;
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                // Remove the undesired boxes.
                if (removed < removeCount && level.IsBox(coord))
                {
                    newData[coord] &= ~Cell.Box;
                    removed++;
                }
                if (!withSokoban)
                {
                    newData[coord] &= ~Cell.Sokoban;
                }
            }

            return new Level(newData);
        }

        public static Level GetSubsetLevel(Level level, bool withSokoban, params Coordinate2D[] coords)
        {
            Level newLevel = GetSubsetLevel(level, withSokoban, coords.Length);
            newLevel.MoveBoxes(coords);
            return newLevel;
        }

        public static bool AllBoxesAndTargetsAreAccessible(Level level)
        {
            Level emptyLevel = GetEmptyLevel(level);
            PathFinder finder = PathFinder.CreateInstance(emptyLevel);
            finder.Find(level.SokobanCoordinate);
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                if (level.IsBoxOrTarget(coord) && !finder.IsAccessible(coord))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsSolutionCompatible(Level level, MoveList solution)
        {
            Level tempLevel = new Level(level);
            foreach (OperationDirectionPair pair in solution)
            {
                if (!tempLevel.Move(pair.Operation, pair.Direction))
                {
                    if (pair.Operation != Operation.Move || !tempLevel.Move(Operation.Push, pair.Direction))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static int SolutionPushes(MoveList solution)
        {
            if (solution == null)
            {
                return -1;
            }
            int pushes = 0;
            foreach (OperationDirectionPair pair in solution)
            {
                if (pair.Operation == Operation.Push)
                {
                    pushes++;
                }
            }
            return pushes;
        }

        public static bool AllBoxesMove(Level level, MoveList solution)
        {
            return MinimumBoxMoves(level, solution) > 0;
        }

        public static int MinimumBoxMoves(Level level, MoveList solution)
        {
            if (solution == null)
            {
                return -1;
            }
            int boxes = level.Boxes;
            if (boxes == 0)
            {
                return 0;
            }
            int[] boxMoves = new int[boxes];
            Level tempLevel = new Level(level);
            foreach (OperationDirectionPair pair in solution)
            {
                if (pair.Operation == Operation.Push)
                {
                    boxMoves[tempLevel.BoxIndex(tempLevel.SokobanCoordinate + pair.Direction)]++;
                }
                tempLevel.Move(pair);
            }
            int min = int.MaxValue;
            foreach (int moves in boxMoves)
            {
                min = Math.Min(min, moves);
            }
            return min;
        }

        public static int SolutionChanges(Level level, MoveList solution)
        {
            if (solution == null)
            {
                return -1;
            }
            Level tempLevel = new Level(level);
            int changes = 0;
            int lastBoxIndex = -1;
            foreach (OperationDirectionPair pair in solution)
            {
                if (pair.Operation == Operation.Push)
                {
                    int boxIndex = tempLevel.BoxIndex(tempLevel.SokobanCoordinate + pair.Direction);
                    if (boxIndex != lastBoxIndex)
                    {
                        changes++;
                        lastBoxIndex = boxIndex;
                    }
                }
                tempLevel.Move(pair);
            }
            return changes;
        }

        public static Array2D<int> GetTraversalMap(Level level, MoveList solution)
        {
            // Create a traversal map counting the number of times
            // each square is occupied during the course of the solution,
            // initialized to zero.
            Array2D<int> traversalMap = new Array2D<int>(level.Height, level.Width);
            Level tempLevel = new Level(level);

            // Count the starting positions as one traversal.
            traversalMap[tempLevel.SokobanCoordinate]++;
            foreach (Coordinate2D coord in tempLevel.BoxCoordinates)
            {
                traversalMap[coord]++;
            }

            // Count all sokoban positions and box movements in the solution.
            foreach (OperationDirectionPair pair in solution)
            {
                if (pair.Operation == Operation.Push)
                {
                    traversalMap[tempLevel.SokobanCoordinate + 2 * pair.Direction]++;
                }
                tempLevel.Move(pair);
                traversalMap[tempLevel.SokobanCoordinate]++;
            }

            return traversalMap;
        }

        public static string MapAsText(Level level, Array2D<int> map)
        {
            List<string> stringLevel = LevelEncoder.EncodeLevel(level.Data, false);
            List<char[]> charLevel = stringLevel.ConvertAll<char[]>(delegate(string value) { return value.ToCharArray(); });
            Array2D<char> charMap = new Array2D<char>(charLevel.ToArray());
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                int count = map[coord];
                if (count != 0)
                {
                    char glyph;
                    if (count > 9)
                    {
                        glyph = '~';
                    }
                    else
                    {
                        glyph = (char)((int)'0' + count);
                    }
                    charMap[coord] = glyph;
                }
            }

            string[] rows = charMap.ConvertAllRows<string>(delegate(char[] value) { return new string(value); });
            return LevelEncoder.Concat(rows, "\r\n");
        }

        public static string MapAsText(Level level, Array2D<bool> map)
        {
            return MapAsText(level, map.ConvertAll<int>(delegate(bool value) { return value ? 1 : 0; }));
        }

        public static int MinimumTraversalCount(Level level, MoveList solution)
        {
            if (solution == null)
            {
                return 0;
            }

            Array2D<int> traversalMap = GetTraversalMap(level, solution);
            int min = Int32.MaxValue;
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                min = Math.Min(min, traversalMap[coord]);
            }

            return min;
        }

        public static Level RemoveUnusedSquares(Level level, MoveList moveList)
        {
            // Shorten to first push.
            Level finalLevel = new Level(level);
            MoveList finalMoveList = ShortenToFirstPush(finalLevel, moveList);

            // Find unused squares.
            Array2D<int> traversalMap = GetTraversalMap(finalLevel, finalMoveList);

            // Trim unused squares.
            foreach (Coordinate2D coord in finalLevel.InsideCoordinates)
            {
                if (traversalMap[coord] == 0)
                {
                    finalLevel[coord] = Cell.Wall;
                }
            }

            return finalLevel;
        }

        public static Level CleanLevel(Level level, MoveList moveList)
        {
            return TrimLevel(RemoveUnusedSquares(level, moveList));
        }

        public static Level NormalizeLevel(Level level)
        {
            LevelNormalizer normalizer = new LevelNormalizer(level);
            return normalizer.Normalize();
        }

        public static MoveList ShortenToFirstPush(Level level, MoveList moveList)
        {
            // Perform and count initial moves.
            int initialMoves = 0;
            foreach (OperationDirectionPair pair in moveList)
            {
                if (pair.Operation == Operation.Push)
                {
                    break;
                }
                level.Move(pair);
                initialMoves++;
            }

            // Make a copy of the move list and shorten it.
            MoveList finalMoveList = new MoveList(moveList);
            finalMoveList.RemoveRange(0, initialMoves);

            return finalMoveList;
        }

        public static Level TrimWalls(Level level)
        {
            // First make a copy.
            Level newLevel = new Level(level);

            // Delete walls that don't contact the interior.
            foreach (Coordinate2D coord in newLevel.Coordinates)
            {
                if (!level.IsWall(coord))
                {
                    continue;
                }

                bool foundInterior = false;
                foreach (Coordinate2D neighbor in coord.EightNeighbors)
                {
                    if (level.IsInside(neighbor))
                    {
                        foundInterior = true;
                        break;
                    }
                }

                if (!foundInterior)
                {
                    newLevel[coord] = Cell.Empty;
                }
            }

            return newLevel;
        }

        public static Level ShrinkToFit(Level level)
        {
            // Determine unneeded exterior rows and columns.
            int rowMin = level.Height;
            int colMin = level.Width;
            int rowMax = -1;
            int colMax = -1;
            foreach (Coordinate2D coord in level.Coordinates)
            {
                if (level.IsWall(coord))
                {
                    rowMin = Math.Min(rowMin, coord.Row);
                    colMin = Math.Min(colMin, coord.Column);
                    rowMax = Math.Max(rowMax, coord.Row);
                    colMax = Math.Max(colMax, coord.Column);
                }
            }

            Array2D<Cell> data = level.Data.GetSubarray(rowMin, colMin, rowMax - rowMin + 1, colMax - colMin + 1);
            return new Level(data);
        }

        public static Level TrimLevel(Level level)
        {
            return ShrinkToFit(TrimWalls(level));
        }

        public static bool HasDeadEnds(Level level)
        {
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                // Count neighbors.
                int neighbors = 0;
                foreach (Coordinate2D neighbor in coord.FourNeighbors)
                {
                    if (level.IsFloor(neighbor))
                    {
                        neighbors++;
                    }
                }
                if (neighbors == 1)
                {
                    return true;
                }
            }

            return false;
        }

        public static Array2D<bool> GetIslandMap(Level level)
        {
            Array2D<bool> islandMap = new Array2D<bool>(level.Height, level.Width);
            foreach (Coordinate2D coord in islandMap.Coordinates)
            {
                islandMap[coord] = level.IsWall(coord);
            }
            islandMap.FloodFill(FindFirstWall(level), true, false);
            return islandMap;
        }

        public static Array2D<bool> GetCapturedMap(Level level)
        {
            // Create a map of cell positions that if occupied correspond
            // to a deadlock.  This is similar to a simple deadlock map
            // but returns a different result if there are no targets.

            Array2D<bool> simpleDeadlockMap = new Array2D<bool>(level.Height, level.Width);

            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                if (level.IsTarget(coord))
                {
                    continue;
                }
                bool wallUp = level.IsWall(coord + Direction.Up);
                bool wallDown = level.IsWall(coord + Direction.Down);
                bool wallLeft = level.IsWall(coord + Direction.Left);
                bool wallRight = level.IsWall(coord + Direction.Right);
                if (wallUp && (wallLeft || wallRight))
                {
                    simpleDeadlockMap[coord] = true;
                    continue;
                }
                if (wallDown && (wallLeft || wallRight))
                {
                    simpleDeadlockMap[coord] = true;
                    continue;
                }
            }

            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                if (simpleDeadlockMap[coord])
                {
                    CheckForCapturedLine(level, simpleDeadlockMap, coord, Direction.Up, Direction.Right);
                    CheckForCapturedLine(level, simpleDeadlockMap, coord, Direction.Down, Direction.Right);
                    CheckForCapturedLine(level, simpleDeadlockMap, coord, Direction.Left, Direction.Down);
                    CheckForCapturedLine(level, simpleDeadlockMap, coord, Direction.Right, Direction.Down);
                }
            }

            return simpleDeadlockMap;
        }

        private static void CheckForCapturedLine(Level level, Array2D<bool> simpleDeadlockMap,
            Coordinate2D coord, Coordinate2D perpendicular, Coordinate2D parallel)
        {
            // Check for straight walls that look like this:
            //     ###########
            //     #.->      #
            // where the dot is the starting position
            // and perpendicular is the direction of the wall
            // and parallel is the direction to search.

            if (!level.IsWall(coord - parallel))
            {
                return;
            }

            bool isCapturedLine = false;
            for (Coordinate2D c = coord; !level.IsTarget(c); c += parallel)
            {
                if (!level.IsWall(c + perpendicular))
                {
                    break;
                }
                if (level.IsWall(c + parallel))
                {
                    isCapturedLine = true;
                    break;
                }
            }

            if (!isCapturedLine)
            {
                return;
            }

            for (Coordinate2D c = coord; !level.IsWall(c); c += parallel)
            {
                simpleDeadlockMap[c] = true;
            }
        }

        public static bool HasCapturedTargets(Level level)
        {
            // Get a captured map for the level without any targets.
            Array2D<bool> simpleDeadlockMap = DeadlockFinder.GetSimpleDeadlockMap(GetEmptyLevel(level));

            // Check for captured targets in the original level.
            foreach (Coordinate2D coord in level.Coordinates)
            {
                if (level.IsTarget(coord) && simpleDeadlockMap[coord])
                {
                    return true;
                }
            }
            return false;
        }

        public static Coordinate2D FindFirstEmpty(Level level)
        {
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                if (level.IsEmpty(coord))
                {
                    return coord;
                }
            }
            return Coordinate2D.Undefined;
        }

        public static Coordinate2D FindFirstBox(Level level)
        {
            foreach (Coordinate2D coord in level.BoxCoordinates)
            {
                return coord;
            }
            return Coordinate2D.Undefined;
        }

        public static Coordinate2D FindFirstTarget(Level level)
        {
            foreach (Coordinate2D coord in level.TargetCoordinates)
            {
                return coord;
            }
            return Coordinate2D.Undefined;
        }

        public static Coordinate2D FindFirstWall(Level level)
        {
            foreach (Coordinate2D coord in level.WallCoordinates)
            {
                return coord;
            }
            return Coordinate2D.Undefined;
        }


        public static Array2D<int> GetNoBoxMap(Level level)
        {
            return GetNoBoxMap(level, DeadlockFinder.GetSimpleDeadlockMap(level));
        }

        public static Array2D<int> GetNoBoxMap(Level level, Array2D<bool> simpleDeadlockMap)
        {
            // Initially set all floor squares as no-box squares, but not yet assigned to a region.
            Array2D<int> noBoxMap = new Array2D<int>(level.Height, level.Width);
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                noBoxMap[coord] = -1;
            }

            // For each box in the original level, visit as many squares as possible.
            foreach (Coordinate2D coord in level.BoxCoordinates)
            {
                CheckBox(level, noBoxMap, simpleDeadlockMap, coord);
            }

            // The map now has a value of -1 for all no-box squares.
            // Next assign a unique value to each disconnected region of no-box squares.
            int region = 1;
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                // Process all no-box squares that haven't been assigned to a region yet.
                if (noBoxMap[coord] == -1)
                {
                    noBoxMap.FloodFill(coord, -1, region);
                    region++;
                }
            }

            return noBoxMap;
        }

        private static void CheckBox(Level level, Array2D<int> noBoxMap, Array2D<bool> simpleDeadlockMap, Coordinate2D coord)
        {
            // Check whether we've visited this square before.
            if (noBoxMap[coord] == 0)
            {
                return;
            }

            // This square is not a no-box square.
            noBoxMap[coord] = 0;

            // Recursively check for other squares that are not no-box squares.
            foreach (Direction direction in Direction.Directions)
            {
                // Get parallel and perpendicular offsets.
                Coordinate2D parallel = direction;
                Coordinate2D perpendicular = Coordinate2D.Transpose(parallel);

                // Check whether we can move the box to the new position.
                if (level.IsFloor(coord - parallel) && level.IsFloor(coord + parallel))
                {
                    Coordinate2D oldBoxCoord = coord;
                    Coordinate2D newBoxCoord = coord + parallel;

                    // Check for the special case of no influence pushes that lead to deadlocks.
                    bool specialCase = false;
                    while (level.IsWall(oldBoxCoord + perpendicular) &&
                        level.IsWall(oldBoxCoord - perpendicular) &&
                        ((level.IsWall(newBoxCoord + perpendicular) &&
                        level.IsWallOrEmpty(newBoxCoord - perpendicular)) ||
                        (level.IsWallOrEmpty(newBoxCoord + perpendicular) &&
                        level.IsWall(newBoxCoord - perpendicular))))
                    {
                        if (level.IsTarget(newBoxCoord))
                        {
                            break;
                        }
                        if (!level.IsFloor(newBoxCoord) || simpleDeadlockMap[newBoxCoord])
                        {
                            specialCase = true;
                            break;
                        }

                        oldBoxCoord += parallel;
                        newBoxCoord += parallel;
                    }

                    // Otherwise recursively check for more squares.
                    if (!specialCase)
                    {
                        CheckBox(level, noBoxMap, simpleDeadlockMap, coord + parallel);
                    }
                }
            }
        }

        public static Coordinate2D MatchesPatternAt(Level level, Level pattern)
        {
            foreach (Coordinate2D coord in level.Coordinates)
            {
                if (MatchesPattern(level, coord.Row, coord.Column, pattern))
                {
                    return coord;
                }
            }
            return Coordinate2D.Undefined;
        }

        public static bool MatchesPattern(Level level, int rowOffset, int columnOffset, Level pattern)
        {
            // Compute any overlap.
            int row1 = pattern.Height - 2 + rowOffset;
            int column1 = pattern.Width - 2 + columnOffset;
            int rowLimit = pattern.Height - 1 + Math.Min(0, level.Height - row1);
            int columnLimit = pattern.Width - 1 + Math.Min(0, level.Width - column1);

            // Match all defined cells in the pattern to the level.
            for (int row = 1; row < rowLimit; row++)
            {
                for (int column = 1; column < columnLimit; column++)
                {
                    // Undefined always matches.
                    Cell patternCell = pattern[row, column];
                    if (patternCell == Cell.Undefined)
                    {
                        continue;
                    }

                    // Compute coordinates accounting for initial values and offset.
                    int levelRow = row - 1 + rowOffset;
                    int levelColumn = column - 1 + columnOffset;
                    Cell levelCell = level[levelRow, levelColumn];

                    // Outside in the level matches wall in the pattern.
                    if (Level.IsOutside(levelCell) && Level.IsWall(patternCell))
                    {
                        continue;
                    }

                    // Remove sokoban from level cell if not in pattern.
                    if (!Level.IsSokoban(patternCell))
                    {
                        levelCell &= ~Cell.Sokoban;
                    }

                    // No match if corresponding cell in level is different from pattern.
                    if (patternCell != levelCell)
                    {
                        return false;
                    }
                }
            }

            // If the pattern exceeds the bounds of the level,
            // all the non-overlapping squares must match wall.

            // Quadrant below the level.
            for (int row = rowLimit; row < pattern.Height - 1; row++)
            {
                for (int column = 1; column < columnLimit; column++)
                {
                    if (pattern[row, column] != Cell.Wall)
                    {
                        return false;
                    }
                }
            }

            // Quadrant to the right of the level.
            for (int row = 1; row < pattern.Height - 1; row++)
            {
                for (int column = columnLimit; column < pattern.Width - 1; column++)
                {
                    if (pattern[row, column] != Cell.Wall)
                    {
                        return false;
                    }
                }
            }

            // Quadrant below and to the right of the level.
            for (int row = rowLimit; row < pattern.Height - 1; row++)
            {
                for (int column = columnLimit; column < pattern.Width - 1; column++)
                {
                    if (pattern[row, column] != Cell.Wall)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool MatchesPattern(Level level, Coordinate2D coord, Level pattern)
        {
            return MatchesPattern(level, coord.Row, coord.Column, pattern);
        }

        public static bool MatchesDesign(Level level, Level design)
        {
            // First compare boxes.
            if (level.Boxes != design.Boxes)
            {
                return false;
            }

            // Find upper-left box in level.
            Coordinate2D levelCoord = FindFirstBox(level);

            // Find upper-left box in design.
            Coordinate2D designCoord = FindFirstBox(design);

            // Compute level coordinates relative to design.
            int rowOffset = levelCoord.Row - designCoord.Row + 1;
            int columnOffset = levelCoord.Column - designCoord.Column + 1;

            // Match at the specified offset.
            return MatchesPattern(level, rowOffset, columnOffset, design);
        }

        public static Level Replace(Level level, int rowOffset, int columnOffset, Level replacement)
        {
            Level newLevel = new Level(level);
            bool lostSokoban = false;
            for (int row = 1; row < replacement.Height - 1; row++)
            {
                for (int column = 1; column < replacement.Width - 1; column++)
                {
                    if (replacement[row, column] != Cell.Undefined)
                    {
                        int rowTarget = row - 1 + rowOffset;
                        int columnTarget = column - 1 + columnOffset;
                        if (!level.IsValid(rowTarget, columnTarget))
                        {
                            return ExpandAndReplace(level, rowOffset, columnOffset, replacement);
                        }
                        if (level.IsSokoban(rowTarget, columnTarget) && !replacement.IsSokoban(row, column))
                        {
                            lostSokoban = true;
                        }
                        newLevel[rowTarget, columnTarget] = replacement[row, column];
                    }
                }
            }

            if (lostSokoban)
            {
                // Find an empty cell to relocate the sokoban to.
                foreach (Coordinate2D coord in replacement.Coordinates)
                {
                    if (replacement[coord] == Cell.Empty)
                    {
                        newLevel.MoveSokoban(coord.Row - 1 + rowOffset, coord.Column - 1 + columnOffset);
                        return newLevel;
                    }
                }
            }

            return newLevel;
        }

        private static Level ExpandAndReplace(Level level, int rowOffset, int columnOffset, Level replacement)
        {
            // Compute the overlap.
            int row0 = Math.Min(0, rowOffset - 1);
            int column0 = Math.Min(0, columnOffset - 1);
            int row1 = Math.Max(level.Height, replacement.Height - 2 + rowOffset);
            int column1 = Math.Max(level.Width, replacement.Width - 2 + columnOffset);
            int height = row1 - row0;
            int width = column1 - column0;

            // Expand the original level.
            Array2D<Cell> newData = new Array2D<Cell>(height, width);
            newData.SetAll(Cell.Wall);
            level.Data.CopyTo(newData, -row0, -column0, 0, 0, level.Height, level.Width);
            Level newLevel = new Level(newData);

            // Retry the replacement.
            return Replace(newLevel, rowOffset - row0, columnOffset - column0, replacement);
        }

        public static Level Replace(Level level, Coordinate2D replacementCoord, Level replacement)
        {
            return Replace(level, replacementCoord.Row, replacementCoord.Column, replacement);
        }
    }
}
