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
using System.ComponentModel;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Threading;

using Sokoban.Engine.Collections;
using Sokoban.Engine.Core;
using Sokoban.Engine.Deadlocks;
using Sokoban.Engine.Levels;
using Sokoban.Engine.Paths;
using Sokoban.Engine.Solvers;
using Sokoban.Engine.Utilities;

namespace Sokoban.Player
{
    public partial class MainWindow : Form
    {
        private LevelSet levelSet;
        private Level originalLevel;
        private PlayingLevel level;
        private Bitmap skin;
        private Dictionary<Cell, Bitmap> table;
        private Dictionary<CornerType, Bitmap> walls;
        private int wSkin;
        private int hSkin;
        private int wScreen;
        private int hScreen;
        private int wSplit;
        private int hSplit;
        private string dataDirectory;
        private string deadlocksDirectory;
        private int currentLevelNumber;
        private bool customLevel;
        private int scaleFactor;
        private bool doubleSize;
        private bool allowPush;
        private bool allowPull;
        private bool editLevel;
        private bool markAccessible;
        private bool autoFixWalls;
        SolverDialog solverDialog;

        public enum BeepType : uint
        {
            MB_OK = 0x0,
            MB_ICONHAND = 0x00000010,
            MB_ICONQUESTION = 0x00000020,
            MB_ICONEXCLAMATION = 0x00000030,
            MB_ICONASTERISK = 0x00000040,
            SIMPLE_BEEP = 0xffffffff
        };

        [DllImport("user32.dll")]
        public static extern bool MessageBeep(BeepType beepType);

        public Level OriginalLevel
        {
            get
            {
                return originalLevel;
            }
        }

        public Level Level
        {
            get
            {
                return level;
            }
        }

        public LevelSet LevelSet
        {
            get
            {
                return levelSet;
            }
        }

        public string DataDirectory
        {
            get
            {
                return dataDirectory;
            }
        }

        public string DeadlocksDirectory
        {
            get
            {
                return deadlocksDirectory;
            }
        }

        public void Initialize()
        {
            solverDialog = new SolverDialog(this);
            Icon = Properties.Resources.SokobanIcon;
            ToolStripManager.Renderer = new ToolStripProfessionalRenderer(new SokobanColorTable());

            dataDirectory = Application.StartupPath + @"\..\..\";
            deadlocksDirectory = dataDirectory + @"Deadlocks\";

            LoadDefaultSkin();
            LoadDefaultLevelSet();
            ToggleAllowPush();
            ToggleAutoFixWalls();
        }

        public MainWindow()
        {
            InitializeComponent();

            Initialize();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                bool loadedLevelSet = true;
                string arg1 = args[1];
                if (!File.Exists(arg1))
                {
                    arg1 = dataDirectory + arg1;
                }
                if (Path.GetExtension(arg1).Equals(".xml", StringComparison.InvariantCultureIgnoreCase))
                {
                    LoadLevel(arg1);
                    loadedLevelSet = false;
                }
                else
                {
                    LoadLevelSet(arg1);
                }

                if (loadedLevelSet && args.Length > 2)
                {
                    string arg2 = args[2];
                    int levelNumber = Int32.Parse(arg2);
                    SelectLevel(levelNumber - 1);
                }
            }
        }

        public MainWindow(MainWindow other)
        {
            InitializeComponent();

            Initialize();

            this.customLevel = other.customLevel;
            this.levelSet = other.levelSet;
            if (this.customLevel)
            {
                SetCustomLevel(other.originalLevel, other.level);
            }
            else
            {
                this.currentLevelNumber = other.currentLevelNumber;
                SetLevel(levelSet[currentLevelNumber], new PlayingLevel(other.Level));
            }

            RefreshLevel();
        }

        private void LoadDefaultSkin()
        {
            LoadSkin(Properties.Resources.DefaultSkin);
        }

        private void LoadDefaultLevelSet()
        {
            LoadLevelSet(Properties.Resources.DefaultLevelSet);
        }

        private Bitmap GetSkinTile(int row, int column)
        {
            return skin.Clone(new Rectangle(column * wSkin, row * hSkin, wSkin, hSkin), System.Drawing.Imaging.PixelFormat.DontCare);
        }

        private Bitmap GetScreenTile(int row, int column)
        {
            // Use a 3x3 tiled version of the image for scaling to avoid algorithm artifacts near the edges.
            Bitmap skinTile = GetSkinTile(row, column);
            Bitmap screenTile = new Bitmap(wScreen, hScreen);
            using (Bitmap bigTile = new Bitmap(wSkin * 3, hSkin * 3))
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        PictureGrid.CopyBitmap(bigTile, i * wSkin, j * hSkin, skinTile, 0, 0, wSkin, hSkin);
                    }
                }
                PictureGrid.StretchBitmap(screenTile, 0, 0, wScreen, hScreen, bigTile, wSkin, hSkin, wSkin, hSkin);
            }
            return screenTile;
        }

        private void LoadLevelSet(string filename)
        {
            try
            {
                using (TextReader reader = File.OpenText(filename))
                {
                    levelSet = new LevelSet(reader);
                }
                SelectLevel(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Exception loading level set: {0}", ex.Message));
            }
        }

        private void LoadLevelSet(byte[] buffer)
        {
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                using (TextReader reader = new StreamReader(stream))
                {
                    levelSet = new LevelSet(reader);
                }
            }
            SelectLevel(0);
        }

        private void LoadSkin(string filename)
        {
            try
            {
                LoadSkin(new Bitmap(filename));
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Exception loading skin: {0}", ex.Message));
            }
        }

        private void LoadSkin(Bitmap bitmap)
        {
            skin = bitmap;

            wSkin = skin.Width / 4;
            hSkin = skin.Height / 8;

            if (scaleFactor == 0)
            {
                scaleFactor = 100;
            }
            wScreen = (int)Math.Round(wSkin * scaleFactor / 100.0);
            hScreen = (int)Math.Round(hSkin * scaleFactor / 100.0);

            Bitmap emptyFloor = GetScreenTile(0, 0);
            Bitmap sokobanOnFloor = GetScreenTile(0, 1);
            Bitmap boxOnFloor = GetScreenTile(0, 2);

            Bitmap emptyTarget = GetScreenTile(1, 0);
            Bitmap sokobanOnTarget = GetScreenTile(1, 1);
            Bitmap boxOnTarget = GetScreenTile(1, 2);

            Bitmap outside = GetScreenTile(2, 3);

            Bitmap undefined = new Bitmap(emptyFloor);
            using (Graphics g = Graphics.FromImage(undefined))
            {
                g.FillRectangle(Brushes.Black, new Rectangle(0, 0, undefined.Width, undefined.Height));
            }

            walls = new Dictionary<CornerType, Bitmap>();
            walls[CornerType.Type1] = GetScreenTile(2, 0);
            walls[CornerType.Type2] = GetScreenTile(2, 1);
            walls[CornerType.Type3] = GetScreenTile(2, 2);
            walls[CornerType.Type4] = GetScreenTile(3, 0);
            walls[CornerType.Type5] = GetScreenTile(3, 1);

            using (Bitmap splitter = GetSkinTile(3, 3))
            {
                GetSplits(splitter);
                wSplit = (int)Math.Round(wSplit * scaleFactor / 100.0);
                hSplit = (int)Math.Round(hSplit * scaleFactor / 100.0);
            }

            table = new Dictionary<Cell, Bitmap>();

            table[Cell.Empty] = emptyFloor;
            table[Cell.Sokoban] = sokobanOnFloor;
            table[Cell.Sokoban | Cell.Undefined] = sokobanOnFloor;
            table[Cell.Box] = boxOnFloor;

            table[Cell.Target] = emptyTarget;
            table[Cell.Sokoban | Cell.Target] = sokobanOnTarget;
            table[Cell.Box | Cell.Target] = boxOnTarget;

            table[Cell.Outside] = outside;

            table[Cell.Wall] = null;

            table[Cell.Undefined] = undefined;

            RefreshLevel();
        }

        private void GetSplits(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            wSplit = width / 2;
            hSplit = height / 2;

            Color first = bitmap.GetPixel(0, 0);

            for (int i = 1; i < width; i++)
            {
                if (bitmap.GetPixel(i, 0) != first)
                {
                    wSplit = i;
                    break;
                }
            }

            for (int i = 1; i < height; i++)
            {
                if (bitmap.GetPixel(0, i) != first)
                {
                    hSplit = i;
                    break;
                }
            }
        }

        private void SelectLevel(int index)
        {
            if (index < 0 || index >= levelSet.Count)
            {
                return;
            }

            currentLevelNumber = index;
            SetLevel(levelSet[currentLevelNumber]);
            customLevel = false;

            if (allowPull && !allowPush)
            {
                ToggleAllowPull();
                ToggleAllowPush();
            }

            RefreshLevel();

        }

        private void SetLevel(Level newOriginalLevel, PlayingLevel newLevel)
        {
            originalLevel = newOriginalLevel;
            level = newLevel;
        }

        private void SetLevel(Level newOriginalLevel)
        {
            SetLevel(newOriginalLevel, new PlayingLevel(newOriginalLevel));
        }

        private void SetCustomLevel(Level newOriginalLevel, PlayingLevel newLevel)
        {
            customLevel = true;
            SetLevel(newOriginalLevel, newLevel);
        }

        private void SetCustomLevel(Level newOriginalLevel)
        {
            customLevel = true;
            SetLevel(newOriginalLevel);
        }

        private void RunSolver()
        {
            if (solverDialog.ShowDialog() == DialogResult.OK)
            {
                if (solverDialog.Solution != null)
                {
                    if (LevelUtils.IsSolutionCompatible(originalLevel, solverDialog.Solution))
                    {
                        SetLevel(originalLevel);
                        level.SetSolution(solverDialog.Solution);
                        RefreshLevel();
                    }
                    else
                    {
                        MessageBox.Show("Solution not compatible with this level.");
                    }
                }
            }
        }

        private void RefreshLevel()
        {
            if (level == null)
            {
                return;
            }

            pictureGrid.PictureHeight = hScreen;
            pictureGrid.PictureWidth = wScreen;

            pictureGrid.Rows = level.Height;
            pictureGrid.Columns = level.Width;

            for (int row = 0; row < pictureGrid.Rows; row++)
            {
                for (int column = 0; column < pictureGrid.Columns; column++)
                {
                    RefreshCell(row, column);
                }
            }

            Text = levelSet.Name + " - Sokoban";
            toolStripStatusLabel2.Text = level.Name;
            RefreshStatus();
            RefreshSize();

            //pictureGrid.Image.Save(@"c:\tmp\sokoban.bmp", ImageFormat.Bmp);
        }

        private void RefreshSize()
        {
            panel1.Location = new Point(0, menuStrip1.Height);
            panel1.ClientSize = new Size(pictureGrid.Width, pictureGrid.Height);
            ClientSize = new Size(panel1.Width, menuStrip1.Height + panel1.Height + statusStrip1.Height + statusStrip2.Height);
        }

        private void NextLevel()
        {
            SelectLevel(currentLevelNumber + 1);
        }

        private void PreviousLevel()
        {
            SelectLevel(currentLevelNumber - 1);
        }

        private void BackwardsMode()
        {
            customLevel = true;
            level.SwapBoxesAndTargets();
            if (allowPull != allowPush)
            {
                ToggleAllowPull();
                ToggleAllowPush();
            }
            RefreshLevel();
        }

        private void NewWindow()
        {
            MainWindow window = new MainWindow(this);
            SokobanApplicationContext.Instance.AddWindow(window);
            window.Show();
        }

        private void SetStatus(string status)
        {
            toolStripStatusLabel1.Text = status;
        }

        private void SetInfo(string info)
        {
            toolStripStatusLabel1.Text = info;
        }

        private void Information()
        {
            bool showDeadlocked = true;
            bool showLowerBound = false;

            string info = String.Format("size = {0}x{1}", level.Height, level.Width);

            info += String.Format(", squares = {0}", level.InsideSquares);

            if (showDeadlocked)
            {
                DeadlockFinder deadlockFinder = DeadlockFinder.CreateInstance(level);
                deadlockFinder.FindDeadlocks();
                string deadlocked = deadlockFinder.IsDeadlocked() ? "yes" : "no";
                info += String.Format(", dl1 = {0}", deadlocked);

                deadlocked = "unknown";
                deadlockFinder = DeadlockUtils.LoadDeadlocks(level, deadlocksDirectory);
                if (deadlockFinder != null)
                {
                    deadlocked = deadlockFinder.IsDeadlocked() ? "yes" : "no";
                }
                info += String.Format(", dl2 = {0}", deadlocked);
            }

            if (showLowerBound)
            {
                LowerBoundSolver solver = new LowerBoundSolver();
                solver.CalculateDeadlocks = false;
                solver.Level = level;
                int lowerBound = solver.GetLowerBound();
                info += String.Format(", lower bound = {0}", lowerBound);
            }

            SetInfo(info);
        }

        private void CalculateDeadlocks()
        {
#if false
            foreach (int[] combination in CombinationUtils.GetCombinations(5, 3))
            {
                string s = "combination:";
                foreach (int index in combination)
                {
                    s += " " + index;
                }
                Log.DebugPrint(s);
            }
            return;
#endif
            SetInfo("Calculating deadlocks...");

            // Calculate deadlocks for the current level.
            DateTime start = DateTime.Now;
#if false
            // For testing: frozen sets only.
            DeadlockFinder deadlockFinder = new TableBasedDeadlockFinder(originalLevel, true);
#else
            DeadlockFinder deadlockFinder = DeadlockFinder.CreateInstance(originalLevel, true);
#endif
            deadlockFinder.FindDeadlocks();
            TimeSpan elapsed = DateTime.Now - start;
            string info = String.Format("deadlocks in {0} seconds", elapsed.TotalSeconds);

            if (originalLevel.Boxes <= 4)
            {
                // Show the deadlocks calculated.
                DeadlockUtils.ShowDeadlocks(true, originalLevel, deadlockFinder.Deadlocks);
            }
            else
            {
                DeadlockUtils.ShowDeadlocks(false, originalLevel, deadlockFinder.Deadlocks);
            }

#if false
#else
            // Save the deadlocks in the deadlock cache.
            DeadlockUtils.SaveDeadlocks(deadlockFinder, deadlocksDirectory);
#endif

            SetInfo(info);
        }

        private void SolubleInformation()
        {
            string soluble = "unknown";
            try
            {
                Solver solver = Solver.CreateInstance();
                solver.OptimizeMoves = false;
                solver.OptimizePushes = false;
                solver.Level = level;
                if (solver.Solve())
                {
                    soluble = "yes";
                }
                else
                {
                    soluble = "no";
                }
            }
            catch
            {
                soluble = "exception";
            }
            string info = String.Format("soluble = {0}", soluble);
            SetInfo(info);
        }

        private void GoToLevel()
        {
            GoToDialog gtd = new GoToDialog(currentLevelNumber + 1);
            gtd.LevelInfo = String.Format("Levels in levelSet {0}", levelSet.Count);
            if (gtd.ShowDialog() == DialogResult.OK)
            {
                SelectLevel(gtd.Level - 1);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.T:
                    TrimLevel();
                    break;

                case Keys.Control | Keys.K:
                    CleanLevel();
                    break;

                case Keys.Control | Keys.E:
                    ToggleEditLevel();
                    break;

                case Keys.Control | Keys.X:
                    Close();
                    break;

                case Keys.Control | Keys.L:
                    RefreshLevel();
                    break;

                case Keys.Control | Keys.N:
                    NextLevel();
                    break;

                case Keys.Control | Keys.P:
                    PreviousLevel();
                    break;

                case Keys.Control | Keys.G:
                    GoToLevel();
                    break;

                case Keys.Control | Keys.R:
                    RotateLevel();
                    break;

                case Keys.Control | Keys.M:
                    MirrorLevel();
                    break;

                case Keys.Control | Keys.C:
                    CopySolution();
                    break;

                case Keys.Control | Keys.Q:
                    CopyLevel();
                    break;

                case Keys.Control | Keys.O:
                    CopyLevelOneLine();
                    break;

                case Keys.Control | Keys.V:
                    PasteSolutionOrLevel();
                    break;

                case Keys.Control | Keys.Z:
                    ToggleDoubleSize();
                    break;

                case Keys.Control | Keys.Up:
                    AdjustScale(10);
                    break;

                case Keys.Control | Keys.Down:
                    AdjustScale(-10);
                    break;

                case Keys.Control | Keys.D:
                    CalculateDeadlocks();
                    break;

                case Keys.Control | Keys.S:
                    RunSolver();
                    break;

                case Keys.Control | Keys.B:
                    BackwardsMode();
                    break;

                case Keys.Control | Keys.W:
                    NewWindow();
                    break;

                case Keys.Control | Keys.I:
                    Information();
                    break;

                case Keys.Up:
                    MoveOrPushSokoban(Direction.Up);
                    break;

                case Keys.Down:
                    MoveOrPushSokoban(Direction.Down);
                    break;

                case Keys.Left:
                    MoveOrPushSokoban(Direction.Left);
                    break;

                case Keys.Right:
                    MoveOrPushSokoban(Direction.Right);
                    break;

                case Keys.Shift | Keys.Up:
                    PullSokoban(Direction.Up);
                    break;

                case Keys.Shift | Keys.Down:
                    PullSokoban(Direction.Down);
                    break;

                case Keys.Shift | Keys.Left:
                    PullSokoban(Direction.Left);
                    break;

                case Keys.Shift | Keys.Right:
                    PullSokoban(Direction.Right);
                    break;

                case Keys.Back:
                case Keys.Enter:
                case Keys.Tab:
                    Undo();
                    break;

                case Keys.Space:
                    Redo();
                    break;

                case Keys.Control | Keys.Home:
                    UndoToBeginning();
                    break;

                case Keys.Control | Keys.End:
                    RedoToEnd();
                    break;

            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void MoveOrPushSokoban(Direction direction)
        {
            if (level.IsComplete)
            {
                return;
            }
            if (level.CanMoveSokoban(direction))
            {
                level.Move(Operation.Move, direction);
            }
            else if (allowPush)
            {
                level.Move(Operation.Push, direction);
            }

            RefreshModified();
        }

        private void PullSokoban(Direction direction)
        {
            if (level.IsComplete)
            {
                return;
            }
            if (allowPull)
            {
                level.Move(Operation.Pull, direction);

                RefreshModified();
            }
        }

        private void Undo()
        {
            level.Undo();

            RefreshModified();
        }

        private void Redo()
        {
            level.Redo();

            RefreshModified();
        }

        private void UndoToBeginning()
        {
            while (level.MoveCount > 0)
            {
                level.Undo();

                RefreshModified();

                Application.DoEvents();

                Thread.Sleep(10);
            }
        }

        private void RedoToEnd()
        {
            while (level.MoveCount < level.TotalMoves)
            {
                level.Redo();

                RefreshModified();

                Application.DoEvents();

                Thread.Sleep(10);
            }
        }

        private void RotateLevel()
        {
            originalLevel = new PlayingLevel(originalLevel);
            originalLevel.Rotate();
            level.Rotate();
            SetCustomLevel(originalLevel, level);
            RefreshLevel();
        }

        private void MirrorLevel()
        {
            originalLevel = new PlayingLevel(originalLevel);
            originalLevel.Mirror();
            level.Mirror();
            SetCustomLevel(originalLevel, level);
            RefreshLevel();
        }

        private void ChangeSize()
        {
            ChangeSizeDialog csd = new ChangeSizeDialog(level.Height, level.Width);
            csd.RowAndColumnInfo = String.Format("Current rows {0}, columns {1}", level.Height, level.Width);
            if (csd.ShowDialog() == DialogResult.OK)
            {
                level.ChangeSize(csd.Rows, csd.Columns);
                RefreshLevel();
            }
        }

        private void OpenLevelSet()
        {
            openFileDialog1.InitialDirectory = dataDirectory;
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "Sokoban Level Set Files (*.sok;*.txt;*.xsb)|*.sok;*.txt;*.xsb";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                LoadLevelSet(openFileDialog1.FileName);
            }
        }

        private void ChangeSkin()
        {
            openFileDialog1.InitialDirectory = dataDirectory;
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "Sokoban Skin Files (*.bmp)|*.bmp";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                LoadSkin(openFileDialog1.FileName);
            }
        }

        private void ToggleDoubleSize()
        {
            doubleSize = !doubleSize;
            doubleSizeToolStripMenuItem.Checked = doubleSize;
            scaleFactor = doubleSize ? 200 : 100;
            LoadSkin(skin);
            RefreshLevel();
        }

        private void AdjustScale(int amount)
        {
            scaleFactor += amount;
            LoadSkin(skin);
            RefreshLevel();
        }

        private void ToggleAllowPush()
        {
            allowPush = !allowPush;
            allowPushToolStripMenuItem.Checked = allowPush;
        }

        private void ToggleAllowPull()
        {
            allowPull = !allowPull;
            allowPullToolStripMenuItem.Checked = allowPull;
        }

        private void ToggleEditLevel()
        {
            editLevel = !editLevel;
            editLevelToolStripMenuItem.Checked = editLevel;
            pictureGrid.ContextMenuStrip = editLevel ? contextMenuStrip1 : null;
        }

        private void ToggleMarkAccessible()
        {
            markAccessible = !markAccessible;
            markAccessibleToolStripMenuItem.Checked = markAccessible;
            if (markAccessible)
            {
                level.MarkAccessible = true;
                RefreshLevel();
            }
            else
            {
                level.MarkAccessible = false;
                RefreshLevel();
            }
        }

        private void ToggleAutoFixWalls()
        {
            autoFixWalls = !autoFixWalls;
            autoFixWallsToolStripMenuItem.Checked = autoFixWalls;
            level.AutoFixWalls = autoFixWalls;
        }

        private void SaveLevel()
        {
            saveFileDialog1.InitialDirectory = dataDirectory;
            saveFileDialog1.FileName = "";
            if (!customLevel && !String.IsNullOrEmpty(levelSet.Name))
            {
                saveFileDialog1.FileName = String.Format("{0}-{1}.xml", levelSet.Name, currentLevelNumber + 1);
            }
            saveFileDialog1.Filter = "Sokoban Level Files (*.xml)|*.xml";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SaveLevel(saveFileDialog1.FileName);
            }
        }

        private void SaveLevel(string filename)
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(PlayingLevel));
                TextWriter writer = new StreamWriter(filename);
                ser.Serialize(writer, level);
                writer.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("serialization problem: {0}", ex.ToString());
            }
        }

        private void LoadLevel()
        {
            openFileDialog1.InitialDirectory = dataDirectory;
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "Sokoban Level Files (*.xml)|*.xml";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                LoadLevel(openFileDialog1.FileName);
            }
        }

        private void LoadLevel(string filename)
        {
            PlayingLevel newLevel = null;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(PlayingLevel));
                TextReader reader = new StreamReader(filename);
                newLevel = ser.Deserialize(reader) as PlayingLevel;
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Exception loading level: {0}", ex.ToString()));
                return;
            }

            if (newLevel.MoveList.Count > 0 && LevelUtils.IsSolutionCompatible(originalLevel, newLevel.MoveList))
            {
                PlayingLevel newPlayingLevel = new PlayingLevel(originalLevel);
                newPlayingLevel.SetSolution(newLevel.MoveList);
                for (int i = 0; i < newLevel.MoveCount; i++)
                {
                    newPlayingLevel.Redo();
                }
                SetLevel(newLevel, newPlayingLevel);
            }
            else
            {
                SetLevel(UnwindLevel(newLevel), newLevel);
                customLevel = true;
            }
            RefreshLevel();
        }

        private static PlayingLevel UnwindLevel(PlayingLevel level)
        {
            PlayingLevel newLevel = new PlayingLevel(level);
            while (newLevel.MoveCount > 0)
            {
                newLevel.Undo();
            }
            return newLevel;
        }

        private void CopySolution()
        {
            string solution = SolutionEncoder.EncodedSolution(level.MoveList);
            if (!String.IsNullOrEmpty(solution))
            {
                Clipboard.SetText(solution);
            }
        }

        private void CopyLevel()
        {
            Clipboard.SetText(level.AsText);
        }

        private void CopyLevelOneLine()
        {
            Clipboard.SetText(level.AsTextOneLine);
        }

        private void TrimLevel()
        {
            SetCustomLevel(LevelUtils.TrimLevel(level));
            RefreshLevel();
        }

        private void CleanLevel()
        {
            MoveList solution = level.MoveList;
            if (solution.Count == 0)
            {
                TrimLevel();
                return;
            }

            SetCustomLevel(LevelUtils.CleanLevel(UnwindLevel(level), solution));
            RefreshLevel();
        }

        private void NormalizeLevel()
        {
            SetCustomLevel(LevelUtils.NormalizeLevel(UnwindLevel(level)));
            RefreshLevel();
        }

        private void LevelEdited()
        {
            SetCustomLevel(new Level(level), level);
            RefreshLevel();
        }

        private void PasteSolutionOrLevel()
        {
            try
            {
                string text = Clipboard.GetText();
                if (SolutionEncoder.IsSolution(text))
                {
                    SetLevel(originalLevel);
                    MoveList moveList = SolutionEncoder.MoveList(text);
                    if (LevelUtils.IsSolutionCompatible(originalLevel, moveList))
                    {
                        level.SetSolution(moveList);
                    }
                    else
                    {
                        MessageBox.Show("Solution is not compatible with this level");
                    }
                }
                else
                {
                    SetCustomLevel(new PlayingLevel(LevelEncoder.DecodeLevel(text)));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Exception pasting: {0}", ex.Message));
            }
            RefreshLevel();
        }


        private void RefreshModified()
        {
            if (markAccessible)
            {
                RefreshLevel();
                return;
            }
            
            foreach (Coordinate2D coordinate in level.ModifiedCells)
            {
                RefreshCell(coordinate.Row, coordinate.Column);
            }

            RefreshStatus();
        }

        private void RefreshStatus()
        {

            string status = "";
            if (!customLevel)
            {
                status += String.Format("Level {0}, ", currentLevelNumber + 1);
            }
            status += String.Format("Moves {0}, Pushes {1}", level.MoveCount, level.PushCount);
            if (level.MoveCount < level.TotalMoves)
            {
                status += String.Format(", Total Moves {0}, Pushes {1}", level.TotalMoves, level.TotalPushes);
            }
            if (level.IsComplete)
            {
                status += " (completed)";
            }
            SetStatus(status);
        }

        private void RefreshCell(int row, int column)
        {
            Cell cell = level[row, column];
            if (markAccessible)
            {
                if (Level.IsAccessible(cell))
                {
                    cell &= ~Cell.Accessible;
                }
                else if (Level.IsJustFloor(cell))
                {
                    cell = Cell.Outside;
                }
            }
            Bitmap skin = table[cell];
            if (skin == null)
            {
                skin = new Bitmap(wScreen, hScreen);
                GetCornerBitmap(skin, row, column, -1, -1);
                GetCornerBitmap(skin, row, column, -1, 1);
                GetCornerBitmap(skin, row, column, 1, 1);
                GetCornerBitmap(skin, row, column, 1, -1);
            }
            pictureGrid.Pictures[row, column].Image = skin;
        }

        private void GetCornerBitmap(Bitmap skin, int row, int column, int v, int h)
        {
            bool horizontal = level.IsWall(row, column + h);
            bool vertical = level.IsWall(row + v, column);
            bool diagonal = level.IsWall(row + v, column + h);

            CornerType cornerType;
            if (horizontal && vertical && !diagonal)
            {
                cornerType = CornerType.Type1;
            }
            else if (horizontal && !vertical)
            {
                cornerType = CornerType.Type2;
            }
            else if (horizontal && vertical && diagonal)
            {
                cornerType = CornerType.Type3;
            }
            else if (!horizontal && vertical)
            {
                cornerType = CornerType.Type4;
            }
            else if (!horizontal && !vertical)
            {
                cornerType = CornerType.Type5;
            }
            else
            {
                throw new InvalidOperationException("Invalid corner type");
            }

            int x = (h == -1) ? 0 : wSplit;
            int y = (v == -1) ? 0 : hSplit;
            int halfWidth = (h == -1) ? wSplit : wScreen - wSplit;
            int halfHeight = (v == -1) ? hSplit : hScreen - hSplit;

            PictureGrid.CopyBitmap(skin, x, y, walls[cornerType], x, y, halfWidth, halfHeight);
        }

        private void openLevelSetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenLevelSet();
        }

        private void changeSkinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeSkin();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void previousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PreviousLevel();
        }

        private void nextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NextLevel();
        }

        private void goToLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GoToLevel();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Redo();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutBox about = new AboutBox())
            {
                about.ShowDialog();
            }
        }

        private void rotateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RotateLevel();
        }

        private void saveLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveLevel();
        }

        private void loadLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadLevel();
        }

        private void undoToBeginningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UndoToBeginning();
        }

        private void redoToEndToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RedoToEnd();
        }

        private void pasteSolutionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteSolutionOrLevel();
        }

        private void copySolutionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopySolution();
        }

        private void doubleSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleDoubleSize();
        }

        private void menuStrip1_SizeChanged(object sender, EventArgs e)
        {
            RefreshSize();
        }

        private void statusStrip1_SizeChanged(object sender, EventArgs e)
        {
            RefreshSize();
        }

        private void defaultLevelSetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadDefaultLevelSet();
        }

        private void defaultSkinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadDefaultSkin();
        }

        private bool inMouseDrag;
        private Coordinate2D mouseDownCoord;
        private Coordinate2D mouseUpCoord;

        private void pictureGrid_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDownCoord = GetMouseCell(e);
            if (e.Button == MouseButtons.Left)
            {
                if (e.Clicks == 2)
                {
                    inMouseDrag = false;
                    HandleDoubleClick();
                }
                else if (e.Clicks == 1)
                {
                    inMouseDrag = true;
                }
            }
        }

        private void pictureGrid_MouseUp(object sender, MouseEventArgs e)
        {
            mouseUpCoord = GetMouseCell(e);
            if (inMouseDrag)
            {
                if (mouseDownCoord != mouseUpCoord)
                {
                    HandleMouseDrag();
                }
                inMouseDrag = false;
            }
        }

        private Coordinate2D GetMouseCell(MouseEventArgs e)
        {
            PictureGrid.ImageHost picture = pictureGrid.GetPictureAt(e.X, e.Y);
            if (picture == null)
            {
                return Coordinate2D.Undefined;
            }
            return new Coordinate2D(picture.Row, picture.Column);
        }

        private void HandleMouseDrag()
        {
            if (!editLevel)
            {
                return;
            }
            if (!level.IsValid(mouseDownCoord))
            {
                return;
            }
            if (!level.IsValid(mouseUpCoord))
            {
                return;
            }
            if (level.IsEmpty(mouseDownCoord) && level.IsTarget(mouseDownCoord))
            {
                level.ClearTarget(mouseDownCoord);
                LevelEdited();
                level.AddTarget(mouseUpCoord);
                LevelEdited();
            }
            else
            {
                level.MoveOccupant(mouseDownCoord, mouseUpCoord);
                LevelEdited();
            }
        }

        private void HandleDoubleClick()
        {
            if (!editLevel)
            {
                return;
            }
            if (!level.IsValid(mouseDownCoord))
            {
                return;
            }
            if (!level.IsWall(mouseDownCoord))
            {
                level.AddWall(mouseDownCoord);
            }
            else
            {
                level.ClearWall(mouseDownCoord);
            }
            LevelEdited();
        }

        private void clearOccupantToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!level.IsValid(mouseDownCoord))
            {
                return;
            }
            level.ClearOccupant(mouseDownCoord);
            LevelEdited();
        }

        private void clearTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!level.IsValid(mouseDownCoord))
            {
                return;
            }
            level.ClearTarget(mouseDownCoord);
            LevelEdited();
        }

        private void clearWallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!level.IsValid(mouseDownCoord))
            {
                return;
            }
            level.ClearWall(mouseDownCoord);
            LevelEdited();
        }

        private void addSokobanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!level.IsValid(mouseDownCoord))
            {
                return;
            }
            level.AddOccupant(mouseDownCoord, Cell.Sokoban);
            LevelEdited();
        }

        private void addBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!level.IsValid(mouseDownCoord))
            {
                return;
            }
            level.AddOccupant(mouseDownCoord, Cell.Box);
            LevelEdited();
        }

        private void addTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!level.IsValid(mouseDownCoord))
            {
                return;
            }
            level.AddTarget(mouseDownCoord);
            LevelEdited();
        }

        private void addWallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!level.IsValid(mouseDownCoord))
            {
                return;
            }
            level.AddWall(mouseDownCoord);
            LevelEdited();
        }

        private void addUndefinedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!level.IsValid(mouseDownCoord))
            {
                return;
            }
            level[mouseDownCoord] = Cell.Undefined;
            LevelEdited();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshLevel();
        }

        private void editLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleEditLevel();
        }

        private void changeSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeSize();
        }

        private void allowPushToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleAllowPush();
        }

        private void allowPullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleAllowPull();
        }

        private void mirrorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MirrorLevel();
        }

        private void markAccessibleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleMarkAccessible();
        }

        private void solverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RunSolver();
        }

        private void backwardsModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BackwardsMode();
        }

        private void newWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewWindow();
        }

        private void copyLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyLevel();
        }

        private void informationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Information();
        }

        private void autoFixWallsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleAutoFixWalls();
        }

        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void MainWindow_DragDrop(object sender, DragEventArgs e)
        {
            string[] filenames = e.Data.GetData(DataFormats.FileDrop) as string[];
            string filename = filenames[0];
            LoadLevelSet(filename);
        }

        private void trimLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TrimLevel();
        }

        private void cleanLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CleanLevel();
        }

        private void copyLevelOneLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyLevelOneLine();
        }

        private void compressLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NormalizeLevel();
        }

        private void calculateDeadlocksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CalculateDeadlocks();
        }

        private void increaseSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AdjustScale(10);
        }

        private void decreaseSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AdjustScale(-10);
        }
    }
}
