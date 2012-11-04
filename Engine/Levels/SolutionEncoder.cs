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

using Sokoban.Engine.Core;

namespace Sokoban.Engine.Levels
{
    public static class SolutionEncoder
    {
        public static bool IsSolution(string text)
        {
            return !text.Contains("#");
        }

        public static MoveList MoveList(string solution)
        {
            MoveList moveList = new MoveList();
            bool isPull = false;
            string digits = "";
            string moves = null;
            for (int i = 0; i < solution.Length; i++)
            {
                char c = solution[i];
                if (Char.IsWhiteSpace(c))
                {
                    continue;
                }
                if (c == '-')
                {
                    isPull = true;
                    continue;
                }
                if (Char.IsDigit(c))
                {
                    digits += c;
                    continue;
                }
                if (c == '(')
                {
                    moves = "";
                    continue;
                }
                if (moves != null && c != ')')
                {
                    moves += c;
                    continue;
                }
                if (c != ')')
                {
                    moves = c.ToString();
                }
                int count = String.IsNullOrEmpty(digits) ? 1 : Int32.Parse(digits);
                for (int j = 0; j < count; j++)
                {
                    for (int k = 0; k < moves.Length; k++)
                    {
                        c = moves[k];
                        Operation operation = DecodeOperation(c);
                        Direction direction = DecodeDirection(c);
                        if (isPull && operation == Operation.Push)
                        {
                            operation = Operation.Pull;
                        }
                        moveList.Add(new OperationDirectionPair(operation, direction));
                    }
                }
                digits = "";
                moves = null;
                isPull = false;
            }
            return moveList;
        }

        private static Operation DecodeOperation(char move)
        {
            return Char.IsUpper(move) ? Operation.Push : Operation.Move;
        }

        private static Direction DecodeDirection(char move)
        {
            move = Char.ToLower(move);
            if (move == 'u')
            {
                return Direction.Up;
            }
            else if (move == 'd')
            {
                return Direction.Down;
            }
            else if (move == 'l')
            {
                return Direction.Left;
            }
            else if (move == 'r')
            {
                return Direction.Right;
            }
            throw new InvalidOperationException("Invalid direction");
        }

        public static string EncodedSolution(MoveList moveList)
        {
            StringBuilder builder = new StringBuilder(moveList.Count);
            for (int i = 0; i < moveList.Count; i++)
            {
                builder.Append(EncodeMove(moveList[i]));
            }
            return builder.ToString();
        }

        private static string EncodeMove(OperationDirectionPair pair)
        {
            string result;
            if (pair.Direction == Direction.Up)
            {
                result = "u";
            }
            else if (pair.Direction == Direction.Down)
            {
                result = "d";
            }
            else if (pair.Direction == Direction.Left)
            {
                result = "l";
            }
            else if (pair.Direction == Direction.Right)
            {
                result = "r";
            }
            else
            {
                throw new InvalidOperationException("Invalid direction");
            }
            if (pair.Operation != Operation.Move)
            {
                result = result.ToUpper();
            }
            if (pair.Operation == Operation.Pull)
            {
                result = "-" + result;
            }
            return result;
        }
    }
}
