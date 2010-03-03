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
    public partial class GoToDialog : Form
    {
        public int Level
        {
            get
            {
                try
                {
                    return Int32.Parse(textBox1.Text);
                }
                catch
                {
                    return -1;
                }
            }
            set
            {
                textBox1.Text = value.ToString();
            }
        }

        public string LevelInfo
        {
            get
            {
                return labelLevelInfo.Text;
            }
            set
            {
                labelLevelInfo.Text = value;
            }
        }

        public GoToDialog(int initialValue)
        {
            InitializeComponent();

            textBox1.Text = initialValue.ToString();
        }
    }
}