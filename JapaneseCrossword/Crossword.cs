using System.Collections.Generic;
using System.Linq;

namespace JapaneseCrossword
{
    class Crossword
    {
        private readonly int rowsCount;
        private readonly int columnsCount;
        public IEnumerable<IEnumerable<int>> RowsInfo { get; private set; }
        public IEnumerable<IEnumerable<int>> ColumnsInfo { get; private set; }
        public CrosswordCell[,] Cells { get; private set; }
        public bool[] RowsToRefresh { get; private set; }
        public bool[] ColumnsToRefresh { get; set; }

        public Crossword(IEnumerable<IEnumerable<int>> rowsInfo, IEnumerable<IEnumerable<int>> columnsInfo)
        {
            RowsInfo = rowsInfo.ToList();
            ColumnsInfo = columnsInfo.ToList();
            rowsCount = RowsInfo.Count();
            columnsCount = ColumnsInfo.Count();
            Cells = new CrosswordCell[rowsCount, columnsCount];
            RowsToRefresh = InitFlagsArray(rowsCount);
            ColumnsToRefresh = InitFlagsArray(columnsCount);
        }

        private static bool[] InitFlagsArray(int count)
        {
            return Enumerable
                .Range(0, count)
                .Select(i => true)
                .ToArray();
        }

        public CrosswordLine GetRowToRefresh()
        {
            return GetLine(true);
        }

        public CrosswordLine GetColumnToRefresh()
        {
            return GetLine(false);
        }

        private CrosswordLine GetLine(bool isRow)
        {
            var index = GetIndexOfNeedtoRefresh(isRow);
            if (index == -1)
                return null;

            var count = isRow ? columnsCount : rowsCount;
            var lineInfo = isRow ? RowsInfo : ColumnsInfo;
            var lineCells = new List<CrosswordCell>();
            for (int i = 0; i < count; i++)
            {
                var row = isRow ? index : i;
                var col = isRow ? i : index;
                lineCells.Add(Cells[row, col]);
            }
            return new CrosswordLine(lineCells, lineInfo.ElementAt(index), index);
        }

        private int GetIndexOfNeedtoRefresh(bool isRow)
        {
            var arr = isRow ? RowsToRefresh : ColumnsToRefresh;
            if (arr.Any(r => r))
            {
                var nextLine = arr.First(r => r);
                var index = arr.ToList().IndexOf(nextLine);
                return index;
            }
            return -1;
        }

        public void SetRow(CrosswordLine line)
        {
            SetLine(line, true);
            RowsToRefresh[line.Index] = false;
        }

        public void SetColumn(CrosswordLine line)
        {
            SetLine(line, false);
            ColumnsToRefresh[line.Index] = false;
        }

        private void SetLine(CrosswordLine line, bool isRow)
        {
            var count = isRow ? columnsCount : rowsCount;
            var toRefresh = isRow ? ColumnsToRefresh : RowsToRefresh;
            for (int i = 0; i < count; i++)
            {
                var row = isRow ? line.Index : i;
                var col = isRow ? i : line.Index;

                var lineCell = line.Cells.ElementAt(i);
                if (Cells[row, col] != lineCell)
                {
                    toRefresh[i] = true;
                    Cells[row, col] = lineCell;
                }

                
            }
        }

        public bool HasLinesToUpldate()
        {
            return RowsToRefresh.Any(l => l) || ColumnsToRefresh.Any(l => l);
        }
    }

    public enum CrosswordCell
    {
        Unknown,
        Colored,
        Empty
    }
}
