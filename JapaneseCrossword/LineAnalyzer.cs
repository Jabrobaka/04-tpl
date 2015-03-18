using System.Linq;

namespace JapaneseCrossword
{
    class LineAnalyzer
    {
        private readonly CrosswordLine line;

        public LineAnalyzer(CrosswordLine line)
        {
            this.line = line;
        }

        public CrosswordLine AnalyzeLine()
        {
            CanPlaceBlocks(-1, -1); 

            return ApplyAnalysisResultsToLine();
        }

        private bool PreviousCellHasColor(int i)
        {
            return i > 0 && line.Cells[i - 1] == CrosswordCell.Colored;
        }

        /// <summary>
        /// можно-ли поместить все блоки, начиная с блока с номером <paramref name="blockIndex"/>,
        /// если первый блок будет в  ячейке <paramref name="startIndex"/>
        /// </summary>
        private bool CanPlaceBlocks(int startIndex, int blockIndex)
        {
            var blockEndIndex = startIndex;
            if (blockEndIndex != -1)
            {
                blockEndIndex += line.Blocks[blockIndex];
                if (blockEndIndex > line.Length) //если блок уже не поместится в линию
                    return false;
                if (!CanPlaceCurrentBlock(startIndex, blockEndIndex))
                    return false;
            }

            if (!IsLastBlock(blockIndex)) 
            {
                return CheckRemainingCells(startIndex, blockIndex);
            }

            if (HasColorCellAfterLastBlock(blockEndIndex)) return false;

            ColorCells(startIndex, blockEndIndex); 
            UncolorCells(blockEndIndex, line.Length); 
            return true;
        }

        private bool HasColorCellAfterLastBlock(int blockEndIndex)
        {
            for (int i = blockEndIndex; i < line.Length; i++)
                if (line.Cells[i] == CrosswordCell.Colored)
                    return true;
            return false;
        }

        private bool CanPlaceCurrentBlock(int startIndex, int blockEndIndex)
        {
            for (int i = startIndex; i < blockEndIndex; i++)
                if (line.Cells[i] == CrosswordCell.Empty)
                    return false;
            return true;
        }

        private bool CheckRemainingCells(int startIndex, int blockIndex)
        {
            var result = false;
            var blockEndIndex = startIndex + (blockIndex == -1 ? 0 : line.Blocks[blockIndex]);
            for (int startNext = blockEndIndex + 1;
                startNext <= line.Length - line.Blocks[blockIndex + 1] + 1;
                startNext++)
            {
                if (PreviousCellHasColor(startNext)) 
                    break;

                if (CanPlaceBlocks(startNext, blockIndex + 1))
                {
                    result = true;
                    if (blockIndex != -1)
                    {
                        ColorCells(startIndex, blockEndIndex);
                        UncolorCells(blockEndIndex, startNext);
                    }
                    else UncolorCells(0, startNext);
                }
            }
            return result;
        }

        private bool IsLastBlock(int blockIndex)
        {
            return blockIndex == line.Blocks.Length - 1;
        }

        private void UncolorCells(int start, int end)
        {
            for (int i = start; i < end; i++)
                line.CanEmpty[i] = true;
        }

        private void ColorCells(int start, int end)
        {
            for (int i = start; i < end; i++)
                line.CanColor[i] = true;
        }

        private CrosswordLine ApplyAnalysisResultsToLine()
        {
            for (var i = 0; i < line.Length; i++)
            {
                if (!line.CanColor[i] && !line.CanEmpty[i]) //ячейка не может быть никакой
                    throw new IncorrectCrosswordException();

                if (line.CanColor[i] != line.CanEmpty[i]) //нашли какое-то точное состояние ячейки
                    line.Cells[i] = line.CanColor[i] ? CrosswordCell.Colored : CrosswordCell.Empty;
            }
            return line;
        }
    }
}
