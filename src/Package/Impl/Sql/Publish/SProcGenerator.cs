// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        private readonly IProjectSystemServices _pss;
        private readonly IFileSystem _fs;

        public SProcGenerator(IProjectSystemServices pss, IFileSystem fs) {
            _pss = pss;
            _fs = fs;
        }

        public void Generate(SqlSProcPublishSettings settings, string rFilesFolder, EnvDTE.Project targetProject) {
            rFilesFolder = PathHelper.EnsureTrailingSlash(rFilesFolder);
            var targetProjectFolder = PathHelper.EnsureTrailingSlash(Path.GetDirectoryName(targetProject.FullName));

            // Create SQL file with the table that will hold R code
            var sqlTableFile = Path.Combine(targetProjectFolder, "RCodeTable.sql");
            var codeTableName = Invariant($"[dbo].[{settings.TableName}]");
            using (var sw = new StreamWriter(sqlTableFile)) {
                sw.WriteLine(Invariant($"CREATE TABLE {codeTableName}"));
                sw.WriteLine("(");
                sw.WriteLine("[Variable] NVARCHAR(64) NOT NULL,");
                sw.WriteLine("[Code] NVARCHAR(max) NOT NULL,");
                sw.WriteLine(")");
                sw.WriteLine(string.Empty);

                sw.WriteLine(Invariant($"INSERT INTO {codeTableName}"));
                for (int i = 0; i < settings.SProcInfoEntries.Count; i++) {
                    var info = settings.SProcInfoEntries[i];
                    var rFilePath = PathHelper.MakeRooted(rFilesFolder, info.FilePath);
                    using (var sr = new StreamReader(rFilePath)) {
                        var content = sr.ReadToEnd();
                        content = content.Replace("'", "''");
                        var varName = info.VariableName[0] == '@' ? info.VariableName.Substring(1) : info.VariableName;
                        sw.Write(Invariant($"VALUES ('{varName}', '{content}')"));
                    }
                    if (i < settings.SProcInfoEntries.Count - 1) {
                        sw.Write(',');
                    }
                    sw.WriteLine(string.Empty);
                }
            }

            targetProject.ProjectItems.AddFromFile(sqlTableFile);

            if (settings.GenerateStoredProcedures) {
                foreach (var info in settings.SProcInfoEntries) {
                    var sprocFile = Path.Combine(targetProjectFolder, Invariant($"SProcR_{info.SProcName}.sql"));
                    var varName = info.VariableName[0] == '@' ? info.VariableName.Substring(1) : info.VariableName;
                    using (var sw = new StreamWriter(sprocFile)) {
                        sw.WriteLine(Invariant($"DECLARE @{varName} AS NVARCHAR(max) = 'SELECT Code FROM {codeTableName} WHERE Variable IS {varName}'"));
                        sw.WriteLine(Invariant($"CREATE PROCEDURE {info.SProcName}"));
                        sw.WriteLine("AS");
                        sw.WriteLine("BEGIN");
                        sw.WriteLine("EXEC sp_execute_external_script");
                        sw.WriteLine("@language = N'R'");
                        sw.WriteLine(Invariant($", @script = @{varName}"));
                        sw.WriteLine(", @input_data_1 = N''");
                        sw.WriteLine(", @input_data_1_name = N''");
                        sw.WriteLine(", @output_data_1_name = N''");
                        sw.WriteLine("END;");
                    }
                    targetProject.ProjectItems.AddFromFile(sprocFile);
                }
            }
        }
    }
}
