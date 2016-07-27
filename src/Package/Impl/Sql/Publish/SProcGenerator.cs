// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        public void Generate(SqlSProcPublishSettings settings, IEnumerable<string> selectedFiles, EnvDTE.Project targetProject) {
            var targetFolder = Path.Combine(Path.GetDirectoryName(targetProject.FullName), "R\\");
            if (!_fs.DirectoryExists(targetFolder)) {
                _fs.CreateDirectory(targetFolder);
            }
            if (settings.CodePlacement == RCodePlacement.Table) {
                CreateRCodeTable(settings, targetProject, targetFolder, settings.TableName);
                CreatePostDeploymentScript(settings, targetProject, targetFolder);
            }
            CreateStoredProcedures(settings, targetProject, targetFolder);
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
        private void CreatePostDeploymentScript(SqlSProcPublishSettings settings, EnvDTE.Project targetProject, string targetFolder) {
            var targetProjectFolder = Path.GetDirectoryName(targetProject.FullName);
            var postDeploymentScript = Path.Combine(targetProjectFolder, PostDeploymentScriptName);

            using (var sw = new StreamWriter(postDeploymentScript)) {
                sw.WriteLine(Invariant($"INSERT INTO {settings.TableName}"));

                for (int i = 0; i < settings.Files.Count; i++) {
                    var filePath = settings.Files[i];

                    var sprocName = settings.SProcNames[filePath];
                    if (!string.IsNullOrEmpty(sprocName)) {
                        var content = GetRFileContent(filePath);
                        sw.Write(Invariant($"VALUES ('{sprocName}', '{content}')"));
                        if (i < settings.Files.Count - 1) {
                            sw.Write(',');
                        }
                        sw.WriteLine(string.Empty);
                    }
                }
            }
            var item = targetProject.ProjectItems.AddFromFile(postDeploymentScript);
            item.Properties.Item("BuildAction").Value = "PostDeploy";
        }

        /// <summary>
        /// Replaces procedure name, R Code and the SQL query placeholders with actual values
        /// </summary>
        private string FillSprocTemplate(string filePath, string sprocName, RCodePlacement codePlacement, string codeTableName) {
            var sprocTemplateFile = filePath + ".SProc.sql";
            var sprocTemplate = GetSqlFileContent(sprocTemplateFile);

            string scriptCode;
            if (codePlacement == RCodePlacement.Table) {
                scriptCode = Invariant($"SELECT RCode FROM {codeTableName} WHERE {SProcColumnName} IS {sprocName}");
            } else {
                var rCode = GetRFileContent(filePath);
                rCode = rCode.EndsWithOrdinal(Environment.NewLine) ? rCode : rCode + Environment.NewLine;
                scriptCode = Environment.NewLine + rCode;
            }
            sprocTemplate = sprocTemplate.Replace(RCodeTemplate, scriptCode);

            var sqlQuery = GetSqlFileContent(filePath + ".sql").Trim();
            return sprocTemplate.Replace(InputQueryTemplate, sqlQuery);
        }

        private void CreateStoredProcedures(SqlSProcPublishSettings settings, EnvDTE.Project targetProject, string targetFolder) {
            foreach (var filePath in settings.Files) {
                var sprocName = settings.SProcNames[filePath];
                if (!string.IsNullOrEmpty(sprocName)) {
                    var template = FillSprocTemplate(filePath, sprocName, settings.CodePlacement, settings.TableName);
                    if (!string.IsNullOrEmpty(template)) {
                        var sprocFile = Path.ChangeExtension(Path.Combine(targetFolder, sprocName), ".sql");
                        _fs.WriteAllText(sprocFile, template);
                        targetProject.ProjectItems.AddFromFile(sprocFile);
                    }
                }
            }
        }

        private void GetSProcNames(SqlSProcPublishSettings settings) {
            settings.SProcNames.Clear();
            foreach (var file in settings.Files) {
                var sprocName = GetSProcNameFromTemplate(file);
                settings.SProcNames[file] = sprocName;
            }
        }

        private string GetSProcNameFromTemplate(string rFilePath) {
            var sprocTemplateFile = rFilePath + ".SProc.sql";
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
            return null;
        }

        private string GetRFileContent(string filePath) {
            if (_fs.FileExists(filePath)) {
                return _fs.ReadAllText(filePath).Replace("'", "''");
            }
            return string.Empty;
        }

        private string GetSqlFileContent(string filePath) {
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
