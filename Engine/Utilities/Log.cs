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

namespace Sokoban.Engine.Utilities
{
    public static class Log
    {
        private static TextWriter logOut = new DebuggerWriter();
        private static bool logToConsole = false;
        private static string logFile = null;

        public static TextWriter LogOut
        {
            get
            {
                return logOut;
            }
            set
            {
                logOut = value;
            }
        }

        public static bool LogToConsole
        {
            get
            {
                return logToConsole;
            }
            set
            {
                logToConsole = value;
            }
        }

        public static string LogFile
        {
            get
            {
                return logFile;
            }
            set
            {
                logFile = value;
            }
        }

        public static void Write(string format, params object[] args)
        {
            LogOut.Write(format, args);

            if (logToConsole)
            {
                Console.Write(format, args);
            }

            if (logFile != null)
            {
                string text = String.Format(format, args);
                try
                {
                    using (TextWriter writer = File.AppendText(logFile))
                    {
                        writer.Write(text);
                    }
                }
                catch (Exception ex)
                {
                    LogOut.WriteLine("Exception writing to log: {0}", ex.Message);
                }
            }
        }

        public static void WriteLine(string format, params object[] args)
        {
            LogOut.WriteLine(format, args);

            if (logToConsole)
            {
                Console.WriteLine(format, args);
            }

            if (logFile != null)
            {
                string text = String.Format(format, args);
                try
                {
                    using (TextWriter writer = File.AppendText(logFile))
                    {
                        writer.WriteLine(text);
                    }
                }
                catch (Exception ex)
                {
                    LogOut.WriteLine("Exception writing to log: {0}", ex.Message);
                }
            }
        }

        public static void DebugPrint(string format, params object[] args)
        {
            WriteLine(format, args);
        }
    }
}
