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

namespace Sokoban.Engine.Deadlocks
{
    public static class CoordinateUtils
    {
        private class AlternatingAxisState
        {
            public Coordinate2D[] Set;
            public Array2D<bool> SetMap;
            public int N;
            public int K;
            public int Index;
            public Coordinate2D[] Coordinates;
        }

        public static int GetNumberOfSets(int setSize, int subsetSize)
        {
            return CombinationUtils.Choose(setSize, subsetSize);
        }

        public static Coordinate2D[] SortCoordinates(params Coordinate2D[] coords)
        {
            Coordinate2D[] sortedCoords = new Coordinate2D[coords.Length];
            coords.CopyTo(sortedCoords, 0);
            Array.Sort<Coordinate2D>(sortedCoords);
            return sortedCoords;
        }

        public static IEnumerable<Coordinate2D[]> GetCoordinateSets(IEnumerable<Coordinate2D> set, int size)
        {
            return GetCoordinateSets(new List<Coordinate2D>(set).ToArray(), size);
        }

        public static IEnumerable<Coordinate2D[]> GetCoordinateSets(Coordinate2D[] set, int size)
        {
            Coordinate2D[] coords = new Coordinate2D[size];
            int n = set.Length;
            foreach (int[] combination in CombinationUtils.GetCombinations(set.Length, size))
            {
                // Copy the coordinates in the combination.
                for (int i = 0; i < size; i++)
                {
                    coords[i] = set[combination[i]];
                }
                yield return coords;
            }
        }

        public static IEnumerable<Coordinate2D[]> GetAlternatingAxisSets(Coordinate2D[] set, int size)
        {
            // Initialize the state used to enumerate the snake set.
            AlternatingAxisState state = new AlternatingAxisState();
            state.Set = set;
            state.SetMap = GetSetMap(set);
            state.N = set.Length;
            state.K = size;
            state.Coordinates = new Coordinate2D[size];

            // Enumerate all the coordinate sets that consist of sequences
            // of alternating vertical and horizonal turns.
            foreach (Coordinate2D coord in state.Set)
            {
                state.Coordinates[0] = coord;
                state.Index = 1;

                // Enumerate sets that start with a vertical turn.
                foreach (Coordinate2D[] coords in GetAlternatingAxisSets(state, Axis.Vertical))
                {
                    yield return coords;
                }

                // Enumerate sets that start with a horizontal turn.
                foreach (Coordinate2D[] coords in GetAlternatingAxisSets(state, Axis.Horizontal))
                {
                    yield return coords;
                }
            }
        }

        private static Array2D<bool> GetSetMap(IEnumerable<Coordinate2D> set)
        {
            Coordinate2D maxCoord = new Coordinate2D(0, 0);
            foreach (Coordinate2D coord in set)
            {
                maxCoord.Row = Math.Max(maxCoord.Row, coord.Row);
                maxCoord.Column = Math.Max(maxCoord.Column, coord.Column);
            }
            Array2D<bool> setMap = new Array2D<bool>(maxCoord.Row + 2, maxCoord.Column + 2);
            foreach (Coordinate2D coord in set)
            {
                setMap[coord] = true;
            }
            return setMap;
        }

        private static IEnumerable<Coordinate2D[]> GetAlternatingAxisSets(AlternatingAxisState state, Axis axis)
        {
            // Determine the next two coordinates on either side
            // of the specified axis.
            Coordinate2D coord = state.Coordinates[state.Index - 1];
            Coordinate2D coord1;
            Coordinate2D coord2;
            Axis nextAxis;
            if (axis == Axis.Vertical)
            {
                // Check the coordinates above and below this one.
                coord1 = coord + Direction.Up;
                coord2 = coord + Direction.Down;
                nextAxis = Axis.Horizontal;
            }
            else
            {
                // Check the coordinates the the left and right of this one.
                coord1 = coord + Direction.Left;
                coord2 = coord + Direction.Right;
                nextAxis = Axis.Vertical;
            }

            // Pursue both choices.
            ++state.Index;
            for (int i = 0; i < 2; i++)
            {
                // Select the appropriate coordinate for the first or second pass.
                Coordinate2D newCoord = i == 0 ? coord1 : coord2;

                // Check whether this coordinate is in the set.
                if (state.SetMap[newCoord])
                {
                    // Store the new coordinate.
                    state.Coordinates[state.Index - 1] = newCoord;

                    // Ensure we haven't created a loop.
                    if (IsDuplicateAlternatingAxisCoordinate(state))
                    {
                        continue;
                    }

                    // Check whether we have a complete set.
                    if (state.Index == state.K)
                    {
                        // Account for sets that can be
                        // nagivated in a different order.
                        if (!IsDuplicateAlternatingAxisSet(state))
                        {
                            yield return state.Coordinates;
                        }
                    }
                    else
                    {
                        // Otherwise recurse on the opposite axis.
                        foreach (Coordinate2D[] coords in GetAlternatingAxisSets(state, nextAxis))
                        {
                            yield return coords;
                        }
                    }
                }
            }
            --state.Index;
        }

        private static bool IsDuplicateAlternatingAxisCoordinate(AlternatingAxisState state)
        {
            // Check whether the last coordinate is
            // a duplicate of one of the others.
            // It takes at least four coordinates
            // before we can have a loop.
            Coordinate2D[] coords = state.Coordinates;
            Coordinate2D coord = coords[state.Index - 1];
            int limit = state.Index - 4;
            for (int i = 0; i < limit; i++)
            {
                if (coords[i] == coord)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsDuplicateAlternatingAxisSet(AlternatingAxisState state)
        {
            Coordinate2D[] coords = state.Coordinates;
            Coordinate2D firstCoord = coords[0];
            Coordinate2D lastCoord = coords[state.K - 1];

            // If the first coordinate is after the last, then it's a duplicate.
            if (firstCoord > lastCoord)
            {
                return true;
            }

            // If the first coordinate is adjacent to the last and
            // the axis between them is perpendicular to the first
            // and last turns, then the set is a loop.
            // If the set is a loop, and the first coordinate is not
            // lexicographically the lowest or the loop direction
            // is not to the right, then it's a duplicate.
            if (Coordinate2D.GetOrthogonalDistance(firstCoord, lastCoord) == 1)
            {
                Direction loopDirection = GetDirectionBetween(lastCoord, firstCoord);
                Direction firstDirection = GetDirectionBetween(firstCoord, coords[1]);
                Direction lastDirection = GetDirectionBetween(coords[state.K - 2], lastCoord);
                if (Direction.ArePerpendicular(loopDirection, firstDirection) &&
                    Direction.ArePerpendicular(loopDirection, lastDirection))
                {
                    // Keep the loop set that starts with the most upper-left coordinate and
                    // advance clockwise.
                    if (!IsFirstCoordinateLeast(coords) || firstDirection != Direction.Right)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static Direction GetDirectionBetween(Coordinate2D coord1, Coordinate2D coord2)
        {
            if (coord1.Row == coord2.Row)
            {
                return coord1.Column < coord2.Column ? Direction.Right : Direction.Left;
            }
            if (coord1.Column == coord2.Column)
            {
                return coord1.Row < coord2.Row ? Direction.Down : Direction.Up;
            }
            return Direction.None;
        }

        private static bool IsFirstCoordinateLeast(params Coordinate2D[] coords)
        {
            Coordinate2D firstCoord = coords[0];
            for (int i = 1; i < coords.Length; i++)
            {
                if (firstCoord > coords[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static IEnumerable<Coordinate2D[]> GetAdjacentCoordinateSets(Coordinate2D[] set, int size, int minimumAdjacent, bool fourNeighborAdjacent)
        {
            Coordinate2D[] coords = new Coordinate2D[size];
            int n = set.Length;
            foreach (int[] combination in CombinationUtils.GetCombinations(set.Length, size))
            {
                // Copy the coordinates in the combination.
                for (int i = 0; i < size; i++)
                {
                    coords[i] = set[combination[i]];
                }

                // Count the number of adjacent pairs.
                int adjacent = 0;
                if (minimumAdjacent > 0)
                {
                    for (int i = 0; i < size - 1; i++)
                    {
                        Coordinate2D coord1 = coords[i];
                        for (int j = i + 1; j < size; j++)
                        {
                            Coordinate2D coord2 = coords[j];
                            int distance = fourNeighborAdjacent ?
                                Coordinate2D.GetOrthogonalDistance(coord1, coord2) :
                                Coordinate2D.GetDiagonalDistance(coord1, coord2);
                            if (distance == 1)
                            {
                                adjacent++;
                            }
                        }
                    }
                }

                // Verify minimum adjacency.
                if (adjacent >= minimumAdjacent)
                {
                    yield return coords;
                }
            }
        }
    }
}
