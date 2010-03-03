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
using NUnit.Framework;

using Sokoban.Engine.Collections;
using Sokoban.Engine.Core;
using Sokoban.Engine.Deadlocks;
using Sokoban.Engine.Levels;
using Sokoban.Engine.Paths;
using Sokoban.Engine.Solvers;
using Sokoban.Engine.Utilities;

namespace Sokoban.UnitTests
{
    [TestFixture]
    public class CoreTests
    {
        [Test]
        public void HashtableKeysTest()
        {
            Hashtable<int, string> hashtable = new Hashtable<int, string>();
            hashtable.Add(0, "abc");
            hashtable.Add(1, "def");
            hashtable.Add(2, "ghi");
            IEnumerable<int> keys1 = hashtable.Keys;
            foreach (int key in keys1)
            {
                Console.WriteLine("1: key: {0}", key);
            }
            foreach (int key in keys1)
            {
                Console.WriteLine("2: key: {0}", key);
            }
            hashtable.Add(3, "jkl");
            foreach (int key in keys1)
            {
                Console.WriteLine("3: key: {0}", key);
            }
        }
    }
}
