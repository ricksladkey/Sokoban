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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Sokoban.Player
{
    public partial class PictureGrid : PictureBox
    {
        public class ImageHost
        {
            private PictureGrid parent;
            private int row;
            private int column;

            public ImageHost(PictureGrid parent, int row, int column)
            {
                this.parent = parent;
                this.row = row;
                this.column = column;
            }

            private Image image;

            public Image Image
            {
                get
                {
                    return image;
                }
                set
                {
                    image = value;
                    parent.ImageChanged(row, column, image);
                }
            }

            public int Row
            {
                get
                {
                    return row;
                }
            }

            public int Column
            {
                get
                {
                    return column;
                }
            }
        }

        public class PictureCollection
        {
            public PictureCollection(PictureGrid parent, int rows, int columns)
            {
                imageArray = new ImageHost[rows, columns];
                for (int row = 0; row < rows; row++)
                {
                    for (int column = 0; column < columns; column++)
                    {
                        imageArray[row, column] = new ImageHost(parent, row, column);
                    }
                }
            }

            private ImageHost[,] imageArray;

            public ImageHost this[int row, int column]
            {
                get
                {
                    return imageArray[row, column];
                }
                set
                {
                    imageArray[row, column] = value;
                }
            }

            public int Height
            {
                get
                {
                    return imageArray.GetLength(0);
                }
            }

            public int Width
            {
                get
                {
                    return imageArray.GetLength(1);
                }
            }
        }

        public PictureGrid()
        {
            InitializeComponent();
        }

        private int rows;

        public int Rows
        {
            get
            {
                return rows;
            }
            set
            {
                rows = value;
                ApplySettings();
            }
        }

        private int columns;

        public int Columns
        {
            get
            {
                return columns;
            }
            set
            {
                columns = value;
                ApplySettings();
            }
        }

        private int height;

        public int PictureHeight
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
                ApplySettings();
            }
        }

        private int width;

        public int PictureWidth
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
                ApplySettings();
            }
        }

        private PictureCollection pictureCollection;

        public PictureCollection Pictures
        {
            get
            {
                return pictureCollection;
            }
        }

        public ImageHost GetPictureAt(int x, int y)
        {
            int row = y / height;
            int column = x / width;

            if (row < 0 || row >= rows)
            {
                return null;
            }
            if (column < 0 || column >= columns)
            {
                return null;
            }

            return pictureCollection[row, column];
        }

        private void ApplySettings()
        {
            Size = new Size(columns * width, rows * height);
            if (Size.Width == 0 || Size.Height == 0)
            {
                return;
            }

            if (pictureCollection == null || pictureCollection.Width != width || pictureCollection.Height != height)
            {
                pictureCollection = new PictureCollection(this, rows, columns);
            }

            if (Image == null || Image.Height != Size.Height || Image.Width != Size.Width)
            {
                Image = new Bitmap(Size.Width, Size.Height);
            }
        }

        private void ImageChanged(int row, int column, Image image)
        {
            StretchBitmap(Image, column * width, row * height, width, height, image, 0, 0, image.Width, image.Height);
            Invalidate(new Rectangle(column * width, row * height, height, width));
        }

        public static void CopyBitmap(Image dst, int dstX, int dstY, Image src, int srcX, int srcY, int width, int height)
        {
            using (Graphics g = Graphics.FromImage(dst))
            {
                g.DrawImage(src, new Rectangle(dstX, dstY, width, height), new Rectangle(srcX, srcY, width, height), GraphicsUnit.Pixel);
            }
        }

        public static void StretchBitmap(Image dst, int dstX, int dstY, int dstWidth, int dstHeight, Image src, int srcX, int srcY, int srcWidth, int srcHeight)
        {
            if (srcWidth == dstWidth && srcHeight == dstHeight)
            {
                CopyBitmap(dst, dstX, dstY, src, srcX, srcY, srcWidth, srcHeight);
                return;
            }

            using (Graphics g = Graphics.FromImage(dst))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                g.DrawImage(src, new RectangleF(dstX, dstY, dstWidth, dstHeight), new RectangleF(srcX - 0.49f, srcY - 0.49f, srcWidth, srcHeight), GraphicsUnit.Pixel);
            }
        }
    }
}
