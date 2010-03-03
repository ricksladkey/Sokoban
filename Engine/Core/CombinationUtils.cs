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
    public static class CombinationUtils
    {
        private class CombinationState
        {
            public int N;
            public int K;
            public int Index;
            public int Next;
            public int[] Combination;
        }

        public static int Choose(int n, int k)
        {
            if (n < k)
            {
                return 0;
            }
            if (n == k)
            {
                return 1;
            }

            int delta;
            int max;

            if (k < n - k)
            {
                // For example, Choose(100, 3).
                delta = n - k;
                max = k;
            }
            else
            {
                // For example, Choose(100, 97).
                delta = k;
                max = n - k;
            }

            int result = delta + 1;
            for (int i = 2; i <= max; i++)
            {
                checked
                {
                    result = (result * (delta + i)) / i;
                }
            }

            return result;
        }

        public static IEnumerable<int[]> GetCombinations(int n, int k)
        {
            CombinationState state = new CombinationState();
            state.N = n;
            state.K = k;
            state.Combination = new int[k];
            return GetCombinations(state);
        }

        private static IEnumerable<int[]> GetCombinations(CombinationState state)
        {
            // Check recursion termination condition.
            if (state.Index == state.K - 1)
            {
                int limit = state.N;
                for (int i = state.Next; i < limit; i++)
                {
                    // Add the current coordinate.
                    state.Combination[state.Index] = i;

                    // Return the combination.
                    yield return state.Combination;
                }
            }
            else
            {
                // Record old position.
                int oldNext = state.Next;
                int oldIndex = state.Index;
                ++state.Index;
                int limit = state.N - (state.K - state.Index);
                for (int i = oldNext; i < limit; i++)
                {
                    // Add the current coordinate.
                    state.Combination[oldIndex] = i;
                    state.Next = i + 1;

                    // And recurse.
                    foreach (int[] combination in GetCombinations(state))
                    {
                        yield return combination;
                    }
                }
                --state.Index;
                state.Next = oldNext;
            }
        }
    }
}
