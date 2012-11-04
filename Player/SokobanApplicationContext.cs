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
using System.Windows.Forms;
using System.Threading;

namespace Sokoban.Player
{
    /// <summary>
    /// Handle multiple Sokoban windows and unhandled exceptions.
    /// </summary>
    public class SokobanApplicationContext : ApplicationContext
    {
        private List<Form> windows;

        private static SokobanApplicationContext applicationContext;

        public static SokobanApplicationContext Instance
        {
            get
            {
                return applicationContext;
            }
        }

        public SokobanApplicationContext()
        {
            applicationContext = this;
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            windows = new List<Form>();
            Form window = new MainWindow();
            AddWindow(window);
            window.Show();
        }

        public void AddWindow(Form window)
        {
            windows.Add(window);
            window.FormClosed += new FormClosedEventHandler(OnFormClosed);
        }

        public void RemoveWindow(Form window)
        {
            windows.Remove(window);
            if (windows.Count == 0)
            {
                ExitThread();
            }
        }

        private void OnFormClosed(object sender, FormClosedEventArgs args)
        {
            Form window = (Form)sender;
            RemoveWindow(window);
        }

        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            try
            {
                switch (ShowThreadExceptionDialog(e.Exception))
                {
                    case DialogResult.Abort:
                        Application.Exit();
                        break;
                }
            }
            catch
            {
                try
                {
                    MessageBox.Show("Fatal Error", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
                finally
                {
                    Application.Exit();
                }
            }
        }

        private DialogResult ShowThreadExceptionDialog(Exception ex)
        {
            string errorMessage =
                "Unhandled Exception:\n\n" +
                ex.Message + "\n\n" +
                ex.GetType() +
                "\n\nStack Trace:\n" +
                ex.StackTrace;

            return MessageBox.Show(errorMessage, "Application Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
        }
    }
}
