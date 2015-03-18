using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace JapaneseCrossword
{
    public class CrosswordLine 
    {
        public CrosswordCell[] Cells { get; private set; }
        public int[] Blocks { get; private set; }
        public bool[] CanColor { get; set; }
        public bool[] CanEmpty { get; set; }
        public int Length { get; private set; }
        public int Index { get; private set; }

        public CrosswordLine(CrosswordCell[] cells, IEnumerable<int> blocks, int index)
        {
            Cells = cells;
            Blocks = blocks.ToArray();

            ValidateLine();

            CanColor = new bool[Cells.Length];
            CanEmpty = new bool[Cells.Length];
            Length = cells.Length;
            Index = index;
        }

        private void ValidateLine()
        {
            if (Blocks.Sum() + Blocks.Length - 1 > Cells.Length)
            {
                throw new IncorrectCrosswordException();
            }
        }

        public CrosswordCell this[int index]
        {
            get { return Cells[index]; }
            set { Cells[index] = value; }
        }
    }

    [TestFixture]
    class CrosswordLine_should
    {
        [Test]
        [ExpectedException(typeof(IncorrectCrosswordException))]
        public void throw_if_blocks_cannot_fit_line_cells()
        {
            var cells = new[]
            {
                CrosswordCell.Unknown, CrosswordCell.Colored, CrosswordCell.Empty, CrosswordCell.Colored,
                CrosswordCell.Unknown
            };
            var blocks = new[] {2, 2, 1};

            var line = new CrosswordLine(cells, blocks, 0);
        }
    }
}
