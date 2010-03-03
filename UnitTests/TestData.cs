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

namespace Sokoban.UnitTests
{
    class TestData
    {
        public const string DataDirectory = @"..\..\LevelSets\";

        public const string ClaireLevel = "7#|#.@-#-#|#$*-$-#|#3-$-#|#-..--#|#--*--#|7#";

        public const string MinicosmosLevelSetFile = "minicosmos.txt";

        public const string MicrocosmosLevelSetFile = "microcosmos.txt";

        public const string NabokosmosLevelSetFile = "nabokosmos.txt";

        public static int[] MinicosmosPushesExpectedMoves =
        {
            37, 60, 75, 71, 104, 99, 67, 93, 85, 102, 74, 140, 82, 125, 82, 114, 66, 110, 72, 112, 71, 99, 125, 193, 81, 133, 103, 189, 58, 174, 74, 91, 94, 101, 88, 72, 98, 50, 116, 84
        };

        public static int[] MinicosmosPushesExpectedPushes =
        {
            6, 10, 10, 12, 26, 29, 17, 26, 13, 14, 15, 22, 28, 37, 24, 35, 15, 27, 25, 41, 26, 40, 29, 52, 22, 48, 26, 38, 13, 45, 14, 20, 19, 21, 16, 22, 18, 17, 21, 17
        };

        public static int[] MinicosmosMovesExpectedMoves =
        {
            37, 60, 69, 71, 104, 99, 61, 93, 85, 102, 74, 112, 80, 121, 82, 114, 65, 110, 72, 112, 71, 99, 99, 177, 81, 133, 103, 189, 58, 168, 74, 91, 94, 101, 88, 72, 96, 50, 100, 84
        };

        public static int[] MicrocosmosPushesExpectedMoves =
        {
            49, 211, 123, 107, 116, 65, 110, 89, 209, 127, 149, 67, 146, 164, 139, 123, 188, 147, 124, 146, 185, 188, 91, 165, 146, 176, 250, 73, 83, 161, 101, 82, 201, 100, 70, 98, 194, 188, 151, 156
        };

        public static int[] MicrocosmosPushesExpectedPushes =
        {
            13, 82, 29, 32, 30, 21, 23, 26, 64, 24, 28, 21, 20, 46, 51, 44, 56, 42, 29, 42, 28, 55, 17, 31, 38, 53, 43, 23, 24, 40, 18, 20, 37, 29, 25, 28, 49, 46, 24, 28
        };

        public static int[] MicrocosmosMovesExpectedMoves =
        {
            49, 211, 123, 107, 116, 65, 110, 89, 209, 117, 125, 67, 128, 164, 139, 119, 188, 147, 124, 146, 185, 170, 91, 165, 130, 176, 234, 73, 83, 161, 101, 82, 173, 100, 70, 98, 184, 188, 151, 150
        };
    }
}
