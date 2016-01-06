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
            int index;
            var lineNumber = BaselineCompare.CompareLines(expected, actual, out baseLine, out actualLine, out index);

            lineNumber.Should().Be(0, "there should be no difference at line {0}.\r\nExpected:{1}\r\nActual:{2}\r\nDifference at position {3}\r\n", lineNumber, baseLine, actualLine, index);
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
