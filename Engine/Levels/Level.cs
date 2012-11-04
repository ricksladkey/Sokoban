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
using System.Diagnostics;

using Sokoban.Engine.Core;

namespace Sokoban.Engine.Levels
{
    public class Level : CellData
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected string name;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool sokobanAssessed;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool boxesAssessed;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool outsideAssessed;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int sokobanRow;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int sokobanColumn;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool isClosed;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int insideSquares;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int sokobans;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int boxes;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int targets;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int unfinishedBoxes;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool markAccessible;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool validate;

        private Array2D<int> boxMap;
        private Coordinate2D[] boxCoordinates;
        private Array2D<bool> map;
        private CoordinatesByRow insideCoordinates;

        protected Level()
        {
        }

        public Level(int height, int width)
            : base(new Array2D<Cell>(height, width))
        {
            data.SetAll(Cell.Wall);

            Initialize();
        }

        public Level(Array2D<Cell> data)
            : base(data)
        {
            Initialize();
        }

        public Level(string line)
            : base(LevelEncoder.DecodeLevel(line))
        {
            Initialize();
        }

        public Level(IEnumerable<string> lines)
            : base(LevelEncoder.DecodeLevel(lines))
        {
            Initialize();
        }

        public Level(Level other)
            : base(new Array2D<Cell>(other.Data))
        {
            // Copy state information.
            name = other.name;
            sokobanAssessed = other.sokobanAssessed;
            boxesAssessed = other.boxesAssessed;
            outsideAssessed = other.outsideAssessed;
            sokobanRow = other.sokobanRow;
            sokobanColumn = other.sokobanColumn;
            sokobans = other.sokobans;
            boxes = other.boxes;
            targets = other.targets;
            insideSquares = other.insideSquares;
            isClosed = other.isClosed;
            unfinishedBoxes = other.unfinishedBoxes;
            markAccessible = other.markAccessible;
            validate = other.validate;
            boxMap = new Array2D<int>(other.boxMap);
            boxCoordinates = (Coordinate2D[])other.BoxCoordinates.Clone();
            insideCoordinates = other.insideCoordinates;

            Initialize();
        }

        public new Cell this[int row, int column]
        {
            get
            {
                return data[row, column];
            }
            set
            {
                if (IsSokobanBoxOrTarget(data[row, column]) || IsSokobanBoxOrTarget(value))
                {
                    sokobanAssessed = false;
                    boxesAssessed = false;
                }
                outsideAssessed = false;
                data[row, column] = value;
            }
        }

        public new Cell this[Coordinate2D coord]
        {
            get
            {
                return data[coord];
            }
            set
            {
                if (IsSokobanBoxOrTarget(data[coord.Row, coord.Column]) || IsSokobanBoxOrTarget(value))
                {
                    sokobanAssessed = false;
                    boxesAssessed = false;
                }
                outsideAssessed = false;
                data[coord.Row, coord.Column] = value;
            }
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

        public new Array2D<Cell> Data
        {
            get
            {
                return data;
            }
            set
            {
                base.Data = value;
                sokobanAssessed = false;
                boxesAssessed = false;
                outsideAssessed = false;
            }
        }

        public bool IsClosed
        {
            get
            {
                if (!outsideAssessed)
                {
                    AssessOutside();
                }
                return isClosed;
            }
        }

        public int InsideSquares
        {
            get
            {
                if (!outsideAssessed)
                {
                    AssessOutside();
                }
                return insideSquares;
            }
        }

        public int SokobanRow
        {
            get
            {
                if (!sokobanAssessed)
                {
                    AssessSokoban();
                }
                return sokobanRow;
            }
        }

        public int SokobanColumn
        {
            get
            {
                if (!sokobanAssessed)
                {
                    AssessSokoban();
                }
                return sokobanColumn;
            }
        }

        public Coordinate2D SokobanCoordinate
        {
            get
            {
                if (!sokobanAssessed)
                {
                    AssessSokoban();
                }
                return new Coordinate2D(sokobanRow, sokobanColumn);
            }
        }

        public int Boxes
        {
            get
            {
                if (!boxesAssessed)
                {
                    AssessBoxes();
                }
                return boxes;
            }
        }

        public int Targets
        {
            get
            {
                if (!boxesAssessed)
                {
                    AssessBoxes();
                }
                return targets;
            }
        }

        public int Sokobans
        {
            get
            {
                if (!boxesAssessed)
                {
                    AssessBoxes();
                }
                return sokobans;
            }
        }

        public int UnfinishedBoxes
        {
            get
            {
                if (!boxesAssessed)
                {
                    AssessBoxes();
                }
                return unfinishedBoxes;
            }
        }

        public bool IsComplete
        {
            get
            {
                if (!boxesAssessed)
                {
                    AssessBoxes();
                }
                return unfinishedBoxes == 0;
            }
        }

        public bool IsTraditional
        {
            get
            {
                AssessLevel();
                return isClosed && sokobans == 1 && boxes > 0 && boxes == targets;
            }
        }

        public bool MarkAccessible
        {
            get
            {
                return markAccessible;
            }
            set
            {
                markAccessible = value;
                if (markAccessible)
                {
                    AddAccessible();
                }
                else
                {
                    ClearAccessible();
                }
            }
        }

        public bool Validate
        {
            get
            {
                return validate;
            }
            set
            {
                validate = value;
            }
        }

        public string AsText
        {
            get
            {
                return LevelEncoder.EncodeLevel(Data, "\r\n");
            }
        }

        public string AsTextOneLine
        {
            get
            {
                string result = "";
                foreach (string line in LevelEncoder.EncodeLevel(Data))
                {
                    if (result != "")
                    {
                        result += "|";
                    }
                    result += line;
                }
                result = result.Replace(' ', '-');
                return result;
            }
        }

        public IEnumerable<Coordinate2D> Coordinates
        {
            get
            {
                return data.Coordinates;
            }
        }

        public CoordinatesByRow InsideCoordinates
        {
            get
            {
                if (!outsideAssessed)
                {
                    AssessOutside();
                }
                if (insideCoordinates == null)
                {
                    insideCoordinates = new CoordinatesByRow(levelHeight, SlowInsideCoordinates);
                }
                return insideCoordinates;
            }
        }

        private IEnumerable<Coordinate2D> SlowInsideCoordinates
        {
            get
            {
                for (int row = 0; row < levelHeight; row++)
                {
                    for (int column = 0; column < levelWidth; column++)
                    {
                        if (IsInside(data[row, column]))
                        {
                            yield return new Coordinate2D(row, column);
                        }
                    }
                }
            }
        }

        public IEnumerable<Coordinate2D> OutsideCoordinates
        {
            get
            {
                if (!outsideAssessed)
                {
                    AssessOutside();
                }
                for (int row = 0; row < levelHeight; row++)
                {
                    for (int column = 0; column < levelWidth; column++)
                    {
                        if (IsOutside(data[row, column]))
                        {
                            yield return new Coordinate2D(row, column);
                        }
                    }
                }
            }
        }

        public Coordinate2D[] BoxCoordinates
        {
            get
            {
                if (!boxesAssessed)
                {
                    AssessBoxes();
                }
                return boxCoordinates;
            }
        }

        public int BoxIndex(int row, int column)
        {
            if (!boxesAssessed)
            {
                AssessBoxes();
            }
            return boxMap[row, column];
        }

        public int BoxIndex(Coordinate2D coord)
        {
            return BoxIndex(coord.Row, coord.Column);
        }

        public IEnumerable<Coordinate2D> TargetCoordinates
        {
            get
            {
                for (int row = 0; row < levelHeight; row++)
                {
                    for (int column = 0; column < levelWidth; column++)
                    {
                        if (IsTarget(data[row, column]))
                        {
                            yield return new Coordinate2D(row, column);
                        }
                    }
                }
            }
        }

        public IEnumerable<Coordinate2D> WallCoordinates
        {
            get
            {
                for (int row = 0; row < levelHeight; row++)
                {
                    for (int column = 0; column < levelWidth; column++)
                    {
                        if (IsWall(data[row, column]))
                        {
                            yield return new Coordinate2D(row, column);
                        }
                    }
                }
            }
        }

        public IEnumerable<Coordinate2D> FloorCoordinates
        {
            get
            {
                for (int row = 0; row < levelHeight; row++)
                {
                    for (int column = 0; column < levelWidth; column++)
                    {
                        if (IsFloor(data[row, column]))
                        {
                            yield return new Coordinate2D(row, column);
                        }
                    }
                }
            }
        }

        protected void Initialize()
        {
            if (name == null)
            {
                name = "";
            }

            AssessLevel();

            if (markAccessible)
            {
                AddAccessible();
            }
        }

        private void AssessLevel()
        {
            if (!outsideAssessed)
            {
                AssessOutside();
            }
            if (!boxesAssessed)
            {
                AssessBoxes();
            }
            if (!sokobanAssessed)
            {
                AssessSokoban();
            }
        }

        private void AssessSokoban()
        {
            sokobanAssessed = true;

            sokobanRow = 0;
            sokobanColumn = 0;
            for (int row = 0; row < levelHeight; row++)
            {
                for (int column = 0; column < levelWidth; column++)
                {
                    if (IsSokoban(row, column))
                    {
                        sokobanRow = row;
                        sokobanColumn = column;
                        return;
                    }
                }
            }
        }

        private void AssessBoxes()
        {
            boxesAssessed = true;

            sokobans = 0;
            boxes = 0;
            targets = 0;
            unfinishedBoxes = 0;
            boxMap = new Array2D<int>(levelHeight, levelWidth);
            List<Coordinate2D> boxList = new List<Coordinate2D>();

            int rowLimit = levelHeight - 1;
            int columnLimit = levelWidth - 1;

            for (int row = 1; row < rowLimit; row++)
            {
                for (int column = 1; column < columnLimit; column++)
                {
                    Cell cell = data[row, column];
                    if (IsSokobanBoxOrTarget(cell))
                    {
                        if (IsSokoban(cell))
                        {
                            // While we're at it, record the sokoban square.
                            if (!sokobanAssessed)
                            {
                                sokobanAssessed = true;
                                sokobanRow = row;
                                sokobanColumn = column;
                            }
                            sokobans++;
                        }
                        else if (IsBox(cell))
                        {
                            boxMap[row, column] = boxes;
                            boxList.Add(new Coordinate2D(row, column));
                            boxes++;

                            if (!IsTarget(cell))
                            {
                                unfinishedBoxes++;
                            }
                        }
                        if (IsTarget(cell))
                        {
                            targets++;
                        }
                    }
                }
            }

            boxCoordinates = boxList.ToArray();
        }

#if DEBUG
        private void ValidateBoxes()
        {
            int count = 0;
            int unfinished = 0;
            foreach (Coordinate2D coord in InsideCoordinates)
            {
                if (IsBox(coord))
                {
                    count++;
                    if (!IsTarget(coord))
                    {
                        unfinished++;
                    }
                }
            }
            if (count != boxes)
            {
                throw new Exception("box count corrupted");
            }
            if (unfinished != unfinishedBoxes)
            {
                throw new Exception("unfinished boxes corrupted");
            }
            if (boxCoordinates.Length != boxes)
            {
                throw new Exception("wrong size for box coordinates");
            }
            for (int i = 0; i < boxCoordinates.Length; i++)
            {
                Coordinate2D coord = boxCoordinates[i];
                if (!IsBox(coord))
                {
                    throw new Exception("non-box square in box coordinates");
                }
                int index = boxMap[coord];
                if (index != i)
                {
                    throw new Exception("box map corrupted");
                }
            }
        }
#endif

        public void SortBoxes()
        {
            for (int i = 1; i < boxes; i++)
            {
                Coordinate2D value = boxCoordinates[i];
                int j = i - 1;
                while (j >= 0 && boxCoordinates[j] > value)
                {
                    boxCoordinates[j + 1] = boxCoordinates[j];
                    j--;
                }
                boxCoordinates[j + 1] = value;
            }

            for (int i = 0; i < boxes; i++)
            {
                boxMap[boxCoordinates[i]] = i;
            }
        }

        private void AssessOutside()
        {
            if (!boxesAssessed)
            {
                AssessBoxes();
            }
            outsideAssessed = true;
            insideCoordinates = null;

            // Determine whether there are any occupants.
            Array2D<bool> outsideMap = GetMap();
            if (boxes == 0 && sokobans == 0)
            {
                // Initially set all the wall squares to be outside as a barrier.
                for (int row = 0; row < levelHeight; row++)
                {
                    for (int column = 0; column < levelWidth; column++)
                    {
                        outsideMap[row, column] = IsWall(row, column);
                    }
                }

                // Mark all the floor squares accessible from the perimeter as outside.
                foreach (Coordinate2D coord in data.PerimeterCoordinates)
                {
                    if (IsFloor(coord) && !outsideMap[coord])
                    {
                        outsideMap.FloodFill(coord, false, true);
                    }
                }
            }
            else
            {
                // Note: Don't use coordinate iterators since this needs to be as fast as possible.

                // Initially set all the floor squares to be outside.
                for (int row = 0; row < levelHeight; row++)
                {
                    for (int column = 0; column < levelWidth; column++)
                    {
                        outsideMap[row, column] = IsFloor(row, column);
                    }
                }

                // Mark all the cells accessible to boxes or the sokoban as inside.
                for (int i = 0; i < boxes; i++)
                {
                    Coordinate2D coord = boxCoordinates[i];
                    outsideMap.FloodFill(coord.Row, coord.Column, true, false);
                }
                if (sokobans == 1)
                {
                    outsideMap.FloodFill(sokobanRow, sokobanColumn, true, false);
                }
                else
                {
                    for (int row = 0; row < levelHeight; row++)
                    {
                        for (int column = 0; column < levelWidth; column++)
                        {
                            if (IsSokoban(row, column))
                            {
                                outsideMap.FloodFill(row, column, true, false);
                            }
                        }
                    }
                }
            }

            // Mark or clear the outside flag and at the same
            // time notice if the level is closed or not.
            isClosed = true;
            insideSquares = 0;
            for (int row = 0; row < levelHeight; row++)
            {
                for (int column = 0; column < levelWidth; column++)
                {
                    if (IsFloor(Data[row, column]))
                    {
                        if (outsideMap[row, column])
                        {
                            Data[row, column] |= Cell.Outside;
                        }
                        else
                        {
                            Data[row, column] &= ~Cell.Outside;
                            insideSquares++;

                            if (IsEdge(row, column))
                            {
                                // Found a inside square on the perimeter.
                                isClosed = false;
                            }
                        }
                    }
                }
            }
            if (insideSquares == 0)
            {
                isClosed = false;
            }

            // Inside and outside aren't useful if the level isn't closed.
            if (!isClosed)
            {
                insideSquares = 0;
                for (int row = 0; row < levelHeight; row++)
                {
                    for (int column = 0; column < levelWidth; column++)
                    {
                        if (outsideMap[row, column])
                        {
                            Data[row, column] &= ~Cell.Outside;
                        }
                    }
                }
            }
        }

        private Array2D<bool> GetMap()
        {
            // Create the map if it hasn't been created yet or is the wrong size.
            if (map == null || map.Height != levelHeight || map.Width != levelWidth)
            {
                map = new Array2D<bool>(levelHeight, levelWidth);
            }
            return map;
        }

        public void AddAccessible()
        {
            AssessLevel();

            // Create a map of inaccessible squares initialized to false.
            Array2D<bool> inaccessibleMap = GetMap();

            // Mark all empty cells as inaccessible.
            foreach (Coordinate2D coord in data.Coordinates)
            {
                inaccessibleMap[coord] = IsSokobanOrEmpty(coord);
            }

            // Find accessible cells.
            inaccessibleMap.FloodFill(sokobanRow, sokobanColumn, true, false);

            // Mark those cells that are accessible.
            foreach (Coordinate2D coord in InsideCoordinates)
            {
                if (inaccessibleMap[coord] && IsSokobanOrEmpty(coord))
                {
                    data[coord] &= ~Cell.Accessible;
                }
                else
                {
                    data[coord] |= Cell.Accessible;
                }
            }
        }

        public void ClearAccessible()
        {
            Array2D<Cell> data = Data;
            foreach (Coordinate2D coord in InsideCoordinates)
            {
                data[coord] &= ~Cell.Accessible;
            }
        }

        public void MoveSokoban(int newSokobanRow, int newSokobanColumn)
        {
            if (!sokobanAssessed)
            {
                AssessSokoban();
            }

#if DEBUG
            if (validate)
            {
                // Perform extra checks in debug build.
                if (!IsEmpty(newSokobanRow, newSokobanColumn))
                {
                    if (!IsSokoban(newSokobanRow, newSokobanColumn))
                    {
                        throw new Exception("new sokoban coordinate not empty");
                    }
                }
            }
#endif

            data[sokobanRow, sokobanColumn] &= ~Cell.Sokoban;
            data[newSokobanRow, newSokobanColumn] |= Cell.Sokoban;
            sokobanRow = newSokobanRow;
            sokobanColumn = newSokobanColumn;

            if (markAccessible)
            {
                AddAccessible();
            }
        }

        public void MoveSokoban(Coordinate2D coord)
        {
            MoveSokoban(coord.Row, coord.Column);
        }

        public void AddSokoban(int newSokobanRow, int newSokobanColumn)
        {

#if DEBUG
            if (validate)
            {
                // Perform extra checks in debug build.
                if (!IsEmpty(newSokobanRow, newSokobanColumn))
                {
                    throw new Exception("new sokoban coordinate not empty");
                }
            }
#endif

            data[newSokobanRow, newSokobanColumn] |= Cell.Sokoban;
            sokobanRow = newSokobanRow;
            sokobanColumn = newSokobanColumn;
            sokobans++;

            if (markAccessible)
            {
                AddAccessible();
            }
        }

        public void AddSokoban(Coordinate2D newSokobanCoord)
        {
            AddSokoban(newSokobanCoord.Row, newSokobanCoord.Column);
        }

        public void RemoveSokoban()
        {
            if (!sokobanAssessed)
            {
                AssessSokoban();
            }

#if DEBUG
            if (validate)
            {
                // Perform extra checks in debug build.
                if (!IsSokoban(sokobanRow, sokobanColumn))
                {
                    throw new Exception("no sokoban to remove");
                }
            }
#endif

            data[sokobanRow, sokobanColumn] &= ~Cell.Sokoban;
            sokobanRow = 0;
            sokobanColumn = 0;
            sokobans--;
        }

        public void MoveBox(int oldBoxRow, int oldBoxColumn, int newBoxRow, int newBoxColumn)
        {
            if (!boxesAssessed)
            {
                AssessBoxes();
            }

#if DEBUG
            if (validate)
            {
                // Perform extra checks in debug build.
                if (oldBoxRow != newBoxRow || oldBoxColumn != newBoxColumn)
                {
                    if (!IsBox(oldBoxRow, oldBoxColumn))
                    {
                        throw new Exception("no box on old coordinate");
                    }
                    if (!IsEmpty(newBoxRow, newBoxColumn))
                    {
                        throw new Exception("new box coordinate is not empty");
                    }
                }
            }
#endif

            Cell oldBoxCell = (data[oldBoxRow, oldBoxColumn] &= ~Cell.Box);
            Cell newBoxCell = (data[newBoxRow, newBoxColumn] |= Cell.Box);

            unfinishedBoxes += IsTarget(oldBoxCell) ? 1 : 0;
            unfinishedBoxes -= IsTarget(newBoxCell) ? 1 : 0;

            int boxIndex = boxMap[oldBoxRow, oldBoxColumn];
            boxMap[newBoxRow, newBoxColumn] = boxIndex;
            boxCoordinates[boxIndex] = new Coordinate2D(newBoxRow, newBoxColumn);

#if DEBUG
            if (validate)
            {
                ValidateBoxes();
            }
#endif

            if (markAccessible)
            {
                AddAccessible();
            }
        }

        public void MoveBox(Coordinate2D oldBoxCoordinate, Coordinate2D newBoxCoordinate)
        {
            MoveBox(oldBoxCoordinate.Row, oldBoxCoordinate.Column, newBoxCoordinate.Row, newBoxCoordinate.Column);
        }

        public void MoveBoxes(params Coordinate2D[] newBoxCoords)
        {
            if (!boxesAssessed)
            {
                AssessBoxes();
            }

            // Validate that the the proper number of new coordinates was supplied.
            if (newBoxCoords.Length != boxes)
            {
                throw new InvalidOperationException("newBoxCoords does not match boxes");
            }

            // Clear the old boxes.
            for (int i = 0; i < boxes; i++)
            {
                data[boxCoordinates[i]] &= ~Cell.Box;
            }

            // Add the new boxes, updating the box state information.
            unfinishedBoxes = 0;
            for (int i = 0; i < boxes ; i++)
            {
                Coordinate2D coord = newBoxCoords[i];
                boxCoordinates[i] = coord;
                boxMap[coord] = i;
                data[coord] |= Cell.Box;
                if (!IsTarget(data[coord]))
                {
                    unfinishedBoxes++;
                }
            }

#if DEBUG
            if (validate)
            {
                ValidateBoxes();
                for (int i = 0; i < newBoxCoords.Length; i++)
                {
                    if (!IsBox(newBoxCoords[i]))
                    {
                        throw new Exception("boxes corrupted");
                    }
                }
            }
#endif
        }

        public bool Move(Operation operation, Direction direction)
        {
            Coordinate2D sokobanCoord = SokobanCoordinate;

            if (operation == Operation.Move)
            {
                if (IsEmpty(sokobanCoord + direction))
                {
                    MoveSokoban(sokobanCoord + direction);

                    return true;
                }

                return false;
            }

            if (operation == Operation.Push)
            {
                if (IsBox(sokobanCoord + direction) && IsEmpty(sokobanCoord + 2 * direction))
                {
                    MoveBox(sokobanCoord + direction, sokobanCoord + 2 * direction);
                    MoveSokoban(sokobanCoord + direction);

                    return true;
                }

                return false;
            }

            if (operation == Operation.Pull)
            {
                if (IsBox(data[sokobanCoord - direction]) && IsEmpty(data[sokobanCoord + direction]))
                {
                    MoveSokoban(sokobanCoord + direction);
                    MoveBox(sokobanCoord - direction, sokobanCoord);

                    return true;
                }

                return false;
            }

            throw new InvalidOperationException("Invalid operation");
        }

        public bool Move(OperationDirectionPair pair)
        {
            return Move(pair.Operation, pair.Direction);
        }

        public void Rotate()
        {
            Array2D<Cell> newData = new Array2D<Cell>(levelWidth, levelHeight);
            for (int row = 0; row < levelWidth; row++)
            {
                for (int column = 0; column < levelHeight; column++)
                {
                    newData[row, column] = Data[levelHeight - 1 - column, row];
                }
            }
            Data = newData;
        }

        public void Mirror()
        {
            Array2D<Cell> newData = new Array2D<Cell>(levelHeight, levelWidth);
            for (int row = 0; row < levelHeight; row++)
            {
                for (int column = 0; column < levelWidth; column++)
                {
                    newData[row, column] = Data[row, levelWidth - 1 - column];
                }
            }
            Data = newData;
        }

        public new bool IsWallOrOutside(int row, int column)
        {
            if (!outsideAssessed)
            {
                AssessOutside();
            }
            return base.IsWallOrOutside(row, column);
        }

        public new bool IsInside(int row, int column)
        {
            if (!outsideAssessed)
            {
                AssessOutside();
            }
            return base.IsInside(row, column);
        }

        public new bool IsOutside(int row, int column)
        {
            if (!outsideAssessed)
            {
                AssessOutside();
            }
            return base.IsOutside(row, column);
        }

        public HashKey GetOccupantsHashKey()
        {
            if (!boxesAssessed)
            {
                AssessBoxes();
            }

            HashKey hashKey = sokobans == 1 ? HashKey.GetSokobanHashKey(sokobanRow, sokobanColumn) : 0;
            for (int i = 0; i < boxes; i++)
            {
                hashKey ^= HashKey.GetBoxHashKey(boxCoordinates[i].Row, boxCoordinates[i].Column);
            }
            return hashKey;
        }

        public void CopyOccupantsFrom(Level other)
        {
            // Make sure the levels are assessed.
            AssessLevel();
            other.AssessLevel();

            // Quick check that the levels are compatible with this operation.
            if (boxes != other.boxes || sokobans > 1 || other.sokobans > 1 || insideSquares != other.insideSquares)
            {
                throw new InvalidOperationException("incompatible level");
            }

            // Remove the old sokoban.
            if (sokobans == 1)
            {
                RemoveSokoban();
            }

            // Move the boxes.
            MoveBoxes(other.boxCoordinates);

            // Add the new sokoban.
            if (other.sokobans == 1)
            {
                AddSokoban(other.SokobanCoordinate);
            }

            if (markAccessible)
            {
                AddAccessible();
            }

#if DEBUG
            if (validate)
            {
                if (!Equals(other))
                {
                    throw new Exception("copy occupants failed");
                }
            }
#endif

        }

        public override string ToString()
        {
            return String.Format("{{Level: Height = {0}, Width = {1}}}", Height, Width);
        }

    }
}
