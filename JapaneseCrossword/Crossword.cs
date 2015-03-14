using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace JapaneseCrossword
{
    public class Crossword
    {
        private readonly int rowsCount;
        private readonly int columnsCount;
        public IEnumerable<IEnumerable<int>> RowsInfo { get; private set; }
        public IEnumerable<IEnumerable<int>> ColumnsInfo { get; private set; }
        public CrosswordCell[,] Cells { get; private set; }
        public bool[] RowsToRefresh { get; private set; }
        public bool[] ColumnsToRefresh { get; private set; }

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

        public IEnumerable<CrosswordLine> GetRowsToRefresh()
        {
            return GetLines(true);
        }

        public IEnumerable<CrosswordLine> GetColumnsToRefresh()
        {
            return GetLines(false);
        }

        private IEnumerable<CrosswordLine> GetLines(bool isRow)
        {
            var toRefresh = isRow ? RowsToRefresh : ColumnsToRefresh;
            var lineInfo = isRow ? RowsInfo : ColumnsInfo;

            for (int i = 0; i < toRefresh.Length; i++)
            {
                yield return new CrosswordLine(GetLineCells(i, isRow), lineInfo.ElementAt(i), i);
            }
        }

        private IEnumerable<CrosswordCell> GetLineCells(int index, bool isRow)
        {
            var lineLength = isRow ? columnsCount : rowsCount;
            var lineCells = new CrosswordCell[lineLength];
            for (int i = 0; i < lineLength; i++)
            {
                var row = isRow ? index : i;
                var col = isRow ? i : index;
                lineCells[i] = Cells[row, col];
            }
            return lineCells;
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
            var toRefresh = isRow ? ColumnsToRefresh : RowsToRefresh;
            for (int i = 0; i < line.Length; i++)
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

    [TestFixture]
    class Crossword_should
    {
        [Test]
        public void create_correct_number_of_cells()
        {
            var rowsCount = 5;
            var columnsCount = 50;
            var rowinfo = Enumerable.Range(0, columnsCount).Select(i => new[] {1,2,3});
            var colinfo = Enumerable.Range(0, rowsCount).Select(i => new[]{999,888,777});

            var crossword = new Crossword(rowinfo, colinfo);

            Assert.That(crossword.Cells.GetLength(0), Is.EqualTo(columnsCount));
            Assert.That(crossword.Cells.GetLength(1), Is.EqualTo(rowsCount));
        }

        [Test]
        public void mark_inserted_line_as_no_needed_to_refresh()
        {
            var rowinfo = Enumerable.Range(0, 5).Select(i => new[] {1, 2});
            var colinfo = Enumerable.Range(0, 8).Select(i => new[] {2, 1});
            var crossword = new Crossword(rowinfo, colinfo);
            var columnIndex = 2;
            var lineCells = Enumerable.Range(0, 5).Select(i => CrosswordCell.Empty);
            var line = new CrosswordLine(lineCells, new[] {1, 2}, columnIndex);

            crossword.SetColumn(line);

            Assert.That(crossword.ColumnsToRefresh[columnIndex], Is.EqualTo(false));
        }

        //маркировать перепендикулярную линию, как нужню для обновления, если вставить линию, которая меняет ячейку перп. линии
        [Test]
        public void mark_perpendicular_line_to_refresh_when_insert_line_that_changes_perp_line_cell()
        {
            var rowinfo = Enumerable.Range(0, 10).Select(i => new[] {1, 1});
            var colinfo = Enumerable.Range(0, 3).Select(i => new[] {2, 2});
            var crossword = new Crossword(rowinfo, colinfo);
            var cells = Enumerable.Range(0, 10)
                .Select(i => CrosswordCell.Unknown);
            crossword.SetColumn(new CrosswordLine(cells, new[] {1}, 2)); //здесь 3-я колонка помечается как не нуждающаяся в обновлении

            crossword.SetRow(new CrosswordLine(new[] {CrosswordCell.Colored, CrosswordCell.Empty, CrosswordCell.Colored}, new[]{1,1}, 0));

            Assert.That(crossword.ColumnsToRefresh[2], Is.EqualTo(true));
        }
    }
}
