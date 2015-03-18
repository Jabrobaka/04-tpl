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
                var typeTuple = GetCrosswordLineTypes();
                while (crossword.HasLinesToUpdate())
                {
                    var tasks = crossword
                        .GetLinesToUpdate(typeTuple.Item1)
                        .Select(line => Task.Run(() => AnalyzeLine(line, typeTuple.Item1)))
                        .ToArray();


                    Task.WaitAll(tasks);

                    tasks = crossword
                        .GetLinesToUpdate(typeTuple.Item2)
                        .Select(line => Task.Run(() => AnalyzeLine(line, typeTuple.Item2)))
                        .ToArray();

                    Task.WaitAll(tasks);
                }
            }
            catch (AggregateException e)
            {
                throw new IncorrectCrosswordException();
            }
        }

        protected override void UpdateCrossword(LineType type, CrosswordLine analyzed)
        {
            lock (crossword)
            {
                crossword.SetLine(analyzed, type);
            }
        }

        public override string ToString()
        {
            return "TPL Solver";
        }
    }
}
