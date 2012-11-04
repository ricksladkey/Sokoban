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

using Sokoban.Engine.Core;

namespace Sokoban.Engine.Levels
{
    public class LevelSet : List<Level>
    {
        private string name;

        public LevelSet(IEnumerable<Level> levels)
            : base(levels)
        {
        }

        public LevelSet(string filename)
        {
            using (TextReader reader = File.OpenText(filename))
            {
                Read(reader);
            }
        }

        public LevelSet(TextReader reader)
        {
            Read(reader);
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public void SaveAs(string filename)
        {
            using (TextWriter writer = File.CreateText(filename))
            {
                writer.WriteLine("{0}", Name);
                writer.WriteLine();

                foreach (Level level in this)
                {
                    writer.WriteLine(level.AsText);
                }
            }
        }

        private void Read(TextReader reader)
        {
            name = null;
            string levelName = null;

            while (true)
            {
                string line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }

                if (name == null)
                {
                    if (!IsSokobanLine(line))
                    {
                        name = SanitizeSetName(line);
                        continue;
                    }
                    else
                    {
                        name = "Untitled";
                    }
                }

                if (!IsSokobanLine(line))
                {
                    levelName = line;
                    continue;
                }

                List<string> lines = new List<string>();
                while (true)
                {
                    if (line == null)
                    {
                        break;
                    }
                    if (!IsSokobanLine(line))
                    {
                        break;
                    }
                    lines.Add(line);
                    line = reader.ReadLine();
                }

                Level level = new Level(lines);
                level.Name = levelName;
                Add(level);

                if (line == null)
                {
                    break;
                }
            }
        }

        private string SanitizeSetName(string text)
        {
            foreach (string leader in new string[] { "Author:", ";", "//" })
            {
                if (text.StartsWith(leader))
                {
                    return text.Remove(0, leader.Length).Trim();
                }
            }
            return text;
        }

        private bool IsSokobanLine(string line)
        {
            return (line.StartsWith(" ") || line.StartsWith("#")) && line.Contains("#");
        }
    }
}
