using System;
using System.Collections.Generic;
using System.Linq;

namespace JapaneseCrossword
{
    public class CrosswordSolver : ICrosswordSolver
    {
        protected Crossword crossword;
        protected bool[] columnsToUpdate;
        protected bool[] rowsToUpdate;

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

            rowsToUpdate = InitFlagsArray(crossword.Cells.GetLength(0));
            columnsToUpdate = InitFlagsArray(crossword.Cells.GetLength(1));
        }

        private static bool[] InitFlagsArray(int count)
        {
            return Enumerable
                .Range(0, count)
                .Select(i => true)
                .ToArray();
        }

        protected virtual void AnalyzeToEnd()
        {
            var rowHasLessLength = rowsToUpdate.Length < columnsToUpdate.Length;
            while (HasLinesToUpdate())
            {
                GetLinesToUpdate(rowHasLessLength)
                    .ToList()
                    .ForEach(line => AnalyzeLine(line, rowHasLessLength));

                GetLinesToUpdate(!rowHasLessLength)
                    .ToList()
                    .ForEach(line => AnalyzeLine(line, !rowHasLessLength));    
            }
        }

        protected bool HasLinesToUpdate()
        {
            return rowsToUpdate.Any(l => l) || columnsToUpdate.Any(l => l);
        }

        protected IEnumerable<CrosswordLine> GetLinesToUpdate(bool isRows)
        {
            var toUpdate = isRows ? rowsToUpdate : columnsToUpdate;
            return GetLines(isRows)
                .Where((line, i) => toUpdate[i]);
        } 

        protected IEnumerable<CrosswordLine> GetLines(bool isRows)
        {
            var method = GetLineMethod(isRows);
            var count = isRows ? rowsToUpdate.Length : columnsToUpdate.Length;
            for (int i = 0; i < count; i++)
            {
                yield return method(i);
            }
        }

        private Func<int, CrosswordLine> GetLineMethod(bool isRows)
        {
            if (isRows)
                return crossword.GetRowByIndex;
            return crossword.GetColumnByIndex;
        }       

        protected void AnalyzeLine(CrosswordLine line, bool isRow)
        {
            var analyzer = new LineAnalyzer(line);
            var analyzedLine = analyzer.AnalyzeLine();
            UpdateLine(analyzedLine, isRow);

            UpdateCrosswordState(isRow,  analyzedLine);
        }

        private void UpdateCrosswordState(bool isRow,  CrosswordLine analyzedLine)
        {
            var toUpdate = isRow ? columnsToUpdate : rowsToUpdate;
            for (int i = 0; i < analyzedLine.Length; i++)
            {
                var row = isRow ? analyzedLine.Index : i;
                var col = isRow ? i : analyzedLine.Index;
                if (crossword[row, col] != analyzedLine[i])
                {
                    toUpdate[i] = true;
                    crossword[row, col] = analyzedLine[i];
                }
            }
        }

        private void UpdateLine(CrosswordLine newLine, bool isRow)
        {
            (isRow ? rowsToUpdate : columnsToUpdate)[newLine.Index] = false; //пометить текущую линию
        }


        public override string ToString()
        {
            return "Single thread solver"; //для сравнения солверов
        }
    }
}