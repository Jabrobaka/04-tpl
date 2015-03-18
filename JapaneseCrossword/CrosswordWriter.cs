using System;
using System.IO;

namespace JapaneseCrossword
{
    class CrosswordWriter
    {
        private readonly Crossword crossword;

        public CrosswordWriter(Crossword crossword)
        {
            this.crossword = crossword;
        }

        public void TryWrite(string outputFilePath)
        {
            try
            {
                Write(outputFilePath);
            }
            catch (ArgumentException)
            {
                throw new BadOutputFileException();
            }
            
        }

        private void Write(string outputFilePath)
        {
            using (var file = new StreamWriter(outputFilePath))
            {
                for (int i = 0; i < crossword.Cells.GetLength(0); i++)
                {
                    for (int j = 0; j < crossword.Cells.GetLength(1); j++)
                    {
                        var ch = GetCharRepresentation(crossword.Cells[i, j]);
                        file.Write(ch);
                    }
                    file.Write("\r\n");
                }
            }
        }

        private char GetCharRepresentation(CrosswordCell cell)
        {
            var ch = '?';
            switch (cell)
            {
                case CrosswordCell.Colored:
                    ch = '*';
                    break;
                case CrosswordCell.Empty:
                    ch = '.';
                    break;
                case CrosswordCell.Unknown:
                    ch = '?';
                    break;
            }
            return ch;
        }
    }

    internal class BadOutputFileException : Exception
    {
    }

}
