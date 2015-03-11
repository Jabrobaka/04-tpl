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
            //первый блок может начинаться не с 1 ячейки
            //конец цикла - минимально необходимое количество ячеек для остальных блоков
            for (int i = 0; i < line.Length - (line.Block.Sum() - line.Block.FirstOrDefault() + line.Block.Count() - 1); i++)
            {
                //если до начала проверки уже есть черные - нет смысла проверять
                if (i > 0 && line[i - 1] == CrosswordCell.Colored)
                    break;
                
                //можно-ли поместить все блоки, начиная с первого, если первый блок будет в i
                if (CanPlaceBlocks(i, 0))
                {
                    //все ячейки до первого могут быть пустыми
                    for (int j = 0; j < i; j++)
                    {
                        line.CanEmpty[j] = true;
                    }
                }
            }
            return ApplyAnalysisResultsToLine();
        }

        private static CrosswordLine BuildNewLine(CrosswordLine line)
        {
            var cells = new CrosswordCell[line.Length];
            var index = line.Index;
            var blocks = line.Block.ToList();
            return new CrosswordLine(cells, blocks, index);
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


        //это ужас, но оригинал на паскале прибыл из пыточной камеры для программистов
        //TODO: нужен серьезный рефакторинг
        private bool CanPlaceBlocks(int startIndex, int blockIndex) 
        {
            var blockEndIndex = startIndex + line.Block.ElementAt(blockIndex);
            if (blockEndIndex > line.Length) //если блок уже не поместится в линию
                return false;
            for (int i = startIndex; i < blockEndIndex; i++) //можно ли разместить текущий блок
                if (line[i] == CrosswordCell.Empty) //если клетка точно пустая, блок не поместить
                    return false;

            if (blockIndex != line.Block.Count() - 1) //рекурсия идет до последнего блока
            {
                var result = false;
                for (int startNext = blockEndIndex + 1;
                    startNext <= line.Length - line.Block.ElementAt(blockIndex+1);
                    startNext++)
                {
                    if (startIndex> 0 && line[startIndex - 1] == CrosswordCell.Colored) //если до начала блока есть черная клетка, то всё
                        break;

                    if (CanPlaceBlocks(startNext, blockIndex + 1))
                    {
                        result = true;                          
                                                                //если следующие блоки можно расставить,
                        ColorCells(startIndex, blockEndIndex);  //значит ячейки текущего могут быть черными

                        UncolorCells(blockEndIndex, startNext); //следовательно, ячейки до следующего блока могут быть пустыми
                    }
                }
                return result; //если блок не последний, то больше нечего делать
            }

            for (int i = blockEndIndex; i < line.Length; i++)
                if (line[i] == CrosswordCell.Colored) //после последнего блока не должно быть закрашенных ячеек
                    return false;

            ColorCells(startIndex, blockEndIndex); //ячейки последнего блока могут быть черными
            UncolorCells(blockEndIndex, line.Length); //оставшиеся могут быть пустыми
            return true;
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

    }
}
