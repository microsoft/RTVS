// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Common.Core.IO;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using static System.FormattableString;
using Microsoft.Common.Core;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
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

        private readonly IProjectSystemServices _pss;
        private readonly IFileSystem _fs;

        public SProcGenerator(IProjectSystemServices pss, IFileSystem fs) {
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

        private void CreateStoredProcedures(SqlSProcPublishSettings settings, string sourceProjectFolder,
                                            EnvDTE.Project targetProject, string targetFolder, string codeTableName) {
            var projectFolder = Path.GetDirectoryName(targetProject.FullName);
            foreach (var info in settings.SProcInfoEntries) {
                var sprocFile = Path.ChangeExtension(Path.Combine(targetFolder, info.SProcName), ".sql");
                using (var sw = new StreamWriter(sprocFile)) {
                    sw.WriteLine(Invariant($"CREATE PROCEDURE {info.SProcName}"));
                    sw.WriteLine("AS");
                    sw.WriteLine("BEGIN");
                    sw.WriteLine("EXEC sp_execute_external_script");
                    sw.WriteLine("        @language = N'R'");
                    if (settings.CodePlacement == RCodePlacement.Table) {
                        sw.WriteLine(Invariant($"        , @script = 'SELECT RCode FROM {codeTableName} WHERE {SProcColumnName} IS {info.SProcName}'"));
                    } else {
                        var content = GetRFileContent(sourceProjectFolder, info.FilePath);
                        content = content.EndsWithOrdinal(Environment.NewLine) ? content : content + Environment.NewLine;
                        sw.WriteLine(Invariant($"        , @script = N'{Environment.NewLine}{content}'"));
                    }
                    sw.WriteLine("        , @input_data_1 = N''");
                    sw.WriteLine("        , @input_data_1_name = N''");
                    sw.WriteLine("        , @output_data_1_name = N''");
                    sw.WriteLine("END;");
                }
                targetProject.ProjectItems.AddFromFile(sprocFile);
            }
        }

        private string GetRFileContent(string rFilesFolder, string relativePath) {
            var rFilePath = PathHelper.MakeRooted(PathHelper.EnsureTrailingSlash(rFilesFolder), relativePath);
            return _fs.ReadToEnd(rFilePath).Replace("'", "''");
        }
    }
}
