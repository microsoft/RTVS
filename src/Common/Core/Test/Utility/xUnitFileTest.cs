using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Xunit;

namespace Microsoft.Common.Core.Tests.Utility {
    public class xUnitFileTest : xUnitTest, IClassFixture<TestFilesSetupFixture> {
        protected TestFilesSetupFixture Fixture { get; }
        public xUnitFileTest(TestFilesSetupFixture fixture) {
            Fixture = fixture;
        }

        protected string LoadFile(string fileName) {
            var filePath = GetTestFilePath(fileName);

            using (var sr = new StreamReader(filePath)) {
                return sr.ReadToEnd();
            }
        }

        protected string GetTestFilesFolder() {
            string thisAssembly = Assembly.GetExecutingAssembly().Location;
            string assemblyLoc = Path.GetDirectoryName(thisAssembly);
            return Path.Combine(assemblyLoc, @"Files\");
        }

        protected string GetTestFilePath(string fileName) {
            return Path.Combine(GetTestFilesFolder(), fileName);
        }

        protected IList<string> GetTestFiles(string extension) {
            string path = GetTestFilesFolder();
            var files = new List<string>();

            IEnumerable<string> filesInFolder = Directory.EnumerateFiles(path);
            foreach (string name in filesInFolder) {
                if (name.EndsWithIgnoreCase(extension))
                    files.Add(name);
            }

            return files;
        }

        protected void CompareToBaseLine(string baselinefilePath, string actual) {
            string expected;

            using (var streamReader = new StreamReader(baselinefilePath)) {
                expected = streamReader.ReadToEnd();
            }

            // trim whitescpase in the end to avoid false positives b/c file 
            // has extra line break or whitespace at the end.
            expected = expected.TrimEnd(new char[] { ' ', '\r', '\n', '\t' });
            actual = actual.TrimEnd(new char[] { ' ', '\r', '\n', '\t' });

            string baseLine;
            string actualLine;
            int lineNumber = BaselineCompare.CompareLines(expected, actual, out baseLine, out actualLine);

            Assert.False(false,
                string.Format(CultureInfo.InvariantCulture,
                    "\r\nDifferent at line {0}\r\nExpected: {1}\r\nActual: {2}", lineNumber, baseLine, actualLine));
        }

        protected void UpdateBaseline(string filePath, string content) {
            if (File.Exists(filePath)) {
                File.SetAttributes(filePath, FileAttributes.Normal);
            }

            using (var streamWriter = new StreamWriter(filePath)) {
                streamWriter.Write(content);
            }
        }
    }
}
