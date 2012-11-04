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
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

using Sokoban.Engine.Collections;
using Sokoban.Engine.Core;
using Sokoban.Engine.Deadlocks;
using Sokoban.Engine.Levels;
using Sokoban.Engine.Paths;
using Sokoban.Engine.Solvers;
using Sokoban.Engine.Utilities;

namespace Sokoban.Player
{
    public partial class SolverDialog : Form
    {
        private MainWindow mainWindow;
        private ISolver solver;
        private Thread solverThread;
        private MoveList solution;
        private bool solving;

        public SolverDialog(MainWindow mainWindow)
        {
            InitializeComponent();

            ResetOptions();

            this.mainWindow = mainWindow;
        }

        public MoveList Solution
        {
            get
            {
                return solution;
            }
        }

        private void SolverDialog_Load(object sender, EventArgs e)
        {
            ShowMemory("before");

            textBoxSolverInfo.Text = "";
            buttonOK.Text = "Solve";
            buttonOK.Enabled = true;

            solution = null;
            solving = false;
        }

        private void ResetOptions()
        {
            checkBoxOptimizeMoves.Checked = true;
            checkBoxOptimizePushes.Checked = true;
            checkBoxLowerBound.Checked = false;
            checkBoxCalculateDeadlocks.Checked = true;
            checkBoxUseDeadlockCache.Checked = false;
            checkBoxDetectNoInfluencePushes.Checked = true;
            checkBoxValidate.Checked = false;
            checkBoxVerbose.Checked = true;
            numericUpDownNodes.Value = 40000000;
            numericUpDownInitial.Value = 20000000;
        }

        private void SolverThreadEntry()
        {
            try
            {
                solver.Solve();
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Exception: {0}", ex.Message));
            }
            Invoke(new MethodInvoker(SolverThreadFinished));
        }

        private void SolverThreadFinished()
        {
            buttonOK.Enabled = true;
            buttonOK.Focus();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (!solving)
            {
                SolverAlgorithm algorithm = checkBoxLowerBound.Checked ?
                    SolverAlgorithm.LowerBound : SolverAlgorithm.BruteForce;
                solver = Solver.CreateInstance(algorithm);
                solver.Level = mainWindow.OriginalLevel;
                solver.OptimizeMoves = checkBoxOptimizeMoves.Checked;
                solver.OptimizePushes = checkBoxOptimizePushes.Checked;
                solver.CalculateDeadlocks = checkBoxCalculateDeadlocks.Checked;
                solver.DeadlocksDirectory = checkBoxUseDeadlockCache.Checked ? mainWindow.DeadlocksDirectory : null;
                solver.DetectNoInfluencePushes = checkBoxDetectNoInfluencePushes.Checked;
                solver.Validate = checkBoxValidate.Checked;
                solver.Verbose = checkBoxVerbose.Checked;

                solver.MaximumNodes = (int)numericUpDownNodes.Value;
                solver.InitialCapacity = (int)numericUpDownInitial.Value;

                solver.CancelInfo.Cancel = false;

                solving = true;
                buttonOK.Text = "OK";
                buttonOK.Enabled = false;

                solverThread = new Thread(new ThreadStart(SolverThreadEntry));
                solverThread.Start();

                timer1.Interval = 100;
                timer1.Enabled = true;
            }
            else
            {
                CleanupDialog();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (solving)
            {
                solver.CancelInfo.Cancel = true;
                CleanupDialog();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (solver != null)
            {
                try
                {
                    textBoxSolverInfo.Text = solver.CancelInfo.Info;
                }
                catch (Exception)
                {
                }
            }
        }

        private void CleanupDialog()
        {
            while (!solverThread.Join(10))
            {
                Application.DoEvents();
            }
            timer1.Enabled = false;
            if (solver != null)
            {
                textBoxSolverInfo.Text = solver.CancelInfo.Info;
                solution = solver.Solution;
                solver = null;
                GC.Collect();
            }
            solving = false;

            ShowMemory("after");

        }

        private void ShowMemory(string label)
        {
            Trace.WriteLine(String.Format("{0}: Total memory allocated: {1} MB", label, GC.GetTotalMemory(false) / 1000000));
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            ResetOptions();
        }
    }
}
