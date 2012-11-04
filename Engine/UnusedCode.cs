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

namespace Sokoban.Engine
{

#if false

        public bool IsInsideConnected
        {
            get
            {
                return GetInsideIsConnected();
            }
        }

        private bool GetInsideIsConnected()
        {
            pathFinder.Find(LevelUtils.FindFirstEmpty(level));
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                if (!level.IsBox(coord) && !pathFinder.IsAccessible(coord))
                {
                    return false;
                }
            }
            return true;
        }
#endif

#if false
    public class ReliablePositionSet : PositionSet
    {
        private class PositionComparer : IEqualityComparer<CoordinateKey[]>
        {
            private int boxes;
            private int[] boxCodes;
            private int[] sokobanCodes;

            public PositionComparer(int boxes, int maxKey)
            {
                this.boxes = boxes;

                boxCodes = new int[maxKey];
                sokobanCodes = new int[maxKey];
                MersenneTwister32 random = new MersenneTwister32(0);
                for (int i = 0; i < maxKey; i++)
                {
                    boxCodes[i] = (int)random.Next();
                    sokobanCodes[i] = (int)random.Next();
                }
            }

    #region IEqualityComparer<CoordinateKey[]> Members

            public bool Equals(CoordinateKey[] x, CoordinateKey[] y)
            {
                for (int i = 0; i <= boxes; i++)
                {
                    if (x[i] != y[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            public int GetHashCode(CoordinateKey[] key)
            {
                int hashCode = sokobanCodes[key[boxes]];
                for (int i = 0; i < boxes; i++)
                {
                    hashCode ^= boxCodes[key[i]];
                }
                return hashCode;
            }

    #endregion
        }

        private Level level;
        private Coordinate2D[] boxCoordinates;
        private int boxes;
        private Array2D<CoordinateKey> insideMap;
        private Hashtable<CoordinateKey[], EmptyValue> set;
        private CoordinateKey[] key;

        public ReliablePositionSet(Level level, int capacity)
        {
            this.level = level;

            boxCoordinates = level.BoxCoordinates;
            boxes = level.Boxes;

            insideMap = new Array2D<CoordinateKey>(level.Height, level.Width);
            int insideCount = 0;
            CoordinateKey index = 0;
            foreach (Coordinate2D coord in level.InsideCoordinates)
            {
                insideMap[coord] = index++;
                insideCount++;
            }
            if (insideCount > CoordinateKey.MaxValue)
            {
                throw new Exception("too many inside coordinates");
            }

            IEqualityComparer<CoordinateKey[]> comparer = new PositionComparer(boxes, insideCount);
            set = new Hashtable<CoordinateKey[], EmptyValue>(capacity, comparer);
            key = new CoordinateKey[boxes + 1];
        }

        public override int Count
        {
            get
            {
                return set.Count;
            }
        }

        public override int Capacity
        {
            get
            {
                return set.Capacity;
            }
        }

        public override bool Add(Coordinate2D sokobanCoord)
        {
            GetKey(sokobanCoord);
            if (set.ContainsKey(key))
            {
                return true;
            }
            CoordinateKey[] newKey = new CoordinateKey[boxes + 1];
            key.CopyTo(newKey, 0);
            set.Add(newKey, new EmptyValue());
            return false;
        }

        public override bool Contains(Coordinate2D sokobanCoord)
        {
            GetKey(sokobanCoord);
            return set.ContainsKey(key);
        }

        private void GetKey(Coordinate2D sokobanCoord)
        {
            for (int i = 0; i < boxes; i++)
            {
                CoordinateKey value = insideMap[boxCoordinates[i]];
                int j = i - 1;
                while (j >= 0 && key[j] > value)
                {
                    key[j + 1] = key[j];
                    j--;
                }
                key[j + 1] = value;
            }
            key[boxes] = insideMap[sokobanCoord];
        }
    }
#endif

#if false
        private static Array2D<int[][]> combinationsMap;

        private static void InitializeCombinationsMap(int maxCombinations)
        {
            // Intialize the combinations maps.
            combinationsMap = new Array2D<int[][]>(maxCombinations + 1, maxCombinations);
            for (int n = 3; n <= maxCombinations; n++)
            {
                for (int k = 2; k < n; k++)
                {
                    List<int[]> combinations = new List<int[]>();
                    foreach (int[] combination in CombinationUtils.GetCombinations(n, k))
                    {
                        combinations.Add((int[])combination.Clone());
                    }
                    combinationsMap[n, k] = combinations.ToArray();
                }
            }
        }
#endif

#if false
        private void FindPulls()
        {
            foreach (Coordinate2D coord in level.BoxCoordinates)
            {
                foreach (Direction direction in Direction.Directions)
                {
                    if (level.IsSokobanOrEmpty(coord + direction) && pathFinder.IsAccessible(coord + 2 * direction))
                    {
                        moves.Push(new Move(coord + direction, direction));
                    }
                }
            }
        }
#endif


#if false
        private static IEnumerable<Coordinate2D[]> GetSets(SetState state)
        {
            // Check recursion termination condition.
            if (state.Index == state.K - 1)
            {
                int limit = state.N;
                for (int i = state.Next; i < limit; i++)
                {
                    // Add the current coordinate.
                    state.Coordinates[state.Index] = state.Set[i];

                    // Verify minimum adjacency.
                    if (state.MinimumAdjacent == 0 || CheckAdjacent(state))
                    {
                        yield return state.Coordinates;
                    }
                }
            }
            else
            {
                // Record old position.
                int oldPosition = state.Next;
                int oldIndex = state.Index;
                ++state.Index;
                int limit = state.N - (state.K - state.Index);
                for (int i = oldPosition; i < limit; i++)
                {
                    // Add the current coordinate.
                    state.Coordinates[oldIndex] = state.Set[i];
                    state.Next = i + 1;

                    // Otherwise recurse.
                    foreach (Coordinate2D[] coords in GetSets(state))
                    {
                        yield return coords;
                    }
                }
                --state.Index;
                state.Next = oldPosition;
            }
        }
#endif

#if false
        protected bool IsProperSubsetDeadlocked(bool unconditionally, params Coordinate2D[] coords)
        {
            // Handle all possible subset sizes two or larger.
            int n = coords.Length;
            int limit = coords.Length - 1;
            for (int k = 2; k <= limit; k++)
            {
                int[][] combinations = combinationsMap[n, k];
                Coordinate2D[] subsetCoords = combinationArrays[k];

                foreach (int[] combination in combinations)
                {
                    for (int i = 0; i < k; i++)
                    {
                        subsetCoords[i] = coords[combination[i]];
                    }
                    if (IsSetDeadlocked(unconditionally, subsetCoords))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

#endif

#if false
        protected bool IsProperSubsetDeadlocked(bool unconditionally, params Coordinate2D[] coords)
        {
            // All non-simple deadlocks are by definition two or larger.
            int n = coords.Length;
            if (n < 3)
            {
                return false;
            }

            // Perform a quick check to see if any of the coordinates have deadlocks
            // starting with that coordinate.
            int matches = 0;
            for (int i = 0; i < n - 1; i++)
            {
                if (entriesMap[coords[i]] != null)
                {
                    matches++;
                }
            }
            if (matches == 0)
            {
                return false;
            }

            // Create or re-use a scratch array of pair coordinates.
            if (pairCoords == null)
            {
                pairCoords = new Coordinate2D[2];
            }

            // Iterate over all coordinate pairs.
            for (int firstIndex = 0; firstIndex < n - 1; firstIndex++)
            {
                pairCoords[0] = coords[firstIndex];
                for (int secondIndex = firstIndex + 1; secondIndex < n; secondIndex++)
                {
                    pairCoords[1] = coords[secondIndex];
                    if (IsSetDeadlocked(unconditionally, pairCoords))
                    {
                        return true;
                    }
                }
            }

            // Handle the in-between subset sizes.
            for (int size = 3; size < n - 2; size++)
            {
                foreach (Coordinate2D[] subsetCoords in CoordinateUtils.GetCoordinateSets(coords, size))
                {
                    if (IsSetDeadlocked(unconditionally, subsetCoords))
                    {
                        return true;
                    }
                }
            }

            // Handle the largest subset of the larger sizes. 
            if (n < 4)
            {
                return false;
            }

            // Check whether any proper subset of specified sorted coordinates is
            // presently in the working deadlock entries map.

            // Create or re-use a scratch array of subset coordinates.
            if (oneFewerCoords == null || oneFewerCoords.Length != n - 1)
            {
                oneFewerCoords = new Coordinate2D[n - 1];
            }

            // Omit each coordinate once.
            for (int omittedIndex = 0; omittedIndex < n; omittedIndex++)
            {
                // Populate the subset coordinates skipping one.
                for (int setIndex = 0, subsetIndex = 0; setIndex < n; setIndex++)
                {
                    if (setIndex != omittedIndex)
                    {
                        oneFewerCoords[subsetIndex++] = coords[setIndex];
                    }
                }
                if (IsSetDeadlocked(unconditionally, oneFewerCoords))
                {
                    return true;
                }
            }

            return false;
        }
#endif

#if false
            // Prepare the forward and reverse inside functions.
            count = level.InsideSquares + 1;
            f = new int[m];
            g = new Coordinate2D[count];
            int index = 0;
            foreach (Coordinate2D coord in level.Coordinates)
            {
                if (level.IsInside(coord))
                {
                    // Inside squares map to consecutive
                    // indices for the forward function
                    // and to their own coordinate for
                    // the reverse function.
                    f[coord.Row * n + coord.Column] = index;
                    g[index] = coord;
                    index++;
                }
                else
                {
                    // All non-inside squares map to the last
                    // function element, which in turn points
                    // to a wall.
                    f[coord.Row * n + coord.Column] = count - 1;
                }
            }
            g[count - 1] = LevelUtils.FindFirstWall(level);

#endif

#if false
        public static IEnumerable<Coordinate2D[]> GetFrozenDeadlockSets(int size)
        {
            // This enumerates the correct set but does so very inefficiently.
            return GetDeadlockSets(size, size - 1, true);
        }
#endif

#if false
                bool tryToSolve = true;

                // If the sokoban is trapped in a single square, the
                // only way to get to this position is for it to be
                // a subset of the starting position.  However, this
                // information depends on the starting sokoban square,
                // which makes the whole set much less general.  This
                // same optimization could instead be make at run-time
                // when we know for sure which level we are trying to
                // solve.
                if (accessibleCount == 1)
                {
                    bool isSubset = true;
                    if (sokobanCoord == level.SokobanCoordinate)
                    {
                        foreach (Coordinate2D boxCoord in level.BoxCoordinates)
                        {
                            if (!level.IsBox(boxCoord))
                            {
                                isSubset = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        isSubset = false;
                    }
                    tryToSolve = isSubset;
                }

                if (tryToSolve)
                {
#endif

#if false
        protected bool IsSubsetDeadlocked(bool unconditionally, params Coordinate2D[] coords)
        {
            bool newResult = NewIsSubsetDeadlocked(unconditionally, coords);
            bool oldResult = OldIsSubsetDeadlocked(unconditionally, coords);
            if (newResult != oldResult)
            {
                throw new Exception("new subset not working");
            }
            return newResult;
        }

        protected bool IsSubsetDeadlocked(bool unconditionally, params Coordinate2D[] coords)
        {
            for (int size = coords.Length - 1; size >= 2; size--)
            {
                foreach (Coordinate2D[] subsetCoords in CoordinateSet.GetCoordinateSets(coords, size))
                {
                    if (IsSetDeadlocked(unconditionally, subsetCoords))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected bool OldIsSubsetDeadlocked(bool unconditionally, params Coordinate2D[] coords)
        {
            // For all existing deadlocks shorter than this one.
            int n = coords.Length;
            foreach (Deadlock deadlock in Deadlocks)
            {
                if (deadlock.Coordinates.Length < n)
                {
                    // Compare all coordinates in the existing deadlock
                    // with the coordinates in the new one.
                    int matches = 0;
                    foreach (Coordinate2D subsetCoord in deadlock.Coordinates)
                    {
                        foreach (Coordinate2D setCoord in coords)
                        {
                            if (subsetCoord == setCoord)
                            {
                                matches++;
                            }
                        }
                    }

                    // If we matched all coordinates
                    // then a proper subset is deadlocked.
                    if (matches == deadlock.Coordinates.Length)
                    {
                        // Obey unconditional restrictions.
                        if (!unconditionally || deadlock.SokobanMap == null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

#endif

#if false
        private bool FindChildren(Node node)
        {
            if (optimizePushes ? node.Pushes >= pushLimit : node.Moves >= moveLimit)
            {
                // Once we've found a solution, ignore postions that already meet or exceed its length.
                return true;
            }

            // Check all boxes for possible movement.
            for (int i = 0; i < boxes; i++)
            {
                // Rotate the order with each search so that we don't
                // always search the same box first.
                int ii = (i + boxStart) % boxes;
                int row = boxCoordinates[ii].Row;
                int column = boxCoordinates[ii].Column;
                Direction[] directions = pushMap[row, column];
                int n = directions.Length;
                for (int j = 0; j < n; j++)
                {
                    CheckBox(node, row, column, directions[j]);
                }
            }
            boxStart = (boxStart + 1) % boxes;

            return node.HasChildren;
        }

        private void CheckBox(Node node, int row, int column, Direction direction)
        {
            // The row and column specify the square of the box.
            // The sokoban is behind the box direction-wise.
            // The new box is ahead of the box direction-wise.

            int v = Direction.GetVertical(direction);
            int h = Direction.GetHorizontal(direction);

            // Check that the new box square is not a box.
            int newBoxRow = row + v;
            int newBoxColumn = column + h;
            Cell newBoxCell = data[newBoxRow, newBoxColumn];
            if (Level.IsBox(newBoxCell))
            {
                return;
            }

            // Check that we can access to push this box.
            int sokobanRow = row - v;
            int sokobanColumn = column - h;
            int distance = finder.GetDistance(sokobanRow, sokobanColumn);
            if (distance >= finderInaccessible)
            {
                return;
            }

            // Handle no influence pushes.
            int pushes = 1;
            if (detectNoInfluencePushes)
            {
                int oldBoxRow = row;
                int oldBoxColumn = column;

                while (!Level.IsTarget(newBoxCell) &&
                    Level.IsWall(data[oldBoxRow + h, oldBoxColumn + v]) &&
                    Level.IsWall(data[oldBoxRow - h, oldBoxColumn - v]) &&
                    ((Level.IsWall(data[newBoxRow + h, newBoxColumn + v]) &&
                    Level.IsWallOrEmpty(data[newBoxRow - h, newBoxColumn - v])) ||
                    (Level.IsWallOrEmpty(data[newBoxRow + h, newBoxColumn + v]) &&
                    Level.IsWall(data[newBoxRow - h, newBoxColumn - v]))))
                {
                    // Continue down the tunnel.
                    pushes++;

                    oldBoxRow += v;
                    oldBoxColumn += h;
                    newBoxRow += v;
                    newBoxColumn += h;
                    newBoxCell = data[newBoxRow, newBoxColumn];

                    if (!Level.IsSokobanOrEmpty(newBoxCell))
                    {
                        return;
                    }
                }
            }

            // Add this move to the search tree.
            Node child = new Node(nodes, sokobanRow, sokobanColumn, direction, node.Pushes + pushes, node.Moves + distance + pushes);
            node.Add(child);
        }
#endif

#if false
        private void SearchNode(Node node, int depth)
        {
            if (!node.Searched)
            {
                node.Searched = true;
                HashKey hashKey = currentHashKey;
                IsTerminal(node);
                FindChildren(node);
                currentHashKey = hashKey;
            }
            DoSearch(node, depth);
        }

        private void DoSearch(Node node, int depth)
        {
            depth--;
            MoveState state = new MoveState();
            PrepareToMove(node, ref state);

            Node previous = node;
            for (Node child = node.Child; !Node.IsEmpty(child); child = child.Sibling)
            {
                if (child.Terminal)
                {
#if DEBUG
                    if (child.InTable)
                    {
                        Abort("releasing node still in table");
                    }
#endif
                    RemoveDescendants(child);
                    node.Remove(nodes, previous, child);
                    child.Release(nodes);
                    continue;
                }

                DoMove(child, ref state);

                bool remove = false;
                bool terminal = false;
                if (!child.Searched)
                {
                    // Search the child node.
                    child.Searched = true;

                    if (level.IsComplete)
                    {
                        child.Complete = true;
                        CollectSolution(child);
                    }
                    else if (IsTerminal(child) || !FindChildren(child))
                    {
                        remove = true;
                        terminal = true;
                    }
                }

                if (!remove && depth > 0 && !child.Complete && !cancel)
                {
                    DoSearch(child, depth);
                    if (!child.HasChildren)
                    {
                        remove = true;
                    }
                }

                if (remove)
                {
#if DEBUG
                    if (child.HasChildren)
                    {
                        Abort("removing node with children");
                    }
#endif
                    node.Remove(nodes, previous, child);
                    if (terminal)
                    {
                        if (child.InTable)
                        {
                            // Node had no children so it is effectively deadlocked.
                            transpositionTable[currentHashKey] = Node.Empty;
                        }
                        child.Release(nodes);
                    }
                    else
                    {
                        child.Dormant = true;
                    }
                }
                else
                {
                    previous = child;
                }

                UndoMove(ref state);
            }

            FinishMoving(ref state);
        }
#endif

#if false
        private HashKey GetHashKey(Node node)
        {
#if DEBUG
            if (validate)
            {
                HashKey slowHashKey = GetSlowHashKey();
                if (currentHashKey != slowHashKey)
                {
                    Abort("hash key corrupted: incremental does not match full");
                }
                if (!node.HashKey.IsEmpty && currentHashKey != node.HashKey)
                {
                    Abort("hash key corrupted: does not match hash when created");
                }
            }
#endif
            return currentHashKey;
        }

#if false

        private HashKey GetHashKey(Node node)
        {
            if (optimizeMoves)
            {
                return currentHashKey ^ Level.GetSokobanHashKey(currentSokobanRow, currentSokobanColumn);
            }
            Coordinate2D proxySokobanCoord = finder.GetFirstAccessibleCoordinate();
            return currentHashKey ^ Level.GetSokobanHashKey(proxySokobanCoord.Row, proxySokobanCoord.Column);
        }

#endif

#endif

#if false
        private void RemovePosition(Node node)
        {
            if (node.InTable)
            {
#if DEBUG
                if (validate)
                {
                    HashKey hashKey = GetHashKey(node);
                    Node other = transpositionTable[hashKey];
                    if (other != node)
                    {
                        Abort("hash key associated with incorrect node");
                    }
                }
#endif
                transpositionTable[GetHashKey(node)] = Node.Empty;
            }
            ReleaseNode(node);
        }

        private void MarkDormant(Node node)
        {
            // A dormant node is a node that is no longer in the search tree
            // but cannot yet be removed from the transposition table.  It
            // serves to record the fewest number of moves and/or pushes
            // required to reach that position until a better one is found.
            // You may ask why its is then no longer in the tree and the
            // answer is that its children all lead to deadlocks or duplicates.
            // If we remove the node, we'll have to remove all its children
            // and we'll be forced the reinvestigate them if we find the
            // position again.

#if DEBUG
            if (validate)
            {
                HashKey hashKey = node.HashKey;
                if (hashKey.IsEmpty || transpositionTable[hashKey] != node)
                {
                    Abort("dormant node not in table");
                }
            }
#endif
            node.Dormant = true;
        }

        private void ReleaseNode(Node node)
        {
#if DEBUG
            if (validate)
            {
                if (!node.HashKey.IsEmpty)
                {
                    Node other = transpositionTable[node.HashKey];
                    if (!Node.IsEmpty(other) && other == node)
                    {
                        Abort("releasing node still in transposition table");
                    }
                }
            }
#endif
            node.Release(nodes);
        }

#endif

#if false
        private bool IsDuplicatePosition(Node node)
        {
            HashKey hashKey = GetHashKey(node);
            if (transpositionTable.ContainsKey(hashKey))
            {
                duplicates++;

                if (optimizeMoves)
                {
                    // Choose which node to pursue.
                    Node other = transpositionTable[hashKey];
                    if (Node.IsEmpty(other))
                    {
                        return true;
                    }
                    if (optimizePushes && other.Pushes < node.Pushes)
                    {
                        return true;
                    }
                    if (other.Moves <= node.Moves)
                    {
                        return true;
                    }

                    transpositionTable[hashKey] = node;
#if DEBUG
                    node.HashKey = hashKey;
#endif
                    node.InTable = true;
                    other.InTable = false;
                    if (other.Dormant)
                    {
                        ReleaseNode(other);
                    }
                    else
                    {
                        other.Terminal = true;
                    }
                    return false;
                }

                return true;
            }

            transpositionTable.Add(hashKey, node);
#if DEBUG
            node.HashKey = hashKey;
#endif
            node.InTable = true;
            return false;
        }

#endif

#if false
            // Delay duplicate detection until after deadlock detection
            // to avoid calling GetFirstAccessibleCoordinate.
            if (optimizeMoves)
            {
                currentHashKey = GetSlowHashKey();
#if DEBUG
                node.HashKey = curretHashKey;
#endif
                if (IsDuplicatePosition(node) || LevelUtils.IsDeadlocked(level))
                {
                    node.Terminal = true;
                    return false;
                }
            }
            else
            {
                if (LevelUtils.IsDeadlocked(level))
                {
                    node.Terminal = true;
                    return false;
                }
                finder.Find(currentSokobanRow, currentSokobanColumn);
                currentHashKey = GetSlowHashKey();
#if DEBUG
                node.HashKey = curretHashKey;
#endif
                if (IsDuplicatePosition(node))
                {
                    node.Terminal = true;
                    return false;
                }
            }
#endif


#if false
            Info u = infoMap[srcRow, srcCol];
            u.Distance = 0;
            u.Previous = null;
            u.Visited = true;
            while (true)
            {
                Info[] neighbors = u.Neighbors;
                int n = neighbors.Length;
                int alt = u.Distance + 1;
                for (int i = 0; i < n; i++)
                {
                    Info v = neighbors[i];
                    if (!v.Visited)
                    {
                        v.Visited = true;
                        v.Distance = alt;
                        v.Previous = u;
                        q.Enqueue(v);
                    }
                }

                if (q.Count == 0)
                {
                    break;
                }

                u = q.Dequeue();
            }
#endif

}

