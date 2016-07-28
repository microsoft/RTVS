// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Common.Core.IO;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    /// <summary>
    /// Represents persistent settings for the SQL stored procedure publishing dialog.
    /// </summary>
    internal class SqlSProcPublishSettings {
        public const string DefaultRCodeTableName = "RCodeTable";

        private readonly List<string> _files = new List<string>();
        private readonly Dictionary<string, string> _sprocNameMap = new Dictionary<string, string>();
        private readonly IFileSystem _fs;

        /// <summary>
        /// List of files
        /// </summary>
        public IReadOnlyList<string> Files => _files;

        /// <summary>
        /// List of stored procedure names
        /// </summary>
        public IReadOnlyDictionary<string, string> SProcNames => _sprocNameMap;

        /// <summary>
        /// Target SQL table name
        /// </summary>
        public string TableName { get; set; } = DefaultRCodeTableName;

        /// <summary>
        /// Target database project name
        /// </summary>
        public string TargetProject { get; set; }

        /// <summary>
        /// Determines where to place R code in SQL
        /// </summary>
        public RCodePlacement CodePlacement { get; set; } = RCodePlacement.Inline;

        /// <summary>
        /// Determines type of quoting for SQL names with spaces
        /// </summary>
        public SqlQuoteType QuoteType { get; set; } = SqlQuoteType.Bracket;

        public SqlSProcPublishSettings(IEnumerable<string> files, IFileSystem fs) {
            _files.AddRange(files);
            _fs = fs;
            LoadSProcNames();
        }

        private void LoadSProcNames() {
            _sprocNameMap.Clear();
            foreach (var file in Files) {
                var sprocName = GetSProcNameFromTemplate(file);
                _sprocNameMap[file] = sprocName;
            }
        }

        private string GetSProcNameFromTemplate(string rFilePath) {
            var sprocTemplateFile = rFilePath.ToSProcFilePath();
            if (!string.IsNullOrEmpty(sprocTemplateFile) && _fs.FileExists(sprocTemplateFile)) {
                var content = _fs.ReadAllText(sprocTemplateFile);
                var regex = new Regex(@"\bCREATE\s+PROCEDURE\s+");
                var match = regex.Match(content);
                 if (match.Index >= 0) {
                     return GetProcedureName(content, match.Index + match.Length);
                }
            }
            return string.Empty;
        }

        private string GetProcedureName(string s, int start) {
            // Check if name starts with [ or "
            if (start >= s.Length) {
                return string.Empty;
            }
            int i = start;
            var openQuote = s[start];
            if (openQuote == '[' || openQuote == '\"') {
                var closeQuote = openQuote == '[' ? ']' : '\"';
                i++; // Skip over opening quoting character
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