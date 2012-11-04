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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Sokoban.Engine.Utilities
{
    /// <summary>
    /// Implements a <see cref="TextWriter"/> for writing information to the debugger log.
    /// </summary>
    /// <seealso cref="Debugger.Log"/>
    public class DebuggerWriter : TextWriter
    {
        private bool isOpen;
        private static UnicodeEncoding encoding;
        private readonly int level;
        private readonly string category;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebuggerWriter"/> class.
        /// </summary>
        public DebuggerWriter()
            : this(0, Debugger.DefaultCategory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DebuggerWriter"/> class with the specified level and category.
        /// </summary>
        /// <param name="level">A description of the importance of the messages.</param>
        /// <param name="category">The category of the messages.</param>
        public DebuggerWriter(int level, string category)
            : this(level, category, CultureInfo.CurrentCulture)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DebuggerWriter"/> class with the specified level, category and format provider.
        /// </summary>
        /// <param name="level">A description of the importance of the messages.</param>
        /// <param name="category">The category of the messages.</param>
        /// <param name="formatProvider">An <see cref="IFormatProvider"/> object that controls formatting.</param>
        public DebuggerWriter(int level, string category, IFormatProvider formatProvider)
            : base(formatProvider)
        {
            this.level = level;
            this.category = category;
            this.isOpen = true;
        }

        protected override void Dispose(bool disposing)
        {
            isOpen = false;
            base.Dispose(disposing);
        }

        public override void Write(char value)
        {
            if (!isOpen)
            {
                throw new ObjectDisposedException(null);
            }
            Debugger.Log(level, category, value.ToString());
        }

        public override void Write(string value)
        {
            if (!isOpen)
            {
                throw new ObjectDisposedException(null);
            }
            if (value != null)
            {
                Debugger.Log(level, category, value);
            }
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (!isOpen)
            {
                throw new ObjectDisposedException(null);
            }
            if (buffer == null || index < 0 || count < 0 || buffer.Length - index < count)
            {
                base.Write(buffer, index, count); // delegate throw exception to base class
            }
            Debugger.Log(level, category, new string(buffer, index, count));
        }

        public override Encoding Encoding
        {
            get
            {
                if (encoding == null)
                {
                    encoding = new UnicodeEncoding(false, false);
                }
                return encoding;
            }
        }

        public int Level
        {
            get { return level; }
        }

        public string Category
        {
            get { return category; }
        }
    }
}
