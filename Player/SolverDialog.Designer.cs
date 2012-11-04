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

namespace Sokoban.Player
{
    partial class SolverDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.textBoxSolverInfo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel7 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.panel8 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonReset = new System.Windows.Forms.Button();
            this.checkBoxUseDeadlockCache = new System.Windows.Forms.CheckBox();
            this.checkBoxVerbose = new System.Windows.Forms.CheckBox();
            this.checkBoxCalculateDeadlocks = new System.Windows.Forms.CheckBox();
            this.checkBoxLowerBound = new System.Windows.Forms.CheckBox();
            this.checkBoxValidate = new System.Windows.Forms.CheckBox();
            this.checkBoxDetectNoInfluencePushes = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownNodes = new System.Windows.Forms.NumericUpDown();
            this.checkBoxOptimizePushes = new System.Windows.Forms.CheckBox();
            this.checkBoxOptimizeMoves = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownInitial = new System.Windows.Forms.NumericUpDown();
            this.panel5.SuspendLayout();
            this.panel7.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel8.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNodes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownInitial)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonOK.Location = new System.Drawing.Point(0, 10);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(90, 31);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonCancel.Location = new System.Drawing.Point(302, 10);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(103, 31);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // textBoxSolverInfo
            // 
            this.textBoxSolverInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxSolverInfo.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxSolverInfo.Location = new System.Drawing.Point(20, 248);
            this.textBoxSolverInfo.Multiline = true;
            this.textBoxSolverInfo.Name = "textBoxSolverInfo";
            this.textBoxSolverInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxSolverInfo.Size = new System.Drawing.Size(405, 179);
            this.textBoxSolverInfo.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(405, 36);
            this.label1.TabIndex = 3;
            this.label1.Text = "Solver Information";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.textBoxSolverInfo);
            this.panel5.Controls.Add(this.panel7);
            this.panel5.Controls.Add(this.panel6);
            this.panel5.Controls.Add(this.panel8);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(0, 0);
            this.panel5.Name = "panel5";
            this.panel5.Padding = new System.Windows.Forms.Padding(20);
            this.panel5.Size = new System.Drawing.Size(445, 488);
            this.panel5.TabIndex = 8;
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.label1);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel7.Location = new System.Drawing.Point(20, 212);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(405, 36);
            this.panel7.TabIndex = 5;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.buttonOK);
            this.panel6.Controls.Add(this.buttonCancel);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel6.Location = new System.Drawing.Point(20, 427);
            this.panel6.Name = "panel6";
            this.panel6.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.panel6.Size = new System.Drawing.Size(405, 41);
            this.panel6.TabIndex = 4;
            // 
            // panel8
            // 
            this.panel8.Controls.Add(this.groupBox1);
            this.panel8.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel8.Location = new System.Drawing.Point(20, 20);
            this.panel8.Name = "panel8";
            this.panel8.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.panel8.Size = new System.Drawing.Size(405, 192);
            this.panel8.TabIndex = 6;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.numericUpDownInitial);
            this.groupBox1.Controls.Add(this.buttonReset);
            this.groupBox1.Controls.Add(this.checkBoxUseDeadlockCache);
            this.groupBox1.Controls.Add(this.checkBoxVerbose);
            this.groupBox1.Controls.Add(this.checkBoxCalculateDeadlocks);
            this.groupBox1.Controls.Add(this.checkBoxLowerBound);
            this.groupBox1.Controls.Add(this.checkBoxValidate);
            this.groupBox1.Controls.Add(this.checkBoxDetectNoInfluencePushes);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.numericUpDownNodes);
            this.groupBox1.Controls.Add(this.checkBoxOptimizePushes);
            this.groupBox1.Controls.Add(this.checkBoxOptimizeMoves);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(405, 182);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options";
            // 
            // buttonReset
            // 
            this.buttonReset.Location = new System.Drawing.Point(302, 136);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(89, 40);
            this.buttonReset.TabIndex = 10;
            this.buttonReset.Text = "&Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // checkBoxUseDeadlockCache
            // 
            this.checkBoxUseDeadlockCache.AutoSize = true;
            this.checkBoxUseDeadlockCache.Location = new System.Drawing.Point(165, 68);
            this.checkBoxUseDeadlockCache.Name = "checkBoxUseDeadlockCache";
            this.checkBoxUseDeadlockCache.Size = new System.Drawing.Size(128, 17);
            this.checkBoxUseDeadlockCache.TabIndex = 9;
            this.checkBoxUseDeadlockCache.Text = "&Use Deadlock Cache";
            this.checkBoxUseDeadlockCache.UseVisualStyleBackColor = true;
            // 
            // checkBoxVerbose
            // 
            this.checkBoxVerbose.AutoSize = true;
            this.checkBoxVerbose.Location = new System.Drawing.Point(25, 159);
            this.checkBoxVerbose.Name = "checkBoxVerbose";
            this.checkBoxVerbose.Size = new System.Drawing.Size(65, 17);
            this.checkBoxVerbose.TabIndex = 8;
            this.checkBoxVerbose.Text = "Ver&bose";
            this.checkBoxVerbose.UseVisualStyleBackColor = true;
            // 
            // checkBoxCalculateDeadlocks
            // 
            this.checkBoxCalculateDeadlocks.AutoSize = true;
            this.checkBoxCalculateDeadlocks.Location = new System.Drawing.Point(25, 67);
            this.checkBoxCalculateDeadlocks.Name = "checkBoxCalculateDeadlocks";
            this.checkBoxCalculateDeadlocks.Size = new System.Drawing.Size(124, 17);
            this.checkBoxCalculateDeadlocks.TabIndex = 7;
            this.checkBoxCalculateDeadlocks.Text = "Calculate &Deadlocks";
            this.checkBoxCalculateDeadlocks.UseVisualStyleBackColor = true;
            // 
            // checkBoxLowerBound
            // 
            this.checkBoxLowerBound.AutoSize = true;
            this.checkBoxLowerBound.Location = new System.Drawing.Point(25, 90);
            this.checkBoxLowerBound.Name = "checkBoxLowerBound";
            this.checkBoxLowerBound.Size = new System.Drawing.Size(111, 17);
            this.checkBoxLowerBound.TabIndex = 6;
            this.checkBoxLowerBound.Text = "Use &Lower Bound";
            this.checkBoxLowerBound.UseVisualStyleBackColor = true;
            // 
            // checkBoxValidate
            // 
            this.checkBoxValidate.AutoSize = true;
            this.checkBoxValidate.Checked = true;
            this.checkBoxValidate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxValidate.Location = new System.Drawing.Point(25, 136);
            this.checkBoxValidate.Name = "checkBoxValidate";
            this.checkBoxValidate.Size = new System.Drawing.Size(64, 17);
            this.checkBoxValidate.TabIndex = 3;
            this.checkBoxValidate.Text = "&Validate";
            this.checkBoxValidate.UseVisualStyleBackColor = true;
            // 
            // checkBoxDetectNoInfluencePushes
            // 
            this.checkBoxDetectNoInfluencePushes.AutoSize = true;
            this.checkBoxDetectNoInfluencePushes.Checked = true;
            this.checkBoxDetectNoInfluencePushes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDetectNoInfluencePushes.Location = new System.Drawing.Point(25, 113);
            this.checkBoxDetectNoInfluencePushes.Name = "checkBoxDetectNoInfluencePushes";
            this.checkBoxDetectNoInfluencePushes.Size = new System.Drawing.Size(160, 17);
            this.checkBoxDetectNoInfluencePushes.TabIndex = 2;
            this.checkBoxDetectNoInfluencePushes.Text = "Detect No &Influence Pushes";
            this.checkBoxDetectNoInfluencePushes.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(165, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "&Node Limit";
            // 
            // numericUpDownNodes
            // 
            this.numericUpDownNodes.Increment = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDownNodes.Location = new System.Drawing.Point(165, 41);
            this.numericUpDownNodes.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.numericUpDownNodes.Minimum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDownNodes.Name = "numericUpDownNodes";
            this.numericUpDownNodes.Size = new System.Drawing.Size(85, 20);
            this.numericUpDownNodes.TabIndex = 5;
            this.numericUpDownNodes.ThousandsSeparator = true;
            this.numericUpDownNodes.Value = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            // 
            // checkBoxOptimizePushes
            // 
            this.checkBoxOptimizePushes.AutoSize = true;
            this.checkBoxOptimizePushes.Checked = true;
            this.checkBoxOptimizePushes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxOptimizePushes.Location = new System.Drawing.Point(25, 44);
            this.checkBoxOptimizePushes.Name = "checkBoxOptimizePushes";
            this.checkBoxOptimizePushes.Size = new System.Drawing.Size(104, 17);
            this.checkBoxOptimizePushes.TabIndex = 1;
            this.checkBoxOptimizePushes.Text = "Optimize &Pushes";
            this.checkBoxOptimizePushes.UseVisualStyleBackColor = true;
            // 
            // checkBoxOptimizeMoves
            // 
            this.checkBoxOptimizeMoves.AutoSize = true;
            this.checkBoxOptimizeMoves.Checked = true;
            this.checkBoxOptimizeMoves.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxOptimizeMoves.Location = new System.Drawing.Point(25, 20);
            this.checkBoxOptimizeMoves.Name = "checkBoxOptimizeMoves";
            this.checkBoxOptimizeMoves.Size = new System.Drawing.Size(101, 17);
            this.checkBoxOptimizeMoves.TabIndex = 0;
            this.checkBoxOptimizeMoves.Text = "Optimize &Moves";
            this.checkBoxOptimizeMoves.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(260, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Initial &Capacity";
            // 
            // numericUpDownInitial
            // 
            this.numericUpDownInitial.Increment = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDownInitial.Location = new System.Drawing.Point(260, 41);
            this.numericUpDownInitial.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.numericUpDownInitial.Minimum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDownInitial.Name = "numericUpDownInitial";
            this.numericUpDownInitial.Size = new System.Drawing.Size(85, 20);
            this.numericUpDownInitial.TabIndex = 12;
            this.numericUpDownInitial.ThousandsSeparator = true;
            this.numericUpDownInitial.Value = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            // 
            // SolverDialog
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(445, 488);
            this.Controls.Add(this.panel5);
            this.Name = "SolverDialog";
            this.Text = "Solver";
            this.Load += new System.EventHandler(this.SolverDialog_Load);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel7.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel8.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNodes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownInitial)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox textBoxSolverInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxDetectNoInfluencePushes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownNodes;
        private System.Windows.Forms.CheckBox checkBoxOptimizePushes;
        private System.Windows.Forms.CheckBox checkBoxOptimizeMoves;
        private System.Windows.Forms.CheckBox checkBoxValidate;
        private System.Windows.Forms.CheckBox checkBoxLowerBound;
        private System.Windows.Forms.CheckBox checkBoxCalculateDeadlocks;
        private System.Windows.Forms.CheckBox checkBoxVerbose;
        private System.Windows.Forms.CheckBox checkBoxUseDeadlockCache;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDownInitial;
    }
}