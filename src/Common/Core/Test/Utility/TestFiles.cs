// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;

namespace Microsoft.Common.Core.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class TestFiles {
        private static readonly char[] _whitespace = { ' ', '\r', '\n', '\t' };

        public static void CompareToBaseLine(string baselinefilePath, string actual) {
            var expected = File.ReadAllText(baselinefilePath);

            // Trim whitescpase in the end to avoid false positives b/c file 
            // has extra line break or whitespace at the end.
            expected = expected.TrimEnd(_whitespace);
            actual = actual.TrimEnd(_whitespace);

            var lineNumber = BaselineCompare.CompareLines(expected, actual, out var baseLine, out var actualLine, out var index);
            // Fluent does not like HTML
            if(lineNumber != 0) {
                actualLine.Should().Be(baseLine, $"Line number:{lineNumber}");
            }
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
