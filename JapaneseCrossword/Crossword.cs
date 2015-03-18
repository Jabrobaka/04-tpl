using System;
using System.Collections.Generic;
using System.Linq;

namespace JapaneseCrossword
{
    public class Crossword
    {
        private List<IEnumerable<int>> RowsInfo;
        private List<IEnumerable<int>> ColumnsInfo;
        private bool[] columnsToUpdate;
        private bool[] rowsToUpdate;
        public CrosswordCell[,] Cells { get; private set; }

        public Crossword(IEnumerable<IEnumerable<int>> rowsInfo, IEnumerable<IEnumerable<int>> columnsInfo)
        {
            RowsInfo = rowsInfo.ToList();
            ColumnsInfo = columnsInfo.ToList();
            rowsToUpdate = InitFlagsArray(RowsInfo.Count);
            columnsToUpdate = InitFlagsArray(ColumnsInfo.Count);
            Cells = new CrosswordCell[rowsToUpdate.Length, columnsToUpdate.Length];
        }

        private static bool[] InitFlagsArray(int count)
        {
            return Enumerable
                .Range(0, count)
                .Select(i => true)
                .ToArray();
        }

        public bool HasLinesToUpdate()
        {
            return rowsToUpdate.Any(l => l) || columnsToUpdate.Any(l => l);
        }

        public void SetLine(CrosswordLine line, LineType type)
        {
            var toUpdate = type == LineType.Row ? columnsToUpdate : rowsToUpdate;
            for (int i = 0; i < line.Length; i++)
            {
                var row = type == LineType.Row ? line.Index : i;
                var col = type == LineType.Row ? i : line.Index;
                if (Cells[row, col] != line[i])
                {
                    toUpdate[i] = true;
                    Cells[row, col] = line[i];
                }
            }
            (type == LineType.Row ? rowsToUpdate : columnsToUpdate)[line.Index] = false;
        }

        public IEnumerable<CrosswordLine> GetLinesToUpdate(LineType type)
        {
            var toUpdate = type == LineType.Row ? rowsToUpdate : columnsToUpdate;
            for (int i = 0; i < toUpdate.Length; i++)
            {
                if (toUpdate[i])
                    yield return GetLine(new LineDescription(i, type));
            }
        }

        private CrosswordLine GetLine(LineDescription description)
        {
            var lineInfo = description.Type == LineType.Row ? RowsInfo : ColumnsInfo;
            return new CrosswordLine(GetLineCells(description), lineInfo.ElementAt(description.Index), description.Index);
        }

        private CrosswordCell[] GetLineCells(LineDescription desc)
        {
            var lineLength = desc.Type == LineType.Row ? columnsToUpdate.Length : rowsToUpdate.Length;
            var lineCells = new CrosswordCell[lineLength];
            var cellGetter = GetCellsGetter(desc);
            for (int i = 0; i < lineLength; i++)
            {
                lineCells[i] = cellGetter(i);
            }
            return lineCells.ToArray();
        }

        private Func<int, CrosswordCell> GetCellsGetter(LineDescription desc)
        {
            if (desc.Type == LineType.Row)
                return i => Cells[desc.Index, i];
            return i => Cells[i, desc.Index];
        }
    }

    public enum CrosswordCell
    {
        Unknown,
        Colored,
        Empty
    }
}
