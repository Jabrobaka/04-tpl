using System;
using System.Linq;

namespace JapaneseCrossword
{
    public class CrosswordSolver : ICrosswordSolver
    {
        protected Crossword crossword;

        public SolutionStatus Solve(string inputFilePath, string outputFilePath)
        {
            try
            {
                crossword = CrosswordParser.ParseCrossword(inputFilePath);
                AnalyzeToEnd();
                var resultWriter = new CrosswordWriter(crossword);
                resultWriter.TryWrite(outputFilePath);
            }
            catch (BadInputFileException)
            {
                return SolutionStatus.BadInputFilePath;
            }
            catch (IncorrectCrosswordException)
            {
                return SolutionStatus.IncorrectCrossword;
            }
            catch (BadOutputFileException)
            {
                return SolutionStatus.BadOutputFilePath;
            }

            //можно ведь как-то лучше это сделать. Печально, что двумерный массив нельзя линкой пройти
            foreach (var cell in crossword.Cells)
            {
                if (cell == CrosswordCell.Unknown)
                {
                    return SolutionStatus.PartiallySolved;
                }
            }

            return SolutionStatus.Solved;
        }

        protected virtual void AnalyzeToEnd()
        {
            while (crossword.HasLinesToUpldate())
            {
                var lines = crossword
                    .GetRowsToRefresh()
                    .ToList();
                lines.ForEach(line => AnalyzeLine(line, crossword.SetRow));

                lines = crossword
                    .GetColumnsToRefresh()
                    .ToList();
                lines.ForEach(line => AnalyzeLine(line, crossword.SetColumn));
            }
        }

        protected void AnalyzeLine(CrosswordLine line, Action<CrosswordLine> lineSetter)
        {
            var analyzer = new LineAnalyzer(line);
            var analyzedLine = analyzer.AnalyzeLine();
            lineSetter(analyzedLine);
        }

        public override string ToString()
        {
            return "Single thread solver"; //для сравнения солверов
        }
    }
}