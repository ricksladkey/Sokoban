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

namespace Sokoban.Engine.Collections
{
    /// <summary>
    /// A position set is a collection of positions
    /// that a level can be in.  The boxes are
    /// stored in the level and the sokoban square
    /// can be stored explicitly in the level or
    /// provided implicitly as a method argument.
    /// </summary>
    public interface IPositionSet
    {
        int Count { get; }

        bool Add();
        bool Add(Coordinate2D sokobanCoord);
        bool Contains();
        bool Contains(Coordinate2D sokobanCoord);
    }
}
