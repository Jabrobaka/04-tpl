using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace JapaneseCrossword
{
    class CrosswordParser
    {
        private readonly List<string> fileLines;
        private readonly int rowsSectionIndex;
        private readonly int columnsSectionIndex;

        private CrosswordParser(IEnumerable<string> inputLines)
        {
            fileLines = inputLines.ToList();
            rowsSectionIndex = fileLines.FindIndex(s => s.Contains("rows"));
            columnsSectionIndex = fileLines.FindIndex(s => s.Contains("columns"));
        }

        public static Crossword ParseCrossword(string inputFilePath)
        {
            if (!File.Exists(inputFilePath))
                throw new BadInputFileException();

            var fileLines = File.ReadAllLines(inputFilePath);
            var parser = new CrosswordParser(fileLines);
            var parseResult = parser.Parse();
            return parseResult;
        }

        private Crossword Parse()
        {
            var rowsCount = GetSectionLinesCount(rowsSectionIndex);
            var columnsCount = GetSectionLinesCount(columnsSectionIndex);
            var rowsInfo = new List<IEnumerable<int>>();
            var columnsInfo = new List<IEnumerable<int>>();

            for (int i = 0; i < rowsCount; i++)
            {
                rowsInfo.Add(LineBlocks.FromString(fileLines[rowsSectionIndex + 1 + i], columnsCount));
            }
            for (int i = 0; i < columnsCount; i++)
            {
                columnsInfo.Add(LineBlocks.FromString(fileLines[columnsSectionIndex + 1 + i], rowsCount));
            }
            
            return new Crossword(rowsInfo, columnsInfo);
        }

        private int GetSectionLinesCount(int sectionStartIndex)
        {
            var sectionLengthStr = Regex.Match(fileLines.ElementAt(sectionStartIndex), @"\w+:\s*(.+)").Groups[1].Value;
            var sectionLength = Int32.Parse(sectionLengthStr);
            return sectionLength;
        }
    }

    internal class BadInputFileException : Exception
    {
    }

    #region Tests
    [TestFixture]
    class CrosswordParser_should
    {
        [Test]
        [ExpectedException(typeof(BadInputFileException))]
        public void throw_exception_if_bad_file_path()
        {
            var data = CrosswordParser.ParseCrossword(Path.GetRandomFileName());
        }

        [Test]
        public void return_instance_on_correct_path()
        {
            var path = @"TestFiles\SampleInput.txt";
            var cw = CrosswordParser.ParseCrossword(path);
            Assert.That(cw, Is.InstanceOf(typeof(Crossword)));
        }

        [Test]
        public void return_correct_count_of_lines()
        {
            var path = @"TestFiles\Car.txt";
            var cw = CrosswordParser.ParseCrossword(path);
            Assert.That(cw.ColumnsInfo.Count() + cw.RowsInfo.Count(), Is.EqualTo(18));
        }

    }
    #endregion
}
