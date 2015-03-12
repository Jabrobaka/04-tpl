using System.Linq;

namespace JapaneseCrossword
{
    class LineAnalyzer
    {
        private readonly CrosswordLine line;

        private LineAnalyzer(CrosswordLine line)
        {
            this.line = line;
        }

        public static CrosswordLine UpdateLine(CrosswordLine line)
        {
            var analizer = new LineAnalyzer(line);
            var result = analizer.AnalyzeCurrentLine();
            return result;
        }

        private CrosswordLine AnalyzeCurrentLine()
        {
            if (AllBlocksDiscovered())
            {
                return MakeUnknownCellsEmpty();
            }

            //конец цикла - минимально необходимое количество ячеек для остальных блоков
            for (int i = -1; i <= line.Length - (line.Block.Skip(1).Sum() + line.Block.Count() - 1); i++)
            {
                if (PreviousCellHasColor(i))
                    break;
                
                //можно-ли поместить все блоки, начиная с первого, если первый блок будет в i
                if (CanPlaceBlocks(i, 0))
                {
                    PreviousCellCanEmpty(i);
                }
            }
            return ApplyAnalysisResultsToLine();
        }

        private bool AllBlocksDiscovered()
        {
            return line.Block.Sum() == line.Cells.Count(c => c == CrosswordCell.Colored);
        }

        private CrosswordLine MakeUnknownCellsEmpty()
        {
            line.Cells = line.Cells
                .Select(cell => cell != CrosswordCell.Unknown ? cell : CrosswordCell.Empty)
                .ToArray();
            return line;
        }

        private bool PreviousCellHasColor(int i)
        {
            return i > 0 && line[i - 1] == CrosswordCell.Colored;
        }

        private void PreviousCellCanEmpty(int i)
        {
            for (int j = 0; j < i; j++)
            {
                line.CanEmpty[j] = true;
            }
        }

        //TODO: еще отрефакторить
        private bool CanPlaceBlocks(int startIndex, int blockIndex)
        {
            var blockEndIndex = startIndex + line.Block.ElementAt(blockIndex);

            if (blockEndIndex > line.Length) //если блок уже не поместится в линию
                return false;

            for (int i = startIndex; i < blockEndIndex; i++) //можно ли разместить текущий блок
                if (line[i] == CrosswordCell.Empty) //если клетка точно пустая, блок не поместить
                    return false;

            if (NotLastBlock(blockIndex)) //рекурсия идет до последнего блока
            {
                return CheckRemainingCells(startIndex, blockIndex);
            }

            for (int i = blockEndIndex; i < line.Length; i++)
                if (line[i] == CrosswordCell.Colored) //после последнего блока не должно быть закрашенных ячеек
                    return false;
            ColorCells(startIndex, blockEndIndex); //ячейки последнего блока могут быть черными
            UncolorCells(blockEndIndex, line.Length); //оставшиеся могут быть пустыми
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
                if (PreviousCellHasColor(startIndex)) //если до начала блока есть черная клетка, то всё
                    break;

                if (CanPlaceBlocks(startNext, blockIndex + 1))
                {
                    result = true; //если следующие блоки можно расставить,
                    ColorCells(startIndex, blockEndIndex); //значит ячейки текущего могут быть черными
                    UncolorCells(blockEndIndex, startNext); //следовательно, ячейки до следующего блока могут быть пустыми
                }
            }
            return result; //если блок не последний, то больше нечего делать
        }

        private bool NotLastBlock(int blockIndex)
        {
            return blockIndex != line.Block.Count() - 1;
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
            var resultLine = BuildNewLine(line);
            for (var i = 0; i < line.Length; i++)
            {
                if (!line.CanColor[i] && !line.CanEmpty[i]) //ячейка не может быть никакой
                    throw new IncorrectCrosswordException();

                if (line.CanColor[i] != line.CanEmpty[i]) //нашли какое-то точное состояние ячейки
                    resultLine[i] = line.CanColor[i] ? CrosswordCell.Colored : CrosswordCell.Empty;

                if (line[i] != CrosswordCell.Unknown &&
                    resultLine[i] != CrosswordCell.Unknown &&
                    resultLine[i] != line[i])
                    throw new IncorrectCrosswordException(); //состояние ячеек отличается - что-то не так

                if (line[i] != CrosswordCell.Unknown) // если мы знаем цвет заранее -- так и оставим.
                    resultLine[i] = line[i];
            }
            return resultLine;
        }

        private static CrosswordLine BuildNewLine(CrosswordLine line)
        {
            var cells = new CrosswordCell[line.Length];
            var index = line.Index;
            var blocks = line.Block.ToList();
            return new CrosswordLine(cells, blocks, index);
        }
    }
}
