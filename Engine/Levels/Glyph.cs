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

namespace Sokoban.Engine.Levels
{
    public static class Glyph
    {
        public static readonly char EmptyFloor = ' ';
        public static readonly char SokobanOnFloor = '@';
        public static readonly char BoxOnFloor = '$';

        public static readonly char EmptyTarget = '.';
        public static readonly char SokobanOnTarget = '+';
        public static readonly char BoxOnTarget = '*';

        public static readonly char Wall = '#';
        public static readonly char Undefined = '?';

        public static readonly char EmptyFloor2 = '-';
        public static readonly char EmptyFloor3 = '_';

        public static readonly char Invalid = 'X';
    }
}
