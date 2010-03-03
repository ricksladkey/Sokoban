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

namespace Sokoban.Engine.Paths
{
    /// <summary>
    /// A region finder is an object that can enumerate
    /// all the disconnected regions of a level position.
    /// Each region is defined by the lexicographically
    /// first square in the region and the count of
    /// squares accessible from that square.
    /// </summary>
    public abstract class RegionFinder
    {
        protected Level level;
        protected int[][] insideCoordinates;
        protected Coordinate2D[] boxCoordinates;
        protected int boxes;

        public static RegionFinder CreateInstance(Level level)
        {
#if false
            return new BruteForceRegionFinder(level);
#else
            return new IncrementalRegionFinder(level);
#endif
        }

        public RegionFinder(Level level)
        {
            this.level = level;
            this.insideCoordinates = level.InsideCoordinates;
            this.boxCoordinates = level.BoxCoordinates;
            this.boxes = boxCoordinates.Length;
        }

        public abstract IEnumerable<Region> Regions { get; }

        public virtual IEnumerable<Coordinate2D> Coordinates
        {
            get
            {
                foreach (Region region in Regions)
                {
                    yield return region.Coordinate;
                }
            }
        }
    }
}
