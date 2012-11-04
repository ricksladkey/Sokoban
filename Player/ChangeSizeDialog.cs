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

namespace Sokoban.Player
{
    public partial class ChangeSizeDialog : Form
    {
        public ChangeSizeDialog(int rows, int columns)
        {
            InitializeComponent();

            Rows = rows;
            Columns = columns;
        }

        public string RowAndColumnInfo
        {
            get
            {
                return labelRowAndColumnInfo.Text;
            }
            set
            {
                labelRowAndColumnInfo.Text = value;
            }
        }

        public int Rows
        {
            get
            {
                return Int32.Parse(textBoxRows.Text);
            }
            set
            {
                textBoxRows.Text = value.ToString();
            }
        }

        public int Columns
        {
            get
            {
                return Int32.Parse(textBoxColumns.Text);
            }
            set
            {
                textBoxColumns.Text = value.ToString();
            }
        }
    }
}