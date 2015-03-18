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
                var i = 0;
                var rowHasLessLength = rowsToUpdate.Length < columnsToUpdate.Length;
                while (HasLinesToUpdate())
                {
                    var tasks = GetLinesToUpdate(rowHasLessLength)
                        .Select(line => Task.Run(() =>
                        {
                            AnalyzeLine(line, rowHasLessLength);
                            Console.WriteLine(i++);
                        }));

                    Task.WaitAll(tasks.ToArray());

                    tasks = GetLinesToUpdate(!rowHasLessLength)
                        .Select(line => Task.Run(() => AnalyzeLine(line, !rowHasLessLength))); 

                    Task.WaitAll(tasks.ToArray());
                }
            }
            catch (AggregateException e)
            {
                throw new IncorrectCrosswordException();
            }
        }

        public override string ToString()
        {
            return "TPL Solver";
        }
    }
}
