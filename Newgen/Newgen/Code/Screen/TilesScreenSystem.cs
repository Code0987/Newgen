using System.Collections.Generic;
using System.Windows;
using Newgen.Base;

namespace Newgen.Core
{
    public enum TileCellType
    {
        Native = 0,
        Html = 1,
        Generated = 2
    }

    public class TileCell
    {
        public int Column;
        public int Row;

        /// <summary>
        /// Initializes a new instance of the <see cref="TileCell"/> class.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="row">The row.</param>
        public TileCell(int column, int row)
        {
            this.Column = column;
            this.Row = row;
        }
    }

    public class ScreenMatrix
    {
        readonly List<List<byte>> matrix;
        private int maxrows;
        private int maxcols;

        /// <summary>
        /// Gets the columns count.
        /// </summary>
        public int ColumnsCount { get { return this.maxcols; } }

        /// <summary>
        /// Gets the rows count.
        /// </summary>
        public int RowsCount { get { return this.maxrows; } }

        /// <summary>
        /// Gets or sets the <see cref="System.Byte"/> with the specified x.
        /// </summary>
        public byte this[int x, int y]
        {
            get { return this.matrix[y][x]; }
            set { this.matrix[y][x] = value; }
        }

        /// <summary>
        /// Initilizes a news instance of Matrix class
        /// </summary>
        /// <param name="c">Columns count</param>
        /// <param name="r">Rows count</param>
        public ScreenMatrix(int c, int r)
        {
            this.matrix = new List<List<byte>>();
            this.maxcols = c;
            this.maxrows = r;
        }

        /// <summary>
        /// Gets the real columns count.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns></returns>
        public int GetRealColumnsCount(int row)
        {
            if (this.matrix.Count < row) return 0;

            return this.matrix[row].Count;
        }

        /// <summary>
        /// Gets the real rows count.
        /// </summary>
        /// <returns></returns>
        public int GetRealRowsCount()
        {
            return this.matrix.Count;
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(byte item)
        {
            if (this.matrix.Count == 0)
            {
                this.matrix.Add(new List<byte>() { item });
                return;
            }

            foreach (var row in this.matrix)
            {
                if (row.Count >= maxcols)
                    continue;
                row.Add(item);
                return;
            }

            if (this.matrix.Count < maxrows)
            {
                this.AddRow();
                this.Add(item);
                return;
            }

            if (this.matrix.Count == maxrows)
            {
                this.AddColumn();
                this.Add(item);
                return;
            }
        }

        /// <summary>
        /// Adds the row.
        /// </summary>
        public void AddRow()
        {
            this.matrix.Add(new List<byte>());
        }

        /// <summary>
        /// Adds the column.
        /// </summary>
        public void AddColumn()
        {
            this.maxcols++;
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            foreach (var row in this.matrix)
            {
                row.Clear();
            }

            this.matrix.Clear();
        }

        /// <summary>
        /// Zeroes the matrix.
        /// </summary>
        public void ZeroMatrix()
        {
            if (this.matrix.Count == 0)
            {
                for (var i = 0; i < this.maxrows * this.maxcols; i++)
                    this.Add(0);
            }
            else
            {
                for (var i = 0; i < this.maxrows; i++)
                    for (var j = 0; j < this.maxcols; j++)
                        this[j, i] = 0;
            }
        }

        /// <summary>
        /// Finds free cell with enough horizontal space
        /// </summary>
        /// <param name="length">Required horizontal space</param>
        /// <returns>
        /// Index of cell where widget can be placed. If there is no free cell, returns -1.
        /// </returns>
        public TileCell GetFreeCell(int length)
        {
            for (var i = 0; i < this.maxcols; i++)
            {
                var index = this.GetFreeRow(i, length);

                if (index != -1)
                {
                    var cell = new TileCell(i, index);
                    return cell;
                }
            }
            return new TileCell(-1, -1);
        }

        /// <summary>
        /// Gets the free row.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        private int GetFreeRow(int column, int length)
        {
            if (this.maxcols - column < length) return -1;

            for (var i = 0; i < this.maxrows; i++)
            {
                var isFree = true;

                for (var j = 0; j < length; j++)
                    if (this[column + j, i] == 1)
                    {
                        isFree = false;
                        break;
                    }
                if (isFree)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Reserves the space.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="row">The row.</param>
        /// <param name="length">The length.</param>
        public void ReserveSpace(int column, int row, int length)
        {
            if (row >= this.maxrows) return;

            for (var i = column; i < column + length; i++)
            {
                if (i >= this.maxcols) return;

                this[i, row] = 1;
            }
        }

        /// <summary>
        /// Frees the space.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="row">The row.</param>
        /// <param name="length">The length.</param>
        public void FreeSpace(int column, int row, int length)
        {
            if (row >= this.maxrows) return;

            for (var i = column; i < column + length; i++)
            {
                if (i >= this.maxcols) return;

                this[i, row] = 0;
            }
        }

        /// <summary>
        /// Determines whether [is cell free] [the specified column].
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="row">The row.</param>
        /// <param name="length">The length.</param>
        /// <returns>
        ///   <c>true</c> if [is cell free] [the specified column]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsCellFree(int column, int row, int length)
        {
            if (row >= this.maxrows) return false;

            bool result = true;

            for (int i = column; i < column + length; i++)
            {
                if (i >= this.maxcols) return false;
                if (this[i, row] == 1) result = false;
            }

            return result;
        }
    }

    public class WindowManager
    {
        private Rect region;

        /// <summary>
        /// Gets the matrix.
        /// </summary>
        public ScreenMatrix Matrix { get; private set; }

        /// <summary>
        /// Gets the region.
        /// </summary>
        public Rect Region { get { return this.region; } }

        /// <summary>
        /// Initializes the specified c.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <param name="r">The r.</param>
        public void Initialize(int c, int r)
        {
            this.region = new Rect();

            this.region.Height = SystemParameters.PrimaryScreenHeight - E.Margin.Top - E.Margin.Bottom;
            this.region.Width = SystemParameters.PrimaryScreenWidth;
            this.region.Y = E.Margin.Top;
            this.region.X = E.Margin.Left;

            E.ColumnsCount = c = (c == 0 || c < 5) ? 40 : c;
            E.RowsCount = r = (r == 0 || r < 3) ? 3 : r;

            this.Matrix = new ScreenMatrix(E.ColumnsCount, E.RowsCount);

            this.Matrix.ZeroMatrix();
        }
    }
}