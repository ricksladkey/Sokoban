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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Diagnostics;

using Sokoban.Engine.Core;

namespace Sokoban.Engine.Levels
{
    [XmlRoot("Level")]
    public class PlayingLevel : Level, IXmlSerializable
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int moveCount;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int pushCount;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int totalPushes;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private MoveList moveList;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<Coordinate2D> modifiedCells;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool autoFixWalls;

        public new Cell this[int row, int column]
        {
            get
            {
                return Data[row, column];
            }
            set
            {
                base[row, column] = value;
                ResetAndClearMoves();
            }
        }

        public new Cell this[Coordinate2D coord]
        {
            get
            {
                return Data[coord];
            }
            set
            {
                base[coord] = value;
                ResetAndClearMoves();
            }
        }

        public MoveList MoveList
        {
            get
            {
                return moveList;
            }
        }

        public int MoveCount
        {
            get
            {
                return moveCount;
            }
        }

        public int PushCount
        {
            get
            {
                return pushCount;
            }
        }

        public int TotalMoves
        {
            get
            {
                return moveList.Count;
            }
        }

        public int TotalPushes
        {
            get
            {
                return totalPushes;
            }
        }

        public bool AutoFixWalls
        {
            get
            {
                return autoFixWalls;
            }
            set
            {
                autoFixWalls = value;
            }
        }

        public List<Coordinate2D> ModifiedCells
        {
            get
            {
                return modifiedCells;
            }
        }

        protected PlayingLevel()
        {
            Initialize();
        }

        public PlayingLevel(Array2D<Cell> data)
            : base(data)
        {
            Initialize();
            Reset();
        }

        public PlayingLevel(Level other)
            : base(other)
        {
            Initialize();
            Reset();
        }

        public PlayingLevel(PlayingLevel other)
            : base(other)
        {
            Initialize();

            // Copy state information.
            this.moveList = new MoveList(other.moveList);
            this.moveCount = other.moveCount;
            this.autoFixWalls = other.autoFixWalls;

            Reset();
        }

        private new void Initialize()
        {
            autoFixWalls = true;
        }

        private void Reset()
        {
            if (moveList == null)
            {
                moveList = new MoveList();
            }
            if (modifiedCells == null)
            {
                modifiedCells = new List<Coordinate2D>();
            }

            if (autoFixWalls)
            {
                FixWalls();
            }
            FixPushCount();
        }

        private void ResetAndClearMoves()
        {
            Reset();
            ClearMoves();
        }

        private void ClearMoves()
        {
            pushCount = 0;
            moveCount = 0;
            moveList.Clear();
        }

        private void FixWalls()
        {
            foreach (Coordinate2D coord in OutsideCoordinates)
            {
                foreach (Coordinate2D neighbor in coord.EightNeighbors)
                {
                    if (IsInside(neighbor))
                    {
                        base[coord] = Cell.Wall;
                        break;
                    }
                }
            }
        }

        private void FixPushCount()
        {
            pushCount = 0;
            for (int i = 0; i < moveCount; i++)
            {
                if (moveList[i].Operation == Operation.Push)
                {
                    pushCount++;
                }
            }
            totalPushes = pushCount;
            for (int i = moveCount; i < moveList.Count; i++)
            {
                if (moveList[i].Operation == Operation.Push)
                {
                    totalPushes++;
                }
            }
        }

        private void ClearModified()
        {
            modifiedCells.Clear();
        }

        private void AddModified(int row, int column)
        {
            AddModified(new Coordinate2D(row, column));
        }

        private void AddModified(Coordinate2D coord)
        {
            modifiedCells.Add(coord);
        }

        public bool CanMoveSokoban(Direction direction)
        {
            return IsEmpty(SokobanCoordinate + direction);
        }

        public new bool Move(Operation operation, Direction direction)
        {
            if (MoveWithoutAddingMove(operation, direction))
            {
                AddMove(operation, direction);
                return true;
            }
            return false;
        }

        public new bool Move(OperationDirectionPair pair)
        {
            return Move(pair.Operation, pair.Direction);
        }

        public new void MoveSokoban(int newSokobanRow, int newSokobanColumn)
        {
            int sokobanRow = SokobanRow;
            int sokobanColumn = SokobanColumn;
            AddModified(sokobanRow, sokobanColumn);
            AddModified(newSokobanRow, newSokobanColumn);
            base.MoveSokoban(newSokobanRow, newSokobanColumn);

            ClearMoves();
        }

        public new void MoveSokoban(Coordinate2D coord)
        {
            MoveSokoban(coord.Row, coord.Column);
        }

        public new void MoveBox(int oldBoxRow, int oldBoxColumn, int newBoxRow, int newBoxColumn)
        {
            AddModified(oldBoxRow, oldBoxColumn);
            AddModified(newBoxRow, newBoxColumn);
            base.MoveBox(oldBoxRow, oldBoxColumn, newBoxRow, newBoxColumn);

            ClearMoves();
        }

        public new void MoveBox(Coordinate2D oldBoxCoordinate, Coordinate2D newBoxCoordinate)
        {
            MoveBox(oldBoxCoordinate.Row, oldBoxCoordinate.Column, newBoxCoordinate.Row, newBoxCoordinate.Column);
        }

        public bool MoveWithoutAddingMove(Operation operation, Direction direction)
        {
            ClearModified();

            Coordinate2D sokobanCoord = SokobanCoordinate;
            bool result = false;

            if (operation == Operation.Move)
            {
                if (base.Move(operation, direction))
                {
                    AddModified(sokobanCoord);
                    AddModified(sokobanCoord + direction);

                    result = true;
                }
            }

            if (operation == Operation.Push)
            {
                if (base.Move(operation, direction))
                {
                    AddModified(sokobanCoord);
                    AddModified(sokobanCoord + direction);
                    AddModified(sokobanCoord + 2 * direction);

                    pushCount++;
                    result = true;
                }
            }

            if (operation == Operation.Pull)
            {
                if (base.Move(operation, direction))
                {
                    AddModified(sokobanCoord - direction);
                    AddModified(sokobanCoord);
                    AddModified(sokobanCoord + direction);

                    pushCount--;
                    result = true;
                }
            }

            return result;
        }

        private void AddMove(Operation operation, Direction direction)
        {
            if (moveCount < moveList.Count)
            {
                moveList.RemoveRange(moveCount, moveList.Count - moveCount);
            }
            moveList.Add(new OperationDirectionPair(operation, direction));
            moveCount++;
            if (operation == Operation.Push)
            {
                totalPushes++;
            }
            else if (operation == Operation.Push)
            {
                totalPushes--;
            }
            if (moveCount == moveList.Count)
            {
                totalPushes = pushCount;
            }
        }

        public void SetSolution(MoveList solution)
        {
            moveList = new MoveList(solution);
            moveCount = 0;

            FixPushes();
        }

        private void FixPushes()
        {
            PlayingLevel tempLevel = new PlayingLevel(this);
            for (int i = 0; i < moveList.Count; i++)
            {
                OperationDirectionPair pair = moveList[i];
                if (!tempLevel.CanMoveSokoban(pair.Direction))
                {
                    pair = new OperationDirectionPair(Operation.Push, pair.Direction);
                    moveList[i] = pair;
                }
                tempLevel.Move(pair.Operation, pair.Direction);
            }
            FixPushCount();
        }

        public void Undo()
        {
            ClearModified();

            if (moveCount == 0)
            {
                return;
            }

            OperationDirectionPair pair = OperationDirectionPair.GetOpposite(moveList[moveCount - 1]);

            MoveWithoutAddingMove(pair.Operation, pair.Direction);

            moveCount--;

        }

        public void Redo()
        {
            ClearModified();

            if (moveCount == moveList.Count)
            {
                return;
            }

            OperationDirectionPair pair = moveList[moveCount];

            MoveWithoutAddingMove(pair.Operation, pair.Direction);

            moveCount++;
        }

        public new void Rotate()
        {
            base.Rotate();

            for (int index = 0; index < moveList.Count; index++)
            {
                OperationDirectionPair pair = moveList[index];
                moveList[index] = OperationDirectionPair.Rotate(pair);
            }

            Reset();
        }

        public new void Mirror()
        {
            base.Mirror();

            for (int index = 0; index < moveList.Count; index++)
            {
                OperationDirectionPair pair = moveList[index];
                moveList[index] = OperationDirectionPair.Mirror(pair);
            }

            Reset();
        }

        public void SwapBoxesAndTargets()
        {
            bool moveSokoban = false;
            for (int row = 0; row < levelHeight; row++)
            {
                for (int column = 0; column < levelWidth; column++)
                {
                    Cell cell = Data[row, column];
                    if (IsBox(cell) && !IsTarget(cell))
                    {
                        cell = Cell.Target;
                    }
                    else if (IsTarget(cell) && !IsBox(cell))
                    {
                        if (IsSokoban(cell))
                        {
                            moveSokoban = true;
                        }
                        cell = Cell.Box;
                    }
                    base[row, column] = cell;
                }
            }
            if (moveSokoban)
            {
                for (int row = 0; row < levelHeight; row++)
                {
                    for (int column = 0; column < levelWidth; column++)
                    {
                        Cell cell = base[row, column];
                        if (IsEmpty(cell) && !IsOutside(cell))
                        {
                            cell |= Cell.Sokoban;
                            base[row, column] = cell;
                            moveSokoban = false;
                            break;
                        }
                    }
                }
            }
            ResetAndClearMoves();
        }

        public void ChangeSize(int rows, int columns)
        {
            Array2D<Cell> newData = new Array2D<Cell>(rows, columns);
            newData.SetAll(Cell.Empty);
            int minHeight = Math.Min(levelHeight, rows);
            int minWidth = Math.Min(levelWidth, columns);
            Data.CopyTo(newData, 0, 0, 0, 0, minHeight, minWidth);
            Data = newData;

            ResetAndClearMoves();
        }

        public void MoveOccupant(int srcRow, int srcCol, int dstRow, int dstCol)
        {
            if (IsEmpty(srcRow, srcCol))
            {
                return;
            }
            if (!IsEmpty(dstRow, dstCol))
            {
                return;
            }
            ClearModified();
            Cell occupant = Data[srcRow, srcCol] & (Cell.Sokoban | Cell.Box);
            base[srcRow, srcCol] &= ~occupant;
            base[dstRow, dstCol] |= occupant;
            AddModified(srcRow, srcCol);
            AddModified(dstRow, dstCol);

            ResetAndClearMoves();
        }

        public void MoveOccupant(Coordinate2D srcCoord, Coordinate2D dstCoord)
        {
            MoveOccupant(srcCoord.Row, srcCoord.Column, dstCoord.Row, dstCoord.Column);
        }

        public void ClearOccupant(int row, int column)
        {
            if (IsEmpty(row, column))
            {
                return;
            }
            ClearModified();
            base[row, column] &= ~(Cell.Sokoban | Cell.Box);
            AddModified(row, column);
            ResetAndClearMoves();
        }

        public void ClearOccupant(Coordinate2D coord)
        {
            ClearOccupant(coord.Row, coord.Column);
        }

        public void ClearTarget(int row, int column)
        {
            if (!IsTarget(row, column))
            {
                return;
            }
            ClearModified();
            base[row, column] &= ~Cell.Target;
            AddModified(row, column);
            ResetAndClearMoves();
        }

        public void ClearTarget(Coordinate2D coord)
        {
            ClearTarget(coord.Row, coord.Column);
        }

        public void ClearWall(int row, int column)
        {
            if (!IsWall(row, column))
            {
                return;
            }
            if (autoFixWalls)
            {
                if (IsCriticalWall(row, column))
                {
                    if (IsEdgeWall(row, column))
                    {
                        ExpandLevel(ref row, ref column);
                    }
                    foreach (Coordinate2D neighbor in new Coordinate2D(row, column).FourNeighbors)
                    {
                        if (IsOutside(neighbor))
                        {
                            base[neighbor] = Cell.Wall;
                        }
                    }
                }
            }
            else
            {
                if (IsEdgeWall(row, column))
                {
                    ExpandLevel(ref row, ref column);
                }
            }
            base[row, column] = Cell.Empty;
            ResetAndClearMoves();
        }

        public void ClearWall(Coordinate2D coord)
        {
            ClearWall(coord.Row, coord.Column);
        }

        private void ExpandLevel(ref int row, ref int column)
        {
            int rows = levelHeight;
            int columns = levelWidth;
            int startRow = 0;
            int startColumn = 0;

            if (row == 0)
            {
                rows++;
                startRow = 1;
            }
            else if (row == levelHeight - 1)
            {
                rows++;
            }
            if (column == 0)
            {
                columns++;
                startColumn = 1;
            }
            else if (column == levelWidth - 1)
            {
                columns++;
            }

            Array2D<Cell> newData = new Array2D<Cell>(rows, columns);
            newData.SetAll(Cell.Outside);
            Data.CopyTo(newData, startRow, startColumn, 0, 0, levelHeight, levelWidth);
            Data = newData;

            levelHeight = rows;
            levelWidth = columns;
            row += startRow;
            column += startColumn;
        }

        public void AddOccupant(int row, int column, Cell occupant)
        {
            if (!IsEmpty(row, column))
            {
                return;
            }
            ClearModified();
            base[row, column] &= ~(Cell.Sokoban | Cell.Box);
            base[row, column] |= occupant;
            AddModified(row, column);
            ResetAndClearMoves();
        }

        public void AddOccupant(Coordinate2D coord, Cell occupant)
        {
            AddOccupant(coord.Row, coord.Column, occupant);
        }

        public void AddTarget(int row, int column)
        {
            if (IsWall(row, column))
            {
                return;
            }
            ClearModified();
            base[row, column] |= Cell.Target;
            AddModified(row, column);
            ResetAndClearMoves();
        }

        public void AddTarget(Coordinate2D coord)
        {
            AddTarget(coord.Row, coord.Column);
        }

        public void AddWall(int row, int column)
        {
            AddWall(new Coordinate2D(row, column));
        }

        public void AddWall(Coordinate2D coord)
        {
            if (!IsValid(coord))
            {
                return;
            }
            ClearModified();
            base[coord] = Cell.Wall;
            AddModified(coord);
            if (autoFixWalls)
            {
                foreach (Coordinate2D neighbor in coord.EightNeighbors)
                {
                    CheckRemoveWall(neighbor);
                }
            }
            ResetAndClearMoves();
        }

        private void CheckRemoveWall(Coordinate2D coord)
        {
            if (!IsWall(coord))
            {
                return;
            }
            foreach (Coordinate2D neighbor in coord.FourNeighbors)
            {
                if (IsValid(neighbor) && !IsWallOrOutside(neighbor))
                {
                    return;
                }
            }
            base[coord] = Cell.Outside;
            AddModified(coord);
        }

        private void AddModifiedRegion(int firstRow, int firstCol, int lastRow, int lastCol)
        {
            for (int row = firstRow; row <= lastRow; row++)
            {
                for (int column = firstCol; column <= lastCol; column++)
                {
                    if (IsValid(row, column))
                    {
                        AddModified(row, column);
                    }
                }
            }
        }

        public void DumpMoveList()
        {
            for (int i = 0; i < moveList.Count; i++)
            {
                OperationDirectionPair pair = moveList[i];
                Trace.WriteLine(String.Format("operation = {0}, direction = {1}", pair.Operation, pair.Direction));
            }
        }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Name", name);
            writer.WriteAttributeString("MoveCount", MoveCount.ToString());
            writer.WriteStartElement("Data");
            writer.WriteAttributeString("Height", levelHeight.ToString());
            writer.WriteAttributeString("Width", levelWidth.ToString());
            foreach (string row in LevelEncoder.EncodeLevel(Data))
            {
                writer.WriteStartElement("Row");
                writer.WriteString(row);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteStartElement("Moves");
            writer.WriteString(SolutionEncoder.EncodedSolution(moveList));
            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            name = reader.GetAttribute("Name");
            moveCount = Int32.Parse(reader.GetAttribute("MoveCount"));
            reader.Read();
            if (!reader.IsStartElement("Data"))
            {
                throw new InvalidOperationException("missing data element");
            }
            int height = Int32.Parse(reader.GetAttribute("Height"));
            int width = Int32.Parse(reader.GetAttribute("Width"));
            reader.Read();
            List<string> rows = new List<string>();
            for (int row = 0; row < height; row++)
            {
                string text = "";
                if (reader.IsEmptyElement)
                {
                    reader.Read();
                }
                else
                {
                    reader.ReadStartElement("Row");
                    text = reader.ReadString();
                    reader.ReadEndElement();
                }
                rows.Add(text);
            }
            Data = new Array2D<Cell>(LevelEncoder.DecodeLevel(rows));
            reader.ReadEndElement();
            reader.ReadStartElement("Moves");
            moveList = SolutionEncoder.MoveList(reader.ReadString());
            reader.ReadEndElement();

            base.Initialize();
            Reset();
        }

        #endregion
    }
}
