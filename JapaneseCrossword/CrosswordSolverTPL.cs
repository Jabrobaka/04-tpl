using System;
using System.Linq;
using System.Threading.Tasks;

namespace JapaneseCrossword
{
    class CrosswordSolverTPL : CrosswordSolver
    {
        protected override void AnalyzeToEnd()
        {
            try
            {
                while (crossword.HasLinesToUpldate())
                {
                    var rows = crossword
                        .GetRowsToRefresh()
                        .Select(line => Task.Run(() => AnalyzeLine(line, SetLine(true))));

                    Task.WaitAll(rows.ToArray());

                    var columns = crossword
                        .GetColumnsToRefresh()
                        .Select(line => Task.Run(() => AnalyzeLine(line, SetLine(false))));
                    Task.WaitAll(columns.ToArray());
                }
            }
            catch (AggregateException e)
            {
                throw new IncorrectCrosswordException();
            }
        }

        private Action<CrosswordLine> SetLine(bool isRow)
        {
            //инлайн условие жалуется на метод групп
            Action<CrosswordLine> setAction;
            if (isRow)
                setAction = crossword.SetRow;
            else setAction =  crossword.SetColumn;
            return line =>
            {
                lock (crossword)
                {
                    setAction(line);
                }
            };
        }

        public override string ToString()
        {
            return "TPL Solver";
        }
    }
}
