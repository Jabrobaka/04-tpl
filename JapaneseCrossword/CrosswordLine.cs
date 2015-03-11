using System;
using System.Collections.Generic;
using System.Linq;

namespace JapaneseCrossword
{
    public class CrosswordLine
    {
        public CrosswordCell[] Cells { get; set; }
        public IEnumerable<int> Block { get; set; }
        public bool[] CanColor { get; set; }
        public bool[] CanEmpty { get; set; }
        public int Length { get; set; }
        public int Index { get; set; }

        public CrosswordLine(IEnumerable<CrosswordCell> lineCells, IEnumerable<int> blocks, int index)
        {
            Cells = lineCells.ToArray();
            Block = blocks;

            if (Block.Sum() + Block.Count() - 1> Cells.Length)
            {
                throw new IncorrectCrosswordException();
            }

            CanColor = new bool[Cells.Length];
            CanEmpty = new bool[Cells.Length];
            Length = Cells.Length;
            Index = index;
        }

        public CrosswordCell this[int index]
        {
            get { return Cells.ElementAt(index); }
            set { Cells[index] = value; }
        }
    }
    
}
