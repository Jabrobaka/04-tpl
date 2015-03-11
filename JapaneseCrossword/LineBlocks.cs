using System;
using System.Collections.Generic;
using System.Linq;

namespace JapaneseCrossword
{
    public static class LineBlocks
    {

        public static int[] FromString(string info, int lineLength)
        {
            var figures = info
                .Trim()
                .Split(' ')
                .Select(Int32.Parse)
                .ToArray();

            if (figures.Sum() > lineLength)
            {
                throw new IncorrectCrosswordException();
            }

            return figures.ToArray();
        }
    }
    internal class IncorrectCrosswordException : Exception
    {
    }
}