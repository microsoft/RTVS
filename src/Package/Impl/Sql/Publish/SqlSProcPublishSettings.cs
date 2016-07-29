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
        public SqlQuoteType QuoteType { get; set; } = SqlQuoteType.None;

        public SqlSProcPublishSettings(IEnumerable<string> files, IFileSystem fs) {
            _files.AddRange(files);
            _fs = fs;
            LoadSProcNames();
        }

        private void LoadSProcNames() {
            _sprocNameMap.Clear();
            foreach (var file in Files) {
                var sprocName = _fs.GetSProcNameFromTemplate(file);
                _sprocNameMap[file] = sprocName;
            }
        }
    }
}