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
using System.Diagnostics;

using Sokoban.Engine.Core;

namespace Sokoban.Engine.Levels
{
    public static class LevelEncoder
    {
        private class GlyphMap : Hashtable<int, int>
        {
            public GlyphMap()
                : base(100)
            {
            }

            public void Add(char glyph, Cell cell)
            {
                base.Add((int)glyph, (int)cell);
            }
            public bool ContainsKey(char glyph)
            {
                return base.ContainsKey((int)glyph);
            }
            public Cell this[char glyph]
            {
                get
                {
                    return (Cell)base[(int)glyph];
                }
            }
        }

        private class CellMap : Hashtable<int, int>
        {
            public CellMap()
                : base(100)
            {
            }

            public void Add(Cell cell, char glyph)
            {
                base.Add((int)cell, (int)glyph);
            }
            public bool ContainsKey(Cell cell)
            {
                return base.ContainsKey((int)cell);
            }
            public char this[Cell cell]
            {
                get
                {
                    return (char)base[(int)cell];
                }
            }
        }

        private static GlyphMap glyphMap;
        private static CellMap cellMap;

        static LevelEncoder()
        {
            glyphMap = new GlyphMap();

            glyphMap.Add(Glyph.EmptyFloor, Cell.Empty);
            glyphMap.Add(Glyph.SokobanOnFloor, Cell.Sokoban);
            glyphMap.Add(Glyph.BoxOnFloor, Cell.Box);
            glyphMap.Add(Glyph.EmptyTarget, Cell.Target);
            glyphMap.Add(Glyph.SokobanOnTarget, Cell.Sokoban | Cell.Target);
            glyphMap.Add(Glyph.BoxOnTarget, Cell.Box | Cell.Target);
            glyphMap.Add(Glyph.Wall, Cell.Wall);
            glyphMap.Add(Glyph.Undefined, Cell.Undefined);

            glyphMap.Add(Glyph.EmptyFloor2, Cell.Empty);
            glyphMap.Add(Glyph.EmptyFloor3, Cell.Empty);

            cellMap = new CellMap();

            cellMap.Add(Cell.Empty, Glyph.EmptyFloor);
            cellMap.Add(Cell.Sokoban, Glyph.SokobanOnFloor);
            cellMap.Add(Cell.Box, Glyph.BoxOnFloor);
            cellMap.Add(Cell.Target, Glyph.EmptyTarget);
            cellMap.Add(Cell.Sokoban | Cell.Target, Glyph.SokobanOnTarget);
            cellMap.Add(Cell.Box | Cell.Target, Glyph.BoxOnTarget);
            cellMap.Add(Cell.Wall, Glyph.Wall);
            cellMap.Add(Cell.Undefined, Glyph.Undefined);
        }

        private static Cell GlyphToCell(char glyph)
        {
            return glyphMap[glyph];
        }

        private static char CellToGlyph(Cell cell)
        {
            Cell key = cell & ~(Cell.Accessible | Cell.Outside);
            return cellMap.ContainsKey(key) ? cellMap[key] : Glyph.Invalid;
        }

        private static string GlyphRow(Array2D<Cell> data, int row)
        {
            StringBuilder result = new StringBuilder();
            for (int column = 0; column < data.Width; column++)
            {
                result.Append(CellToGlyph(data[row, column]));
            }
            return result.ToString();
        }

        private static void CellRow(Array2D<Cell> data, int row, string glyphs)
        {
            int width = data.Width;
            glyphs = glyphs.PadRight(width);
            for (int w = 0; w < width; w++)
            {
                data[row, w] = GlyphToCell(glyphs[w]);
            }
        }

        public static Array2D<Cell> DecodeLevel(IEnumerable<string> lines)
        {
            int height = 0;
            int width = 0;
            foreach (string line in lines)
            {
                height++;
                width = Math.Max(width, line.Length);
            }

            Array2D<Cell> data = new Array2D<Cell>(height, width);
            int h = 0;
            foreach (string line in lines)
            {
                string row = line.PadRight(width);
                CellRow(data, h, row);
                h++;
            }
            return data;
        }

        public static Array2D<Cell> DecodeLevel(string level)
        {
            List<string> lines = new List<string>();
            int count = 0;
            StringBuilder line = new StringBuilder();
            int n = level.TrimEnd('\n').Length;
            for (int i = 0; i < n; i++)
            {
                char c = level[i];
                if (c == '\r')
                {
                    continue;
                }
                if (Char.IsDigit(c))
                {
                    count = count * 10 + (c - '0');
                    continue;
                }
                if (c == '|' || c == '\n')
                {
                    lines.Add(line.ToString());
                    line.Remove(0, line.Length);
                    continue;
                }
                if (count == 0)
                {
                    count = 1;
                }
                for (int j = 0; j < count; j++)
                {
                    line.Append(c);
                }
                count = 0;
            }
            lines.Add(line.ToString());

            return DecodeLevel(lines);
        }

        public static List<string> EncodeLevel(Array2D<Cell> data)
        {
            return EncodeLevel(data, true);
        }

        public static List<string> EncodeLevel(Array2D<Cell> data, bool trimEnd)
        {
            List<string> lines = new List<string>();
            for (int row = 0; row < data.Height; row++)
            {
                string line = GlyphRow(data, row);
                if (trimEnd)
                {
                    line = line.TrimEnd();
                }
                lines.Add(line);
            }
            return lines;
        }

        public static string EncodeLevel(Array2D<Cell> data, string separator)
        {
            return Concat(EncodeLevel(data), separator);
        }

        public static string Concat(IEnumerable<string> strings, string separator)
        {
            string result = "";
            foreach (string line in strings)
            {
                result += String.Format("{0}{1}", line.TrimEnd(), separator);
            }

            return result;
        }

        public static string CombineLevels(string level1, string level2)
        {
            char separator = '\n';
            string[] lines1 = level1.Split(separator);
            string[] lines2 = level2.Split(separator);
            int longest = 0;
            for (int i = 0; i < lines1.Length - 1; i++)
            {
                lines1[i] = lines1[i].TrimEnd();
                lines2[i] = lines2[i].TrimEnd();
            }
            foreach (string line in lines1)
            {
                longest = Math.Max(longest, line.Length);
            }
            string result = "";
            for (int i = 0; i < lines1.Length - 1; i++)
            {
                result += String.Format("{0}{1}{2}", lines1[i].PadRight(longest + 1), lines2[i], separator);
            }
            return result;
        }
    }
}
