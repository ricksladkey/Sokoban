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
using System.Runtime.InteropServices;
using System.Security;

namespace Sokoban.Engine.Utilities
{
    public struct TimeSnapshot
    {
        public DateTime RealTime;
        public TimeSpan TotalTime;
        public TimeSpan SystemTime;
        public long PerformanceCounter;

        public static TimeSnapshot Now
        {
            get
            {
                TimeSnapshot snapshot = new TimeSnapshot();
                snapshot.RealTime = DateTime.Now;
                snapshot.TotalTime = Process.GetCurrentProcess().TotalProcessorTime;
                snapshot.SystemTime = Process.GetCurrentProcess().PrivilegedProcessorTime;
                long performanceCounter;
                QueryPerformanceCounter(out performanceCounter);
                snapshot.PerformanceCounter = performanceCounter;
                return snapshot;
            }
        }

        [DllImport("Kernel32.dll")]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool QueryPerformanceCounter(out long performanceCounter);
    }
}
