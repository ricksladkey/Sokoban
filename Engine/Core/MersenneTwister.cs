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
    // Reference: http://en.wikipedia.org/wiki/Mersenne_twister

    //// Create a length 624 array to store the state of the generator
    // int[0..623] MT
    // int index = 0
     
    // // Initialize the generator from a seed
    // function initializeGenerator(int seed) {
    //     MT[0] := seed
    //     for i from 1 to 623 { // loop over each other element
    //         MT[i] := last 32 bits of(1812433253 * (MT[i-1] xor (right shift by 30 bits(MT[i-1]))) + i) // 0x6c078965
    //     }
    // }
     
    // // Extract a tempered pseudorandom number based on the index-th value,
    // // calling generateNumbers() every 624 numbers
    // function extractNumber() {
    //     if index == 0 {
    //         generateNumbers()
    //     }
         
    //     int y := MT[index]
    //     y := y xor (right shift by 11 bits(y))
    //     y := y xor (left shift by 7 bits(y) and (2636928640)) // 0x9d2c5680
    //     y := y xor (left shift by 15 bits(y) and (4022730752)) // 0xefc60000
    //     y := y xor (right shift by 18 bits(y))
         
    //     index := (index + 1) mod 624
    //     return y
    // }
     
    // // Generate an array of 624 untempered numbers
    // function generateNumbers() {
    //     for i from 0 to 623 {
    //         int y := 32nd bit of(MT[i]) + last 31 bits of(MT[(i+1) mod 624])
    //         MT[i] := MT[(i + 397) mod 624] xor (right shift by 1 bit(y))
    //         if (y mod 2) == 1 { // y is odd
    //             MT[i] := MT[i] xor (2567483615) // 0x9908b0df
    //         }
    //     }
    // }

    public class MersenneTwister32
    {
        private const int N = 624;
        private UInt32[] MT;
        private int index;

        public MersenneTwister32(UInt32 seed)
        {
            // Create a length 624 array to store the state of the generator.
            MT = new UInt32[N];

            // Initialize the generator from a seed.
            MT[0] = seed;
            for (int i = 1; i < N; i++)
            {
                MT[i] = 0x6c078965u * (MT[i - 1] ^ ((MT[i - 1] >> 30))) + (UInt32)i;
            }

            index = 0;
        }


        public UInt32 Next()
        {
            // Extract a tempered pseudorandom number based on the index-th value,
            // calling GenerateNumbers() every 624 numbers.
            if (index == 0)
            {
                GenerateNumbers();
            }

            UInt32 y = MT[index];
            y ^= y >> 11;
            y ^= (y << 7) & 0x9d2c5680u;
            y ^= (y << 15) & 0xefc60000u;
            y ^= y >> 18;

            index = (index + 1) % N;

            return y;
        }

        private void GenerateNumbers()
        {
            // Generate an array of 624 untempered numbers.
            for (int i = 0; i < N; i++)
            {
                UInt32 y = (0x80000000u & MT[i]) | (0x7fffffffu & MT[(i + 1) % N]);
                MT[i] = MT[(i + 397) % N] ^ (y >> 1);
                if (y % 2 == 1)
                {
                    MT[i] ^= 0x9908b0dfu;
                }
            }
        }
    }

    public class MersenneTwister64
    {
        private MersenneTwister32 mt;

        public MersenneTwister64(UInt32 seed)
        {
            mt = new MersenneTwister32(seed);
        }

        public UInt64 Next()
        {
            return (((UInt64)mt.Next()) << 32) | ((UInt64)mt.Next());
        }
    }
 }
