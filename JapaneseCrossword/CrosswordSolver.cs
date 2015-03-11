namespace JapaneseCrossword
{
    public class CrosswordSolver : ICrosswordSolver
    {
        private Crossword crossword;

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

            return SolutionStatus.Solved;
        }

        private void AnalyzeToEnd()
        {
            var hasResult = false;
            while (crossword.HasLinesToUpldate())
            {
                CrosswordLine line;
                while ((line = crossword.GetRowToRefresh()) != null)
                {
                    var analyzedLine = LineAnalyzer.UpdateLine(line);
                    crossword.SetRow(analyzedLine);
                    line = null;
                }
                while ((line = crossword.GetColumnToRefresh()) != null)
                {
                    var analyzedLine = LineAnalyzer.UpdateLine(line);
                    crossword.SetColumn(analyzedLine);
                    line = null;
                }
            }
        }
    }
}