using System;
using System.Linq;

namespace JapaneseCrossword
{
    public class CrosswordSolver : ICrosswordSolver
    {
        protected volatile Crossword crossword;

        public SolutionStatus Solve(string inputFilePath, string outputFilePath)
        {
            try
            {
                InitSolver(inputFilePath);
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

        private void InitSolver(string inputFilePath)
        {
            crossword = CrosswordParser.ParseCrossword(inputFilePath);
        }

        protected virtual void AnalyzeToEnd()
        {
            var typeTuple = GetCrosswordLineTypes();
            while (crossword.HasLinesToUpdate())
            {
                crossword
                    .GetLinesToUpdate(typeTuple.Item1)
                    .ToList()
                    .ForEach(line => AnalyzeLine(line, typeTuple.Item1));

                crossword
                    .GetLinesToUpdate(typeTuple.Item2)
                    .ToList()
                    .ForEach(line => AnalyzeLine(line, typeTuple.Item2));    
            }
        }

        protected void AnalyzeLine(CrosswordLine line, LineType type)
        {
            var analyzer = new LineAnalyzer(line);
            var analyzed = analyzer.AnalyzeLine(); 
            UpdateCrossword(type, analyzed);
        }

        protected virtual void UpdateCrossword(LineType type, CrosswordLine analyzed)
        {
            crossword.SetLine(analyzed, type);
        }

        /// <summary>
        /// Возвращает Tuple, в котором первый объект содержит тип самой короткой линии
        /// </summary>
        protected Tuple<LineType, LineType> GetCrosswordLineTypes()
        {
            var rowLength = crossword.Cells.GetLength(0);
            var columnLength = crossword.Cells.GetLength(1);
            if (rowLength < columnLength)
                return Tuple.Create(LineType.Row, LineType.Column);
            return Tuple.Create(LineType.Column, LineType.Row);
        }

        public override string ToString()
        {
            return "Single thread solver"; //для сравнения солверов
        }
    }
}