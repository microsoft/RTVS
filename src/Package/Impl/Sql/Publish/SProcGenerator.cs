// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    internal sealed class SProcGenerator {
        /// <summary>
        /// Name of script file that contains SQL that creates R code table
        /// </summary>
        internal const string CreateRCodeTableScriptName = "CreateRCodeTable.sql";
        /// <summary>
        /// Name of the post-deployment script that inserts actual R code into the table
        /// </summary>
        internal const string PostDeploymentScriptName = "RCode.PostDeployment.sql";
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

        private readonly ICoreShell _coreShell;
        private readonly IProjectSystemServices _pss;
        private readonly IFileSystem _fs;

        public SProcGenerator(ICoreShell coreShell, IProjectSystemServices pss, IFileSystem fs) {
            _coreShell = coreShell;
            _pss = pss;
            _fs = fs;
        }

        /// <summary>
        /// Generates SQL scripts for the deployment of R code into SQL database.
        /// </summary>
        public void Generate(SqlSProcPublishSettings settings, EnvDTE.Project targetProject) {
            var targetFolder = Path.Combine(Path.GetDirectoryName(targetProject.FullName), "R\\");
            if (!_fs.DirectoryExists(targetFolder)) {
                _fs.CreateDirectory(targetFolder);
            }

            var targetProjectItem = targetProject.ProjectItems.Item("R") ?? targetProject.ProjectItems.AddFolder("R");

            if (settings.CodePlacement == RCodePlacement.Table) {
                CreateRCodeTable(settings, targetProject, targetFolder, targetProjectItem);
                CreatePostDeploymentScript(settings, targetProject, targetFolder, targetProjectItem);
            }
            CreateStoredProcedures(settings, targetProject, targetFolder, targetProjectItem);
        }

        /// <summary>
        /// Create SQL file that defines table template that will hold R code
        /// </summary>
        private void CreateRCodeTable(SqlSProcPublishSettings settings, EnvDTE.Project targetProject, string targetFolder, EnvDTE.ProjectItem targetProjectItem) {
            var creatTableScriptFile = Path.Combine(targetFolder, CreateRCodeTableScriptName);
            using (var sw = new StreamWriter(creatTableScriptFile)) {
                sw.WriteLine(Invariant($"CREATE TABLE {settings.TableName.ToSqlName(settings.QuoteType)}"));
                sw.WriteLine("(");
                sw.WriteLine(Invariant($"{SProcColumnName} NVARCHAR(64),"));
                sw.WriteLine(Invariant($"{RCodeColumnName} NVARCHAR(max)"));
                sw.WriteLine(")");
            }
            targetProjectItem.ProjectItems.AddFromFile(creatTableScriptFile);
        }

        /// <summary>
        /// Generates SQL post deployment script that pushes R code into a table
        /// as well as 
        /// </summary>
        private void CreatePostDeploymentScript(SqlSProcPublishSettings settings, EnvDTE.Project targetProject, string targetFolder, EnvDTE.ProjectItem targetProjectItem) {
            var postDeploymentScript = Path.Combine(targetFolder, PostDeploymentScriptName);

            using (var sw = new StreamWriter(postDeploymentScript)) {
                sw.WriteLine(Invariant($"INSERT INTO {settings.TableName.ToSqlName(settings.QuoteType)}"));

                for (int i = 0; i < settings.Files.Count; i++) {
                    var filePath = settings.Files[i];

                    var sprocName = settings.SProcNames[filePath];
                    if (!string.IsNullOrEmpty(sprocName)) {
                        var content = GetRFileContent(filePath);
                        sw.Write(Invariant($"VALUES ('{sprocName.ToSqlName(settings.QuoteType)}', '{content}')"));
                        if (i < settings.Files.Count - 1) {
                            sw.Write(',');
                        }
                        sw.WriteLine(string.Empty);
                    }
                }
            }
            var item = targetProjectItem.ProjectItems.AddFromFile(postDeploymentScript);
            item.Properties.Item("BuildAction").Value = "PostDeploy";
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

        private void CreateStoredProcedures(SqlSProcPublishSettings settings, EnvDTE.Project targetProject, string targetFolder, EnvDTE.ProjectItem targetProjectItem) {
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
                        var sprocFile = Path.ChangeExtension(Path.Combine(targetFolder, sprocName), ".sql");
                        _fs.WriteAllText(sprocFile, template);
                        targetProjectItem.ProjectItems.AddFromFile(sprocFile);
                    }
                }
            }
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
