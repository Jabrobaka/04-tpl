using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace JapaneseCrossword
{
    class Program
    {
        static void Main(string[] args)
        {
            var singleThreadSolver = new CrosswordSolver();
            var TPLSolver = new CrosswordSolverTPL();

            var crosswords = Directory
                .EnumerateFiles(@"TestFiles\")
                .Where(fname => !fname.Contains("solved"));

            var watch = new Stopwatch();

            foreach (var crossword in crosswords)
            {
                watch.Restart();
                singleThreadSolver.Solve(crossword, Path.GetRandomFileName());
                Report(crossword, watch, singleThreadSolver);
                watch.Restart();
                TPLSolver.Solve(crossword, Path.GetRandomFileName());
                Report(crossword, watch, TPLSolver);
            }
            Console.WriteLine("Test done!");
        }

        private static void Report(string crossword, Stopwatch watch, CrosswordSolver solver)
        {
            Console.WriteLine("{0} : {1} millis by {2}", crossword, watch.ElapsedMilliseconds, solver.ToString());
        }
    }
}
