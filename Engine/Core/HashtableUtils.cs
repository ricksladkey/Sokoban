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

namespace Sokoban.Engine.Core
{
    public sealed class HashtableUtils
    {
        public static int NextPrime(int value)
        {
            int candidate = value;
            if (candidate % 2 == 0)
            {
                candidate++;
            }
            while (true)
            {
                int limit = (int)Math.Ceiling(Math.Sqrt(candidate));
                bool isPrime = true;
                for (int divisor = 3; divisor <= limit; divisor += 2)
                {
                    if (candidate % divisor == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }
                if (isPrime)
                {
                    break;
                }
                candidate += 2;
            }
            return candidate;
        }

    }
}
