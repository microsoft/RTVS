// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
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
                var str = "CREATE PROCEDURE";
                var index = content.ToUpperInvariant().IndexOf(str);
                if (index >= 0) {
                    int i = index + str.Length;
                    for (; i < content.Length; i++) {
                        if (!char.IsWhiteSpace(content[i])) {
                            break;
                        }
                    }
                    int start = i;
                    for (; i < content.Length; i++) {
                        if (char.IsWhiteSpace(content[i])) {
                            break;
                        }
                    }
                    if (i > start) {
                        return content.Substring(start, i - start);
                    }
                }
            }
            return string.Empty;
        }
    }
}