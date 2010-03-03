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
    partial class MainWindow
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openLevelSetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultLevelSetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadLevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveLevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToBeginningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToEndToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copySolutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyLevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyLevelOneLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteSolutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeSkinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultSkinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.doubleSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.levelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.previousToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.goToLevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rotateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mirrorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backwardsModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.informationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allowPushToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allowPullToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editLevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.markAccessibleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calculateDeadlocksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.solverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoFixWallsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trimLevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cleanLevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.normalizeLevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureGrid = new Sokoban.Player.PictureGrid();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearOccupantToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearTargetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearWallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addSokobanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addBoxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addTargetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addWallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addUndefinedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip2 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.increaseSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.decreaseSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureGrid)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.statusStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 364);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(465, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            this.statusStrip1.SizeChanged += new System.EventHandler(this.statusStrip1_SizeChanged);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.levelToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(465, 27);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "Sokoban";
            this.menuStrip1.SizeChanged += new System.EventHandler(this.menuStrip1_SizeChanged);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openLevelSetToolStripMenuItem,
            this.defaultLevelSetToolStripMenuItem,
            this.loadLevelToolStripMenuItem,
            this.saveLevelToolStripMenuItem,
            this.newWindowToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl-X";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(45, 23);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openLevelSetToolStripMenuItem
            // 
            this.openLevelSetToolStripMenuItem.Name = "openLevelSetToolStripMenuItem";
            this.openLevelSetToolStripMenuItem.Size = new System.Drawing.Size(247, 24);
            this.openLevelSetToolStripMenuItem.Text = "&Open Level Set";
            this.openLevelSetToolStripMenuItem.Click += new System.EventHandler(this.openLevelSetToolStripMenuItem_Click);
            // 
            // defaultLevelSetToolStripMenuItem
            // 
            this.defaultLevelSetToolStripMenuItem.Name = "defaultLevelSetToolStripMenuItem";
            this.defaultLevelSetToolStripMenuItem.Size = new System.Drawing.Size(247, 24);
            this.defaultLevelSetToolStripMenuItem.Text = "&Default Level Set";
            this.defaultLevelSetToolStripMenuItem.Click += new System.EventHandler(this.defaultLevelSetToolStripMenuItem_Click);
            // 
            // loadLevelToolStripMenuItem
            // 
            this.loadLevelToolStripMenuItem.Name = "loadLevelToolStripMenuItem";
            this.loadLevelToolStripMenuItem.Size = new System.Drawing.Size(247, 24);
            this.loadLevelToolStripMenuItem.Text = "&Load Level";
            this.loadLevelToolStripMenuItem.Click += new System.EventHandler(this.loadLevelToolStripMenuItem_Click);
            // 
            // saveLevelToolStripMenuItem
            // 
            this.saveLevelToolStripMenuItem.Name = "saveLevelToolStripMenuItem";
            this.saveLevelToolStripMenuItem.Size = new System.Drawing.Size(247, 24);
            this.saveLevelToolStripMenuItem.Text = "&Save Level";
            this.saveLevelToolStripMenuItem.Click += new System.EventHandler(this.saveLevelToolStripMenuItem_Click);
            // 
            // newWindowToolStripMenuItem
            // 
            this.newWindowToolStripMenuItem.Name = "newWindowToolStripMenuItem";
            this.newWindowToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+W";
            this.newWindowToolStripMenuItem.Size = new System.Drawing.Size(247, 24);
            this.newWindowToolStripMenuItem.Text = "New &Window";
            this.newWindowToolStripMenuItem.Click += new System.EventHandler(this.newWindowToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+X";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(247, 24);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.undoToBeginningToolStripMenuItem,
            this.redoToEndToolStripMenuItem,
            this.copySolutionToolStripMenuItem,
            this.copyLevelToolStripMenuItem,
            this.copyLevelOneLineToolStripMenuItem,
            this.pasteSolutionToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(48, 23);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.ShortcutKeyDisplayString = "Backspace";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(314, 24);
            this.undoToolStripMenuItem.Text = "&Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.ShortcutKeyDisplayString = "Space";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(314, 24);
            this.redoToolStripMenuItem.Text = "&Redo";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
            // 
            // undoToBeginningToolStripMenuItem
            // 
            this.undoToBeginningToolStripMenuItem.Name = "undoToBeginningToolStripMenuItem";
            this.undoToBeginningToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Home";
            this.undoToBeginningToolStripMenuItem.Size = new System.Drawing.Size(314, 24);
            this.undoToBeginningToolStripMenuItem.Text = "Undo to &Beginning";
            this.undoToBeginningToolStripMenuItem.Click += new System.EventHandler(this.undoToBeginningToolStripMenuItem_Click);
            // 
            // redoToEndToolStripMenuItem
            // 
            this.redoToEndToolStripMenuItem.Name = "redoToEndToolStripMenuItem";
            this.redoToEndToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+End";
            this.redoToEndToolStripMenuItem.Size = new System.Drawing.Size(314, 24);
            this.redoToEndToolStripMenuItem.Text = "Redo to &End";
            this.redoToEndToolStripMenuItem.Click += new System.EventHandler(this.redoToEndToolStripMenuItem_Click);
            // 
            // copySolutionToolStripMenuItem
            // 
            this.copySolutionToolStripMenuItem.Name = "copySolutionToolStripMenuItem";
            this.copySolutionToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+C";
            this.copySolutionToolStripMenuItem.Size = new System.Drawing.Size(314, 24);
            this.copySolutionToolStripMenuItem.Text = "&Copy Solution";
            this.copySolutionToolStripMenuItem.Click += new System.EventHandler(this.copySolutionToolStripMenuItem_Click);
            // 
            // copyLevelToolStripMenuItem
            // 
            this.copyLevelToolStripMenuItem.Name = "copyLevelToolStripMenuItem";
            this.copyLevelToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Q";
            this.copyLevelToolStripMenuItem.Size = new System.Drawing.Size(314, 24);
            this.copyLevelToolStripMenuItem.Text = "Copy &Level";
            this.copyLevelToolStripMenuItem.Click += new System.EventHandler(this.copyLevelToolStripMenuItem_Click);
            // 
            // copyLevelOneLineToolStripMenuItem
            // 
            this.copyLevelOneLineToolStripMenuItem.Name = "copyLevelOneLineToolStripMenuItem";
            this.copyLevelOneLineToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+O";
            this.copyLevelOneLineToolStripMenuItem.Size = new System.Drawing.Size(314, 24);
            this.copyLevelOneLineToolStripMenuItem.Text = "Copy Level &One Line";
            this.copyLevelOneLineToolStripMenuItem.Click += new System.EventHandler(this.copyLevelOneLineToolStripMenuItem_Click);
            // 
            // pasteSolutionToolStripMenuItem
            // 
            this.pasteSolutionToolStripMenuItem.Name = "pasteSolutionToolStripMenuItem";
            this.pasteSolutionToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+V";
            this.pasteSolutionToolStripMenuItem.Size = new System.Drawing.Size(314, 24);
            this.pasteSolutionToolStripMenuItem.Text = "&Paste Solution or Level";
            this.pasteSolutionToolStripMenuItem.Click += new System.EventHandler(this.pasteSolutionToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeSkinToolStripMenuItem,
            this.defaultSkinToolStripMenuItem,
            this.doubleSizeToolStripMenuItem,
            this.refreshToolStripMenuItem,
            this.increaseSizeToolStripMenuItem,
            this.decreaseSizeToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(55, 23);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // changeSkinToolStripMenuItem
            // 
            this.changeSkinToolStripMenuItem.Name = "changeSkinToolStripMenuItem";
            this.changeSkinToolStripMenuItem.Size = new System.Drawing.Size(277, 24);
            this.changeSkinToolStripMenuItem.Text = "&Change Skin";
            this.changeSkinToolStripMenuItem.Click += new System.EventHandler(this.changeSkinToolStripMenuItem_Click);
            // 
            // defaultSkinToolStripMenuItem
            // 
            this.defaultSkinToolStripMenuItem.Name = "defaultSkinToolStripMenuItem";
            this.defaultSkinToolStripMenuItem.Size = new System.Drawing.Size(277, 24);
            this.defaultSkinToolStripMenuItem.Text = "De&fault Skin";
            this.defaultSkinToolStripMenuItem.Click += new System.EventHandler(this.defaultSkinToolStripMenuItem_Click);
            // 
            // doubleSizeToolStripMenuItem
            // 
            this.doubleSizeToolStripMenuItem.Name = "doubleSizeToolStripMenuItem";
            this.doubleSizeToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Z";
            this.doubleSizeToolStripMenuItem.Size = new System.Drawing.Size(277, 24);
            this.doubleSizeToolStripMenuItem.Text = "&Double Size";
            this.doubleSizeToolStripMenuItem.Click += new System.EventHandler(this.doubleSizeToolStripMenuItem_Click);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+L";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(277, 24);
            this.refreshToolStripMenuItem.Text = "&Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // levelToolStripMenuItem
            // 
            this.levelToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.previousToolStripMenuItem,
            this.nextToolStripMenuItem,
            this.goToLevelToolStripMenuItem,
            this.rotateToolStripMenuItem,
            this.mirrorToolStripMenuItem,
            this.backwardsModeToolStripMenuItem,
            this.informationToolStripMenuItem});
            this.levelToolStripMenuItem.Name = "levelToolStripMenuItem";
            this.levelToolStripMenuItem.Size = new System.Drawing.Size(57, 23);
            this.levelToolStripMenuItem.Text = "&Level";
            // 
            // previousToolStripMenuItem
            // 
            this.previousToolStripMenuItem.Name = "previousToolStripMenuItem";
            this.previousToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+P";
            this.previousToolStripMenuItem.Size = new System.Drawing.Size(266, 24);
            this.previousToolStripMenuItem.Text = "&Previous";
            this.previousToolStripMenuItem.Click += new System.EventHandler(this.previousToolStripMenuItem_Click);
            // 
            // nextToolStripMenuItem
            // 
            this.nextToolStripMenuItem.Name = "nextToolStripMenuItem";
            this.nextToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+N";
            this.nextToolStripMenuItem.Size = new System.Drawing.Size(266, 24);
            this.nextToolStripMenuItem.Text = "&Next";
            this.nextToolStripMenuItem.Click += new System.EventHandler(this.nextToolStripMenuItem_Click);
            // 
            // goToLevelToolStripMenuItem
            // 
            this.goToLevelToolStripMenuItem.Name = "goToLevelToolStripMenuItem";
            this.goToLevelToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+G";
            this.goToLevelToolStripMenuItem.Size = new System.Drawing.Size(266, 24);
            this.goToLevelToolStripMenuItem.Text = "&Go To Level";
            this.goToLevelToolStripMenuItem.Click += new System.EventHandler(this.goToLevelToolStripMenuItem_Click);
            // 
            // rotateToolStripMenuItem
            // 
            this.rotateToolStripMenuItem.Name = "rotateToolStripMenuItem";
            this.rotateToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+R";
            this.rotateToolStripMenuItem.Size = new System.Drawing.Size(266, 24);
            this.rotateToolStripMenuItem.Text = "&Rotate";
            this.rotateToolStripMenuItem.Click += new System.EventHandler(this.rotateToolStripMenuItem_Click);
            // 
            // mirrorToolStripMenuItem
            // 
            this.mirrorToolStripMenuItem.Name = "mirrorToolStripMenuItem";
            this.mirrorToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+M";
            this.mirrorToolStripMenuItem.Size = new System.Drawing.Size(266, 24);
            this.mirrorToolStripMenuItem.Text = "&Mirror";
            this.mirrorToolStripMenuItem.Click += new System.EventHandler(this.mirrorToolStripMenuItem_Click);
            // 
            // backwardsModeToolStripMenuItem
            // 
            this.backwardsModeToolStripMenuItem.Name = "backwardsModeToolStripMenuItem";
            this.backwardsModeToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+B";
            this.backwardsModeToolStripMenuItem.Size = new System.Drawing.Size(266, 24);
            this.backwardsModeToolStripMenuItem.Text = "&Backwards Mode";
            this.backwardsModeToolStripMenuItem.Click += new System.EventHandler(this.backwardsModeToolStripMenuItem_Click);
            // 
            // informationToolStripMenuItem
            // 
            this.informationToolStripMenuItem.Name = "informationToolStripMenuItem";
            this.informationToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+I";
            this.informationToolStripMenuItem.Size = new System.Drawing.Size(266, 24);
            this.informationToolStripMenuItem.Text = "&Information";
            this.informationToolStripMenuItem.Click += new System.EventHandler(this.informationToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allowPushToolStripMenuItem,
            this.allowPullToolStripMenuItem,
            this.editLevelToolStripMenuItem,
            this.markAccessibleToolStripMenuItem,
            this.calculateDeadlocksToolStripMenuItem,
            this.solverToolStripMenuItem,
            this.autoFixWallsToolStripMenuItem,
            this.trimLevelToolStripMenuItem,
            this.cleanLevelToolStripMenuItem,
            this.normalizeLevelToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(60, 23);
            this.toolsToolStripMenuItem.Text = "&Tools";
            // 
            // allowPushToolStripMenuItem
            // 
            this.allowPushToolStripMenuItem.Name = "allowPushToolStripMenuItem";
            this.allowPushToolStripMenuItem.Size = new System.Drawing.Size(290, 24);
            this.allowPushToolStripMenuItem.Text = "Allow Pus&h";
            this.allowPushToolStripMenuItem.Click += new System.EventHandler(this.allowPushToolStripMenuItem_Click);
            // 
            // allowPullToolStripMenuItem
            // 
            this.allowPullToolStripMenuItem.Name = "allowPullToolStripMenuItem";
            this.allowPullToolStripMenuItem.Size = new System.Drawing.Size(290, 24);
            this.allowPullToolStripMenuItem.Text = "Allow Pul&l";
            this.allowPullToolStripMenuItem.Click += new System.EventHandler(this.allowPullToolStripMenuItem_Click);
            // 
            // editLevelToolStripMenuItem
            // 
            this.editLevelToolStripMenuItem.Name = "editLevelToolStripMenuItem";
            this.editLevelToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+E";
            this.editLevelToolStripMenuItem.Size = new System.Drawing.Size(290, 24);
            this.editLevelToolStripMenuItem.Text = "&Edit Level";
            this.editLevelToolStripMenuItem.Click += new System.EventHandler(this.editLevelToolStripMenuItem_Click);
            // 
            // markAccessibleToolStripMenuItem
            // 
            this.markAccessibleToolStripMenuItem.Name = "markAccessibleToolStripMenuItem";
            this.markAccessibleToolStripMenuItem.Size = new System.Drawing.Size(290, 24);
            this.markAccessibleToolStripMenuItem.Text = "&Mark Accessible";
            this.markAccessibleToolStripMenuItem.Click += new System.EventHandler(this.markAccessibleToolStripMenuItem_Click);
            // 
            // calculateDeadlocksToolStripMenuItem
            // 
            this.calculateDeadlocksToolStripMenuItem.Name = "calculateDeadlocksToolStripMenuItem";
            this.calculateDeadlocksToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+D";
            this.calculateDeadlocksToolStripMenuItem.Size = new System.Drawing.Size(290, 24);
            this.calculateDeadlocksToolStripMenuItem.Text = "Calculate &Deadlocks";
            this.calculateDeadlocksToolStripMenuItem.Click += new System.EventHandler(this.calculateDeadlocksToolStripMenuItem_Click);
            // 
            // solverToolStripMenuItem
            // 
            this.solverToolStripMenuItem.Name = "solverToolStripMenuItem";
            this.solverToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+S";
            this.solverToolStripMenuItem.Size = new System.Drawing.Size(290, 24);
            this.solverToolStripMenuItem.Text = "Run &Solver";
            this.solverToolStripMenuItem.Click += new System.EventHandler(this.solverToolStripMenuItem_Click);
            // 
            // autoFixWallsToolStripMenuItem
            // 
            this.autoFixWallsToolStripMenuItem.Name = "autoFixWallsToolStripMenuItem";
            this.autoFixWallsToolStripMenuItem.Size = new System.Drawing.Size(290, 24);
            this.autoFixWallsToolStripMenuItem.Text = "Auto-Fix &Walls";
            this.autoFixWallsToolStripMenuItem.Click += new System.EventHandler(this.autoFixWallsToolStripMenuItem_Click);
            // 
            // trimLevelToolStripMenuItem
            // 
            this.trimLevelToolStripMenuItem.Name = "trimLevelToolStripMenuItem";
            this.trimLevelToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+T";
            this.trimLevelToolStripMenuItem.Size = new System.Drawing.Size(290, 24);
            this.trimLevelToolStripMenuItem.Text = "&Trim Level";
            this.trimLevelToolStripMenuItem.Click += new System.EventHandler(this.trimLevelToolStripMenuItem_Click);
            // 
            // cleanLevelToolStripMenuItem
            // 
            this.cleanLevelToolStripMenuItem.Name = "cleanLevelToolStripMenuItem";
            this.cleanLevelToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+K";
            this.cleanLevelToolStripMenuItem.Size = new System.Drawing.Size(290, 24);
            this.cleanLevelToolStripMenuItem.Text = "&Clean Level";
            this.cleanLevelToolStripMenuItem.Click += new System.EventHandler(this.cleanLevelToolStripMenuItem_Click);
            // 
            // normalizeLevelToolStripMenuItem
            // 
            this.normalizeLevelToolStripMenuItem.Name = "normalizeLevelToolStripMenuItem";
            this.normalizeLevelToolStripMenuItem.Size = new System.Drawing.Size(290, 24);
            this.normalizeLevelToolStripMenuItem.Text = "&Normalize Level";
            this.normalizeLevelToolStripMenuItem.Click += new System.EventHandler(this.compressLevelToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(53, 23);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(137, 24);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.pictureGrid);
            this.panel1.Location = new System.Drawing.Point(0, 30);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(218, 153);
            this.panel1.TabIndex = 3;
            // 
            // pictureGrid
            // 
            this.pictureGrid.Columns = 0;
            this.pictureGrid.Location = new System.Drawing.Point(0, 0);
            this.pictureGrid.Name = "pictureGrid";
            this.pictureGrid.PictureHeight = 0;
            this.pictureGrid.PictureWidth = 0;
            this.pictureGrid.Rows = 0;
            this.pictureGrid.Size = new System.Drawing.Size(132, 91);
            this.pictureGrid.TabIndex = 0;
            this.pictureGrid.TabStop = false;
            this.pictureGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureGrid_MouseDown);
            this.pictureGrid.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureGrid_MouseUp);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearOccupantToolStripMenuItem,
            this.clearTargetToolStripMenuItem,
            this.clearWallToolStripMenuItem,
            this.addSokobanToolStripMenuItem,
            this.addBoxToolStripMenuItem,
            this.addTargetToolStripMenuItem,
            this.addWallToolStripMenuItem,
            this.addUndefinedToolStripMenuItem,
            this.changeSizeToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(202, 220);
            // 
            // clearOccupantToolStripMenuItem
            // 
            this.clearOccupantToolStripMenuItem.Name = "clearOccupantToolStripMenuItem";
            this.clearOccupantToolStripMenuItem.Size = new System.Drawing.Size(201, 24);
            this.clearOccupantToolStripMenuItem.Text = "&Clear Occupant";
            this.clearOccupantToolStripMenuItem.Click += new System.EventHandler(this.clearOccupantToolStripMenuItem_Click);
            // 
            // clearTargetToolStripMenuItem
            // 
            this.clearTargetToolStripMenuItem.Name = "clearTargetToolStripMenuItem";
            this.clearTargetToolStripMenuItem.Size = new System.Drawing.Size(201, 24);
            this.clearTargetToolStripMenuItem.Text = "C&lear Target";
            this.clearTargetToolStripMenuItem.Click += new System.EventHandler(this.clearTargetToolStripMenuItem_Click);
            // 
            // clearWallToolStripMenuItem
            // 
            this.clearWallToolStripMenuItem.Name = "clearWallToolStripMenuItem";
            this.clearWallToolStripMenuItem.Size = new System.Drawing.Size(201, 24);
            this.clearWallToolStripMenuItem.Text = "Cl&ear Wall";
            this.clearWallToolStripMenuItem.Click += new System.EventHandler(this.clearWallToolStripMenuItem_Click);
            // 
            // addSokobanToolStripMenuItem
            // 
            this.addSokobanToolStripMenuItem.Name = "addSokobanToolStripMenuItem";
            this.addSokobanToolStripMenuItem.Size = new System.Drawing.Size(201, 24);
            this.addSokobanToolStripMenuItem.Text = "Add &Sokoban";
            this.addSokobanToolStripMenuItem.Click += new System.EventHandler(this.addSokobanToolStripMenuItem_Click);
            // 
            // addBoxToolStripMenuItem
            // 
            this.addBoxToolStripMenuItem.Name = "addBoxToolStripMenuItem";
            this.addBoxToolStripMenuItem.Size = new System.Drawing.Size(201, 24);
            this.addBoxToolStripMenuItem.Text = "Add &Box";
            this.addBoxToolStripMenuItem.Click += new System.EventHandler(this.addBoxToolStripMenuItem_Click);
            // 
            // addTargetToolStripMenuItem
            // 
            this.addTargetToolStripMenuItem.Name = "addTargetToolStripMenuItem";
            this.addTargetToolStripMenuItem.Size = new System.Drawing.Size(201, 24);
            this.addTargetToolStripMenuItem.Text = "Add &Target";
            this.addTargetToolStripMenuItem.Click += new System.EventHandler(this.addTargetToolStripMenuItem_Click);
            // 
            // addWallToolStripMenuItem
            // 
            this.addWallToolStripMenuItem.Name = "addWallToolStripMenuItem";
            this.addWallToolStripMenuItem.Size = new System.Drawing.Size(201, 24);
            this.addWallToolStripMenuItem.Text = "Add &Wall";
            this.addWallToolStripMenuItem.Click += new System.EventHandler(this.addWallToolStripMenuItem_Click);
            // 
            // addUndefinedToolStripMenuItem
            // 
            this.addUndefinedToolStripMenuItem.Name = "addUndefinedToolStripMenuItem";
            this.addUndefinedToolStripMenuItem.Size = new System.Drawing.Size(201, 24);
            this.addUndefinedToolStripMenuItem.Text = "Add &Undefined";
            this.addUndefinedToolStripMenuItem.Click += new System.EventHandler(this.addUndefinedToolStripMenuItem_Click);
            // 
            // changeSizeToolStripMenuItem
            // 
            this.changeSizeToolStripMenuItem.Name = "changeSizeToolStripMenuItem";
            this.changeSizeToolStripMenuItem.Size = new System.Drawing.Size(201, 24);
            this.changeSizeToolStripMenuItem.Text = "Chan&ge Size";
            this.changeSizeToolStripMenuItem.Click += new System.EventHandler(this.changeSizeToolStripMenuItem_Click);
            // 
            // statusStrip2
            // 
            this.statusStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel2});
            this.statusStrip2.Location = new System.Drawing.Point(0, 340);
            this.statusStrip2.Name = "statusStrip2";
            this.statusStrip2.Size = new System.Drawing.Size(465, 24);
            this.statusStrip2.SizingGrip = false;
            this.statusStrip2.TabIndex = 4;
            this.statusStrip2.Text = "statusStrip2";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(158, 19);
            this.toolStripStatusLabel2.Text = "toolStripStatusLabel2";
            // 
            // increaseSizeToolStripMenuItem
            // 
            this.increaseSizeToolStripMenuItem.Name = "increaseSizeToolStripMenuItem";
            this.increaseSizeToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Up";
            this.increaseSizeToolStripMenuItem.Size = new System.Drawing.Size(277, 24);
            this.increaseSizeToolStripMenuItem.Text = "&Increase Size";
            this.increaseSizeToolStripMenuItem.Click += new System.EventHandler(this.increaseSizeToolStripMenuItem_Click);
            // 
            // decreaseSizeToolStripMenuItem
            // 
            this.decreaseSizeToolStripMenuItem.Name = "decreaseSizeToolStripMenuItem";
            this.decreaseSizeToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Down";
            this.decreaseSizeToolStripMenuItem.Size = new System.Drawing.Size(277, 24);
            this.decreaseSizeToolStripMenuItem.Text = "&Decrease Size";
            this.decreaseSizeToolStripMenuItem.Click += new System.EventHandler(this.decreaseSizeToolStripMenuItem_Click);
            // 
            // MainWindow
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(465, 386);
            this.Controls.Add(this.statusStrip2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainWindow";
            this.Text = "Sokoban";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainWindow_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainWindow_DragEnter);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureGrid)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.statusStrip2.ResumeLayout(false);
            this.statusStrip2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PictureGrid pictureGrid;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openLevelSetToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem levelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem previousToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem goToLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rotateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveLevelToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem loadLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeSkinToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem doubleSizeToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStripMenuItem defaultLevelSetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defaultSkinToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem clearOccupantToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearTargetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSokobanToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addBoxToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addTargetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addWallToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearWallToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToBeginningToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToEndToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copySolutionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteSolutionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allowPushToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allowPullToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeSizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mirrorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem markAccessibleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem solverToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backwardsModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem informationToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripMenuItem addUndefinedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoFixWallsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem trimLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cleanLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyLevelOneLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem normalizeLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem calculateDeadlocksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem increaseSizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem decreaseSizeToolStripMenuItem;
    }
}

