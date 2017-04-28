// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using FluentAssertions;

namespace Microsoft.Common.Core.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class BaselineCompare {
        public static string CompareStrings(string expected, string actual) {
            var result = new StringBuilder();

            int length = Math.Min(expected.Length, actual.Length);
            for (int i = 0; i < length; i++) {
                if (expected[i] != actual[i]) {
                    result.Append(FormattableString.Invariant($"Position: {i}: expected: '{expected[i]}', actual '{actual[i]}'\r\n"));
                    if (i > 6 && i < length - 6) {
                        result.Append(FormattableString.Invariant($"Context: {expected.Substring(i - 6, 12)} -> {actual.Substring(i - 6, 12)}"));
                    }
                    break;
                }

            }

            if (expected.Length != actual.Length) {
                result.Append(FormattableString.Invariant($"\r\nLength different. Expected: '{expected.Length}' , actual '{actual.Length}'"));
            }

            return result.ToString();
        }

        public static void CompareStringLines(string expected, string actual) {
            string baseLine, newLine;
            int index;
            int line = CompareLines(expected, actual, out baseLine, out newLine, out index);
            line.Should().Be(0, "there should be no difference at line {0}\r\nExpected:{1}\r\nActual:{2}\r\nDifference at position {3}\r\n", line, baseLine.Trim(), newLine.Trim(), index);
        }

        public static int CompareLines(string expected, string actual, out string expectedLine, out string actualLine, out int index, bool ignoreCase = false) {
            var actualReader = new StringReader(actual);
            var expectedReader = new StringReader(expected);

            int lineNum = 1;
            index = 0;

            for (lineNum = 1; ; lineNum++) {
                expectedLine = expectedReader.ReadLine();
                actualLine = actualReader.ReadLine();

                if (expectedLine == null || actualLine == null) {
                    break;
                }

                int minLength = Math.Min(expectedLine.Length, actualLine.Length);
                for (int i = 0; i < minLength; i++) {
                    char act = actualLine[i];
                    char exp = expectedLine[i];

                    if (ignoreCase) {
                        act = Char.ToLowerInvariant(act);
                        exp = Char.ToLowerInvariant(exp);
                    }

                    if (act != exp) {
                        index = i + 1;
                        return lineNum;
                    }
                }

                if (expectedLine.Length != actualLine.Length) {
                    index = minLength + 1;
                    return lineNum;
                }
            }

            if (expectedLine == null && actualLine == null) {
                expectedLine = string.Empty;
                actualLine = string.Empty;

                return 0;
            }

            return lineNum;
        }

        public static void CompareFiles(string baselineFile, string actual, bool regenerateBaseline, bool ignoreCase = false) {
            if (regenerateBaseline) {
                if (File.Exists(baselineFile)) {
                    File.SetAttributes(baselineFile, FileAttributes.Normal);
                }

                using (var sw = new StreamWriter(baselineFile)) {
                    sw.Write(actual);
                }
            } else {
                using (var sr = new StreamReader(baselineFile)) {
                    string expected = sr.ReadToEnd();

                    string baseLine, newLine;
                    int index;
                    int line = CompareLines(expected, actual, out baseLine, out newLine, out index, ignoreCase);
                    line.Should().Be(0, "there should be no difference at line {0}\r\nExpected:{1}\r\nActual:{2}\r\nBaselineFile:{3}\r\nDifference at {4}\r\n",
                        line, baseLine.Trim(), newLine.Trim(), baselineFile.Substring(baselineFile.LastIndexOf('\\') + 1), index);
                }
            }
        }
    }
}
