// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Common.Core.IO;

namespace Microsoft.VisualStudio.R.Package.Sql {
    internal static class SProcFileExtensions {
        public const string QueryFileExtension = ".Query.sql";
        public const string SProcFileExtension = ".Template.sql";

        public static string ToQueryFilePath(this string rFilePath) {
            return !string.IsNullOrEmpty(rFilePath)
                       ? ChangeExtension(rFilePath, QueryFileExtension)
                       : rFilePath;
        }

        public static string ToSProcFilePath(this string rFilePath) {
            return !string.IsNullOrEmpty(rFilePath)
                    ? ChangeExtension(rFilePath, SProcFileExtension)
                    : rFilePath;
        }

        private static string ChangeExtension(string filePath, string extension) {
            var name = Path.GetFileNameWithoutExtension(filePath);
            return !string.IsNullOrEmpty(name)
                    ? Path.Combine(Path.GetDirectoryName(filePath), name + extension)
                    : string.Empty;
        }

        public static string GetSProcNameFromTemplate(this IFileSystem fs, string rFilePath) {
            var sprocTemplateFile = rFilePath.ToSProcFilePath();
            if (!string.IsNullOrEmpty(sprocTemplateFile) && fs.FileExists(sprocTemplateFile)) {
                var content = fs.ReadAllText(sprocTemplateFile);
                var regex = new Regex(@"(?i)\bCREATE\s+PROCEDURE\s+");
                var match = regex.Match(content);
                if (match.Length > 0) {
                    return GetProcedureName(content, match.Index + match.Length);
                }
            }
            return string.Empty;
        }

        private static string GetProcedureName(string s, int start) {
            // Check if name starts with [ or "
            if (start >= s.Length) {
                return string.Empty;
            }
            int i = start;
            var openQuote = s[start];
            if (openQuote == '[' || openQuote == '\"') {
                var closeQuote = openQuote == '[' ? ']' : '\"';
                // Skip over opening quoting character
                i++; 
                start++;
                for (; i < s.Length; i++) {
                    if (s[i] == '\n' || s[i] == '\r') {
                        break; // No variables with line breaks
                    }
                    if (s[i] == closeQuote) {
                        // handle "" and ]] escapes
                        if (i >= s.Length - 1 || s[i + 1] != closeQuote) {
                            break;
                        }
                        i++;
                    }
                }
            } else {
                // Unquoted
                for (; i < s.Length && !char.IsWhiteSpace(s[i]); i++) { }
            }
            return s.Substring(start, i - start);
        }
    }
}
