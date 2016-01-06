using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;

namespace Microsoft.Common.Core.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class TestFiles {
        public static void CompareToBaseLine(string baselinefilePath, string actual) {
            string expected;

            using (var streamReader = new StreamReader(baselinefilePath)) {
                expected = streamReader.ReadToEnd();
            }

            // trim whitescpase in the end to avoid false positives b/c file 
            // has extra line break or whitespace at the end.
            expected = expected.TrimEnd(' ', '\r', '\n', '\t');
            actual = actual.TrimEnd(' ', '\r', '\n', '\t');

            string baseLine;
            string actualLine;
            var lineNumber = BaselineCompare.CompareLines(expected, actual, out baseLine, out actualLine);

            lineNumber.Should().Be(0, "There should be no difference at line {0}\r\n\tExpected:\t{1}\r\n\tActual:\t{2}\r\n", lineNumber, baseLine, actualLine);
        }

        public static void UpdateBaseline(string filePath, string content) {
            if (File.Exists(filePath)) {
                File.SetAttributes(filePath, FileAttributes.Normal);
            }

            using (var streamWriter = new StreamWriter(filePath)) {
                streamWriter.Write(content);
            }
        }
    }
}
