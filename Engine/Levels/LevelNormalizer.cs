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
using Sokoban.Engine.Paths;

namespace Sokoban.Engine.Levels
{
    public class LevelNormalizer
    {
        private struct PatternReplacementRecipe
        {
            public bool Rotate;
            public bool Mirror;
            public string Pattern;
            public string Replacement;

            public PatternReplacementRecipe(bool rotate, bool mirror, string pattern, string replacment)
            {
                Rotate = rotate;
                Mirror = mirror;
                Pattern = pattern;
                Replacement = replacment;
            }
        }

        private struct PatternReplacementPair
        {
            public Level Pattern;
            public Level Replacement;

            public PatternReplacementPair(Level pattern, Level replacment)
            {
                Pattern = pattern;
                Replacement = replacment;
            }
        }

        private static string tunnelPatternString = "#####|##-##|##-##|#####";

        private static PatternReplacementRecipe[] replacmentStringTable = new PatternReplacementRecipe[]
        {

            // These are all now done by loop normalization.
#if false
            // Shorten long loops.
            new PatternReplacementRecipe(true, false,
                "#######|#?###?#|##---##|##-#-##|##?#?##|#######",
                "#######|#?###?#|#######|##---##|##?#?##|#######"
            ),

            // Soften sharp corners.
            new PatternReplacementRecipe(true, false,
                "######|#?##?#|##--?#|##-###|#??#?#|######",
                "######|#?##?#|###-?#|##--##|#??#?#|######"
            ),

            // Fill hollow corners.
            new PatternReplacementRecipe(true, false,
                "######|#?##?#|##--?#|##--##|#??#?#|######",
                "######|#?##?#|###-?#|##--##|#??#?#|######"
            ),
#endif

            // Normalize the orientation of free six square caves.
            new PatternReplacementRecipe(true, false,
                "#######|#?###?#|##--###|##--###|##--###|#?#?#?#|#######",
                "#######|#?###?#|###--##|###--##|###--##|#?#?#?#|#######"
            ),

            // Shorten six square caves.
            new PatternReplacementRecipe(true, true,
                "######|#?##?#|##--##|##--##|##--##|##?###|#??#?#|######",
                "######|#?##?#|######|##--##|##--##|##?-##|#??#?#|######"
            ),

            // Convert over-sized one-box caves into six square caves.
            new PatternReplacementRecipe(true, true,
                "#######|#?###?#|##---##|##-#-##|##---##|#??##?#|#######",
                "#######|#?###?#|##--###|##--###|##--###|#??##?#|#######"
            ),
            new PatternReplacementRecipe(true, true,
                "########|#?####?#|###---##|##----##|##--##?#|#?#????#|########",
                "########|#?####?#|##--####|##--####|##--##?#|#?#????#|########"
            ),
            new PatternReplacementRecipe(true, true,
                "########|#??###?#|#?##--##|##----##|##----##|#?#?##?#|########",
                "########|#??###?#|#?#--###|###--###|###--###|#?#?##?#|########"
            ),

        };

        private static Level tunnelPatternDown;
        private static Level tunnelPatternRight;
        private static PatternReplacementPair[] replacementTable;
        private static bool patternsInitialized;

        private static void InitializePatterns()
        {
            if (patternsInitialized)
            {
                return;
            }

            tunnelPatternDown = new Level(tunnelPatternString);
            tunnelPatternRight = LevelUtils.GetRotatedLevel(tunnelPatternDown);

            List<PatternReplacementPair> table = new List<PatternReplacementPair>();
            foreach (PatternReplacementRecipe recipe in replacmentStringTable)
            {
                Level pattern = new Level(recipe.Pattern);
                Level replacement = new Level(recipe.Replacement);
                table.Add(new PatternReplacementPair(pattern, replacement));

                if (recipe.Rotate)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        pattern = LevelUtils.GetRotatedLevel(pattern);
                        replacement = LevelUtils.GetRotatedLevel(replacement);
                        table.Add(new PatternReplacementPair(pattern, replacement));
                    }
                    pattern = LevelUtils.GetRotatedLevel(pattern);
                    replacement = LevelUtils.GetRotatedLevel(replacement);
                }

                if (recipe.Mirror)
                {
                    pattern = LevelUtils.GetMirroredLevel(pattern);
                    replacement = LevelUtils.GetMirroredLevel(replacement);
                    table.Add(new PatternReplacementPair(pattern, replacement));

                    if (recipe.Rotate)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            pattern = LevelUtils.GetRotatedLevel(pattern);
                            replacement = LevelUtils.GetRotatedLevel(replacement);
                            table.Add(new PatternReplacementPair(pattern, replacement));
                        }
                    }
                }
            }

            replacementTable = table.ToArray();
            patternsInitialized = true;
        }

        private Level level;

        public LevelNormalizer(Level level)
        {
            this.level = level;
        }

        public Level Normalize()
        {
            InitializePatterns();

            // Create a copy and fill exterior with walls.
            Level newLevel = new Level(level);
            foreach (Coordinate2D coord in newLevel.Coordinates)
            {
                if (level.IsOutside(coord))
                {
                    newLevel[coord] = Cell.Wall;
                }
            }

            Level testLevel = null;
            bool keepTrying = true;
            while (keepTrying)
            {
                keepTrying = false;

                // Do substitutions dependent on matching.
                foreach (Coordinate2D coord in newLevel.Coordinates)
                {
                    for (int i = 0; i < replacementTable.Length; i++)
                    {
                        PatternReplacementPair pair = replacementTable[i];

                        if (LevelUtils.MatchesPattern(newLevel, coord, pair.Pattern))
                        {
                            newLevel = LevelUtils.Replace(newLevel, coord.Row, coord.Column, pair.Replacement);
                            keepTrying = true;
                            break;
                        }
                    }
                    if (keepTrying)
                    {
                        break;
                    }

                    // Try to shorten vertical tunnels.
                    if (LevelUtils.MatchesPattern(newLevel, coord, tunnelPatternDown))
                    {
                        // Note the tunnel is offset right relative to the pattern.
                        testLevel = TryNormalizeTunnel(newLevel, coord + Direction.Right, Direction.Down);
                        if (testLevel != null)
                        {
                            newLevel = testLevel;
                            keepTrying = true;
                            break;
                        }
                    }

                    // Try to shorten horizontal tunnels.
                    if (LevelUtils.MatchesPattern(newLevel, coord, tunnelPatternRight))
                    {
                        // Note the tunnel is offset down relative to the pattern.
                        testLevel = TryNormalizeTunnel(newLevel, coord + Direction.Down, Direction.Right);
                        if (testLevel != null)
                        {
                            newLevel = testLevel;
                            keepTrying = true;
                            break;
                        }
                    }
                }
                if (keepTrying)
                {
                    continue;
                }

                // If no patterns matched, then try to normalize loops.
                testLevel = TryNormalizeLoops(newLevel);
                if (testLevel != null)
                {
                    newLevel = testLevel;
                    keepTrying = true;
                    continue;
                }
            }

            return LevelUtils.TrimLevel(newLevel);
        }

        private static Level TryNormalizeTunnel(Level level, Coordinate2D tunnelCoord, Direction direction)
        {
            // Determine the offsets of the parallel direction.
            Coordinate2D parallel = direction;

            // If the upper half is closed off it's not a tunnel.
            if (level.IsWall(tunnelCoord - parallel))
            {
                return null;
            }

            // Empty out the level.
            Level newLevel = LevelUtils.GetEmptyLevel(level);

            // Add a wall to separate the level into two halves.
            newLevel[tunnelCoord] = Cell.Wall;

            // Find all cells accessible from the upper half.
            PathFinder finder = PathFinder.CreateInstance(newLevel);
            finder.Find(tunnelCoord - parallel);

            // If the lower half is accessible from the upper half we can't normalize.
            if (finder.IsAccessible(tunnelCoord + parallel))
            {
                return null;
            }

            // Create a map of the two regions, initialized to zero.
            Array2D<int> regionMap = new Array2D<int>(newLevel.Height, newLevel.Width);

            // Mark first region in the map.
            foreach (Coordinate2D coord in finder.AccessibleCoordinates)
            {
                regionMap[coord] = 1;
            }

            // Find all cells accessible from the lower half.
            finder.Find(tunnelCoord + parallel);

            // Mark second region in the map.
            foreach (Coordinate2D coord in finder.AccessibleCoordinates)
            {
                regionMap[coord] = 2;
            }

            // See if normalization would result in overlap.
            foreach (Coordinate2D coord in newLevel.Coordinates)
            {
                // For all coordinates in region one.
                if (regionMap[coord] != 1)
                {
                    continue;
                }

                if (coord == tunnelCoord - parallel)
                {
                    // We will have intentional contact right at the match site.
                    continue;
                }

                // Check whether there would be a square in region 1 adjacent to a square in region 2.
                Coordinate2D newCoord = coord + parallel;
                foreach (Coordinate2D neighbor in newCoord.FourNeighbors)
                {
                    if (newLevel.IsValid(neighbor) && regionMap[neighbor] == 2)
                    {
                        // Normalizing would alter the level.
                        return null;
                    }
                }
            }

            // Normalize the level using the region map as our guide.
            Array2D<Cell> data = new Array2D<Cell>(level.Height, level.Width);
            data.SetAll(Cell.Wall);
            foreach (Coordinate2D coord in level.Coordinates)
            {
                if (regionMap[coord] == 1)
                {
                    data[coord] = level[coord];
                }
                else if (regionMap[coord] == 2)
                {
                    data[coord - parallel] = level[coord];
                }
            }

            // Construct the level.
            return new Level(data);
        }

        private static Level TryNormalizeLoops(Level level)
        {
            // A loop is a tunnel that you cannot push a box through.
            // Any square that it is impossible or not useful to push
            // a box to is a no-box square.  All the squares in a loop
            // are no-box squares.  A level may have several disconnected
            // no-box regions.  Within one no-box region we can
            // optimize the path by removing islands that only
            // lengthen the loop without changing its function.
            // Finally, a normalized loop only needs to be one square
            // wide.

            // Copy the level and get island and no-box maps.
            bool modifiedLevel = false;
            Level newLevel = new Level(level);
            Array2D<bool> islandMap = LevelUtils.GetIslandMap(level);
            Array2D<int> noBoxMap = LevelUtils.GetNoBoxMap(level);

            // Any island that only contacts other walls and one or more
            // no-box squares in the same region can be removed.
            foreach (Coordinate2D coord in newLevel.Coordinates)
            {
                if (islandMap[coord])
                {
                    int wallCount = 0;
                    int noBoxCount = 0;
                    int region = 0;
                    foreach (Coordinate2D neighbor in coord.FourNeighbors)
                    {
                        if (newLevel.IsWall(neighbor))
                        {
                            wallCount++;
                        }
                        else if (noBoxMap[neighbor] != 0)
                        {
                            // Check whether we've encountered any no-box regions.
                            if (region == 0)
                            {
                                region = noBoxMap[neighbor];
                            }

                            // Only count no-box squares that match the first region.
                            if (noBoxMap[neighbor] == region)
                            {
                                noBoxCount++;
                            }
                        }
                    }
                    if (wallCount + noBoxCount == 4 && noBoxCount >= 1)
                    {
                        newLevel[coord] = Cell.Empty;
                        noBoxMap[coord] = 1;
                        islandMap[coord] = false;
                        modifiedLevel = true;
                    }
                }
            }

            // Make a map of all the inside squares we want to keep, initialized to false.
            Array2D<bool> keepMap = new Array2D<bool>(newLevel.Height, newLevel.Width);
            foreach (Coordinate2D coord in newLevel.InsideCoordinates)
            {
                if (noBoxMap[coord] != 0)
                {
                    // Keep all the no-box squares that contact box islands.
                    foreach (Coordinate2D neighbor in coord.EightNeighbors)
                    {
                        if (newLevel.IsValid(neighbor) && islandMap[neighbor])
                        {
                            keepMap[coord] = true;
                            break;
                        }
                    }

                    // Keep all the no-box squares that contact box squares.
                    foreach (Coordinate2D neighbor in coord.FourNeighbors)
                    {
                        if (newLevel.IsFloor(neighbor) && noBoxMap[neighbor] == 0)
                        {
                            keepMap[coord] = true;
                            break;
                        }
                    }
                }
                else
                {
                    // Keep all the box squares.
                    keepMap[coord] = true;
                }
            }

            // Fill in the no-box squares that we can safely remove.
            Coordinate2D sokobanCoord = newLevel.SokobanCoordinate;
            foreach (Coordinate2D coord in newLevel.InsideCoordinates)
            {
                if (!keepMap[coord])
                {
                    newLevel[coord] = Cell.Wall;
                    modifiedLevel = true;
                }
            }

            // If the sokoban was on one of the cells we didn't keep,
            // move it to a nearby box square.
            if (!newLevel.IsSokoban(sokobanCoord))
            {
                Level boxLevel = FillBoxSquaresWithBoxes(level, noBoxMap);
                PathFinder finder = PathFinder.CreateInstance(boxLevel);
                finder.Find(sokobanCoord);
                foreach (Coordinate2D coord in finder.AccessibleCoordinates)
                {
                    if (newLevel.IsEmpty(coord))
                    {
                        newLevel[coord] = Cell.Sokoban;
                        break;
                    }
                }
            }

            // Return the new level only if we made changes.
            if (modifiedLevel)
            {
                return newLevel;
            }

            return null;
        }

        private static Level FillBoxSquaresWithBoxes(Level level, Array2D<int> noBoxMap)
        {
            Level boxLevel = new Level(level);
            foreach (Coordinate2D coord in boxLevel.Coordinates)
            {
                if (Level.IsFloor(boxLevel[coord]))
                {
                    boxLevel[coord] = noBoxMap[coord] == 0 ? Cell.Box : Cell.Empty;
                }
            }
            return boxLevel;
        }
    }
}
