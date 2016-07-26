// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using static System.FormattableString;
using Microsoft.Common.Core.Shell;
using System.Globalization;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif
#if VS15
using Microsoft.VisualStudio.ProjectSystem;
#endif

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

        private const string SProcNameTemplate = "_PROCEDURENAME_";
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
        /// <param name="settings">Settings for the R to SQL deployment</param>
        /// <param name="rFilesFolder">Folder with R files</param>
        /// <param name="targetProject">Target database project</param>
        public void Generate(SqlSProcPublishSettings settings, IEnumerable<string> selectedFiles, string sourceProjectFolder, EnvDTE.Project targetProject) {
            var targetFolder = Path.Combine(Path.GetDirectoryName(targetProject.FullName), "R\\");
            if (!_fs.DirectoryExists(targetFolder)) {
                _fs.CreateDirectory(targetFolder);
            }

            if (settings.CodePlacement == RCodePlacement.Table) {
                CreateRCodeTable(settings, targetProject, targetFolder, settings.TableName);
                CreatePostDeploymentScript(settings, sourceProjectFolder, targetProject, targetFolder, settings.TableName);
            }
            if (settings.GenerateStoredProcedures) {
                CreateStoredProcedures(settings, sourceProjectFolder, targetProject, targetFolder, settings.TableName);
            }
        }

        /// <summary>
        /// Create SQL file that defines table template that will hold R code
        /// </summary>
        private void CreateRCodeTable(SqlSProcPublishSettings settings, EnvDTE.Project targetProject, string targetFolder, string codeTableName) {
            var creatTableScriptFile = Path.Combine(targetFolder, CreateRCodeTableScriptName);
            using (var sw = new StreamWriter(creatTableScriptFile)) {
                sw.WriteLine(Invariant($"CREATE TABLE {codeTableName}"));
                sw.WriteLine("(");
                sw.WriteLine(Invariant($"[{SProcColumnName}] NVARCHAR(64),"));
                sw.WriteLine(Invariant($"[{RCodeColumnName}] NVARCHAR(max)"));
                sw.WriteLine(")");
            }
            targetProject.ProjectItems.AddFromFile(creatTableScriptFile);
        }

        /// <summary>
        /// Generates SQL post deployment script that pushes R code into a table
        /// as well as 
        /// </summary>
        private void CreatePostDeploymentScript(SqlSProcPublishSettings settings, string sourceProjectFolder,
                            EnvDTE.Project targetProject, string targetFolder, string codeTableName) {
            var targetProjectFolder = Path.GetDirectoryName(targetProject.FullName);
            var populateTableScriptFile = Path.Combine(targetProjectFolder, PostDeploymentScriptName);
            using (var sw = new StreamWriter(populateTableScriptFile)) {
                sw.WriteLine(Invariant($"INSERT INTO {codeTableName}"));

                for (int i = 0; i < settings.SProcInfoEntries.Count; i++) {
                    var info = settings.SProcInfoEntries[i];
                    var content = GetRFileContent(sourceProjectFolder, info.FilePath);
                    sw.Write(Invariant($"VALUES ('{info.SProcName}', '{content}')"));

                    if (i < settings.SProcInfoEntries.Count - 1) {
                        sw.Write(',');
                    }
                    sw.WriteLine(string.Empty);
                }
            }
            var item = targetProject.ProjectItems.AddFromFile(populateTableScriptFile);
            item.Properties.Item("BuildAction").Value = "PostDeploy";
        }

        /// <summary>
        /// Replaces procedure name, R Code and the SQL query placeholders with actual values
        /// </summary>
        private string FillSprocTemplate(string sourceProjectFolder, SProcInfo info, RCodePlacement codePlacement, string codeTableName) {
            var sprocTemplateFile = info.FilePath + ".SProc.sql";
            var sprocTemplate = GetSqlFileContent(sourceProjectFolder, sprocTemplateFile);

            if (sprocTemplate.IndexOf(SProcNameTemplate) < 0 || sprocTemplate.IndexOf(RCodeTemplate) < 0 || sprocTemplate.IndexOf(InputQueryTemplate) < 0) {
                _coreShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.SqlPublishDialog_TemplateDamaged, sprocTemplateFile));
                return string.Empty;
            }

            sprocTemplate = sprocTemplate.Replace(SProcNameTemplate, info.SProcName);

            string scriptCode;
            if (codePlacement == RCodePlacement.Table) {
                scriptCode = Invariant($"SELECT RCode FROM {codeTableName} WHERE {SProcColumnName} IS {info.SProcName}");
            } else {
                var rCode = GetRFileContent(sourceProjectFolder, info.FilePath);
                rCode = rCode.EndsWithOrdinal(Environment.NewLine) ? rCode : rCode + Environment.NewLine;
                scriptCode = Environment.NewLine + rCode;
            }
            sprocTemplate = sprocTemplate.Replace(RCodeTemplate, scriptCode);

            var sqlQuery = GetSqlFileContent(sourceProjectFolder, info.FilePath + ".sql").Trim();
            return sprocTemplate.Replace(InputQueryTemplate, sqlQuery);
        }

        private void CreateStoredProcedures(SqlSProcPublishSettings settings, string sourceProjectFolder,
                                            EnvDTE.Project targetProject, string targetFolder, string codeTableName) {
            foreach (var info in settings.SProcInfoEntries) {
                var template = FillSprocTemplate(sourceProjectFolder, info, settings.CodePlacement, codeTableName);
                if (!string.IsNullOrEmpty(template)) {
                    var sprocFile = Path.ChangeExtension(Path.Combine(targetFolder, info.SProcName), ".sql");
                    _fs.WriteAllText(sprocFile, template);
                    targetProject.ProjectItems.AddFromFile(sprocFile);
                }
            }
        }

        private string GetRFileContent(string sourceFolder, string relativePath) {
            var filePath = PathHelper.MakeRooted(PathHelper.EnsureTrailingSlash(sourceFolder), relativePath);
            if (_fs.FileExists(filePath)) {
                return _fs.ReadAllText(filePath).Replace("'", "''");
            }
            return string.Empty;
        }

        private string GetSqlFileContent(string sourceFolder, string relativePath) {
            var filePath = PathHelper.MakeRooted(PathHelper.EnsureTrailingSlash(sourceFolder), relativePath);
            if (_fs.FileExists(filePath)) {
                var sb = new StringBuilder();
                foreach (var line in _fs.FileReadAllLines(filePath)) {
                    if (!line.TrimStart().StartsWithOrdinal("--")) {
                        sb.AppendLine(line);
                    }
                }
                return sb.ToString();
            }
            return string.Empty;
        }
    }
}
