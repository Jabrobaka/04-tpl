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
            //конец цикла - минимально необходимое количество ячеек для остальных блоков
            for (int i = 0; i <= line.Length - (line.Block.Sum() + line.Block.Count() - 1); i++)
            {
                if (PreviousCellHasColor(i))
                    break;
                
                if (CanPlaceBlocks(i, 0))
                {
                    PreviousCellCanBeEmpty(i);
                }
            }
            return ApplyAnalysisResultsToLine();
        }

        private bool PreviousCellHasColor(int i)
        {
            return i > 0 && line[i - 1] == CrosswordCell.Colored;
        }

        private void PreviousCellCanBeEmpty(int i)
        {
            for (int j = 0; j < i; j++)
            {
                line.CanEmpty[j] = true;
            }
        }


        /// <summary>
        /// можно-ли поместить все блоки, начиная с блока с номером <paramref name="blockIndex"/>,
        /// если первый блок будет в  ячейке <paramref name="startIndex"/>
        /// </summary>
        private bool CanPlaceBlocks(int startIndex, int blockIndex)
        {
            var blockEndIndex = startIndex + line.Block.ElementAt(blockIndex);

            if (blockEndIndex > line.Length) //если блок уже не поместится в линию
                return false;

            if (!CanPlaceCurrentBlock(startIndex, blockEndIndex)) return false;

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
                if (line[i] == CrosswordCell.Colored)
                    return true;
            return false;
        }

        private bool CanPlaceCurrentBlock(int startIndex, int blockEndIndex)
        {
            for (int i = startIndex; i < blockEndIndex; i++)
                if (line[i] == CrosswordCell.Empty)
                    return false;
            return true;
        }

        private bool CheckRemainingCells(int startIndex, int blockIndex)
        {
            var result = false;
            var blockEndIndex = startIndex + line.Block.ElementAt(blockIndex);
            for (int startNext = blockEndIndex + 1;
                startNext <= line.Length - line.Block.ElementAt(blockIndex + 1) + 1;
                startNext++)
            {
                if (PreviousCellHasColor(startNext)) 
                    break;

                if (CanPlaceBlocks(startNext, blockIndex + 1))
                {
                    result = true; 
                    ColorCells(startIndex, blockEndIndex);
                    UncolorCells(blockEndIndex, startNext);
                }
            }
            return result;
        }

        private bool IsLastBlock(int blockIndex)
        {
            return blockIndex == line.Block.Count() - 1;
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
                    line[i] = line.CanColor[i] ? CrosswordCell.Colored : CrosswordCell.Empty;
            }
            return line;
        }
    }
}
