// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    internal sealed class SProcScriptGenerator {
        /// <summary>
        /// Default name of the column with the stored procedure names
        /// </summary>
        internal const string SProcColumnName = "SProcName";
        /// <summary>
        /// Name of the column with the procedure code
        /// </summary>
        internal const string RCodeColumnName = "RCode";

        private const string RCodeTemplate = "_RCODE_";
        private const string InputQueryTemplate = "_INPUT_QUERY_";

        private readonly IFileSystem _fs;

        public SProcScriptGenerator(IFileSystem fs) {
            _fs = fs;
        }

        public string CreateRCodeTableScript(SqlSProcPublishSettings settings) {
            var sb = new StringBuilder();
            sb.AppendLine(Invariant($"CREATE TABLE {settings.TableName.ToSqlName(settings.QuoteType)}"));
            sb.AppendLine("(");
            sb.AppendLine(Invariant($"{SProcColumnName} NVARCHAR(64),"));
            sb.AppendLine(Invariant($"{RCodeColumnName} NVARCHAR(max)"));
            sb.AppendLine(")");
            return sb.ToString();
        }

        /// <summary>
        /// Generates SQL post deployment script that pushes R code into a table
        /// as well as 
        /// </summary>
        public string CreatePostDeploymentScript(SqlSProcPublishSettings settings) {
            var sb = new StringBuilder();
            sb.AppendLine(Invariant($"INSERT INTO {settings.TableName.ToSqlName(settings.QuoteType)}"));

            for (int i = 0; i < settings.Files.Count; i++) {
                var filePath = settings.Files[i];

                var sprocName = settings.SProcNames[filePath];
                if (!string.IsNullOrEmpty(sprocName)) {
                    var content = GetRFileContent(filePath);
                    sb.Append(Invariant($"VALUES ('{sprocName.ToSqlName(settings.QuoteType)}', '{content}')"));
                    if (i < settings.Files.Count - 1) {
                        sb.Append(',');
                    }
                    sb.AppendLine(string.Empty);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Creates code for stored procedures
        /// </summary>
        public IDictionary<string, string> CreateStoredProcedureScripts(SqlSProcPublishSettings settings) {
            var dict = new Dictionary<string, string>();
            foreach (var rFilePath in settings.Files) {
                var sprocName = settings.SProcNames[rFilePath];
                if (!string.IsNullOrEmpty(sprocName)) {
                    string template;
                    if (settings.CodePlacement == RCodePlacement.Inline) {
                        template = FillSprocInlineTemplate(rFilePath, sprocName);
                    } else {
                        template = FillSprocTableTemplate(rFilePath, sprocName, settings.TableName, settings.QuoteType);
                    }
                    if (!string.IsNullOrEmpty(template)) {
                        dict[sprocName] = template;
                    }
                }
            }
            return dict;
        }

        /// <summary>
        /// Replaces procedure name, R Code and the SQL query placeholders with actual values
        /// </summary>
        private string FillSprocInlineTemplate(string rFilePath, string sprocName) {
            var sprocTemplateFile = rFilePath.ToSProcFilePath();
            var sprocTemplate = GetSqlFileContent(sprocTemplateFile);

            var rCode = GetRFileContent(rFilePath);
            rCode = rCode.EndsWithOrdinal(Environment.NewLine) ? rCode : rCode + Environment.NewLine;
            sprocTemplate = sprocTemplate.Replace(RCodeTemplate, Environment.NewLine + rCode);

            var sqlQuery = GetSqlFileContent(rFilePath.ToQueryFilePath()).Trim();
            return sprocTemplate.Replace(InputQueryTemplate, sqlQuery);
        }

        private string FillSprocTableTemplate(string rFilePath, string sprocName, string codeTableName, SqlQuoteType quoteType) {
            var sprocTemplateFile = rFilePath.ToSProcFilePath();
            var sprocTemplate = GetSqlFileContent(sprocTemplateFile);

            var format =
@"BEGIN
DECLARE @RCodeQuery NVARCHAR(max);
DECLARE @RCode NVARCHAR(max);
DECLARE @ParmDefinition NVARCHAR(max);

SET @RCodeQuery = N'SELECT @RCodeOUT = RCode FROM {0} WHERE SProcName = ''{1}''';
SET @ParmDefinition = N'@RCodeOUT NVARCHAR(max) OUTPUT';

EXEC sp_executesql @RCodeQuery, @ParmDefinition, @RCodeOUT=@RCode OUTPUT;
SELECT @RCode;
";
            var declarations = string.Format(CultureInfo.InvariantCulture, format, codeTableName.ToSqlName(quoteType), sprocName);
            sprocTemplate = sprocTemplate.Replace("BEGIN", declarations);
            sprocTemplate = sprocTemplate.Replace("N'_RCODE_'", "@RCode");

            var sqlQuery = GetSqlFileContent(rFilePath.ToQueryFilePath()).Trim();
            return sprocTemplate.Replace(InputQueryTemplate, sqlQuery);
        }

        private string GetRFileContent(string filePath) {
            return _fs.ReadAllText(filePath).Replace("'", "''");
        }

        private string GetSqlFileContent(string filePath) {
            if (_fs.FileExists(filePath)) {
                return _fs.ReadAllText(filePath);
            }
            return string.Empty;
        }
    }
}
