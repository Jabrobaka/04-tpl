using System.Collections.Generic;
using System.Linq;

namespace JapaneseCrossword
{
    public class Crossword
    {
        private List<IEnumerable<int>> RowsInfo;
        private List<IEnumerable<int>> ColumnsInfo;
        private int rowsCount;
        private int columnsCount;
        public CrosswordCell[,] Cells { get; set; }

        public Crossword(IEnumerable<IEnumerable<int>> rowsInfo, IEnumerable<IEnumerable<int>> columnsInfo)
        {
            RowsInfo = rowsInfo.ToList();
            ColumnsInfo = columnsInfo.ToList();
            rowsCount = RowsInfo.Count();
            columnsCount = ColumnsInfo.Count();
            Cells = new CrosswordCell[rowsCount, columnsCount];
        }

        public CrosswordCell this[int row, int col]
        {
            get { return Cells[row, col]; }
            set { Cells[row, col] = value; }
        }

        public CrosswordLine GetRowByIndex(int index)
        {
            return GetLine(index, true);
        }

        public CrosswordLine GetColumnByIndex(int index)
        {
            return GetLine(index, false);
        }
//
        private CrosswordLine GetLine(int index, bool isRow)
        {
            var lineInfo = isRow ? RowsInfo : ColumnsInfo;
            return new CrosswordLine(GetLineCells(index, isRow), lineInfo.ElementAt(index), index);
        }

        private CrosswordCell[] GetLineCells(int index, bool isRow)
        {
            var lineLength = isRow ? columnsCount : rowsCount;
            var lineCells = new CrosswordCell[lineLength];
            for (int i = 0; i < lineLength; i++)
            {
                var row = isRow ? index : i;
                var col = isRow ? i : index;
                lineCells[i] = Cells[row, col];
            }
            return lineCells.ToArray();
        }

    }

    public enum CrosswordCell
    {
        Unknown,
        Colored,
        Empty
    }
}
