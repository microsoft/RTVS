// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Microsoft.Common.Core.IO;
using Microsoft.VisualStudio.R.Package.ProjectSystem;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    internal sealed class SProcProjectFilesGenerator {
        /// <summary>
        /// Name of script file that contains SQL that creates R code table
        /// </summary>
        internal const string CreateRCodeTableScriptName = "CreateRCodeTable.sql";
        /// <summary>
        /// Name of the post-deployment script that inserts actual R code into the table
        /// </summary>
        internal const string PostDeploymentScriptName = "RCode.PostDeployment.sql";

        private readonly IProjectSystemServices _pss;
        private readonly IFileSystem _fs;

        public SProcProjectFilesGenerator(IProjectSystemServices pss, IFileSystem fs) {
            _pss = pss;
            _fs = fs;
        }

        /// <summary>
        /// Generates SQL scripts for the deployment of R code into SQL database.
        /// Writes scripts to files and pushes files into the target database project.
        /// </summary>
        public void Generate(SqlSProcPublishSettings settings, IEnumerable<string> sprocFiles, EnvDTE.Project targetProject) {
            var targetFolder = Path.Combine(Path.GetDirectoryName(targetProject.FullName), "R\\");
            if (!_fs.DirectoryExists(targetFolder)) {
                _fs.CreateDirectory(targetFolder);
            }

            var targetProjectItem = targetProject.ProjectItems.Item("R") ?? targetProject.ProjectItems.AddFolder("R");

            var sprocMap = CreateStoredProcedureFiles(settings, sprocFiles, targetFolder, targetProjectItem);
            if (settings.CodePlacement == RCodePlacement.Table) {
                CreateRCodeTableFile(settings, targetProject, targetFolder, targetProjectItem);
                CreatePostDeploymentScriptFile(settings, targetFolder, targetProjectItem, sprocMap);
            }
        }

        /// <summary>
        /// Create SQL file that defines table template that will hold R code
        /// </summary>
        internal void CreateRCodeTableFile(SqlSProcPublishSettings settings, EnvDTE.Project targetProject, string targetFolder, EnvDTE.ProjectItem targetProjectItem) {
            var creatTableScriptFile = Path.Combine(targetFolder, CreateRCodeTableScriptName);

            var g = new SProcScriptGenerator(_fs);
            var script = g.CreateRCodeTableScript(settings);

            _fs.WriteAllText(creatTableScriptFile, script);
            targetProjectItem.ProjectItems.AddFromFile(creatTableScriptFile);
        }

        /// <summary>
        /// Generates SQL post deployment script that pushes R code into a table
        /// as well as 
        /// </summary>
        private void CreatePostDeploymentScriptFile(SqlSProcPublishSettings settings, 
            string targetFolder, 
            EnvDTE.ProjectItem targetProjectItem, SProcMap sprocMap) {
            var postDeploymentScript = Path.Combine(targetFolder, PostDeploymentScriptName);

            var g = new SProcScriptGenerator(_fs);
            var script = g.CreatePostDeploymentScript(settings, sprocMap);

            _fs.WriteAllText(postDeploymentScript, script);

            var item = targetProjectItem.ProjectItems.AddFromFile(postDeploymentScript);
            item.Properties.Item("BuildAction").Value = "PostDeploy";
        }

        private SProcMap CreateStoredProcedureFiles(SqlSProcPublishSettings settings, IEnumerable<string> sprocFiles, string targetFolder, EnvDTE.ProjectItem targetProjectItem) {
            var g = new SProcScriptGenerator(_fs);

            var sprocMap = g.CreateStoredProcedureScripts(settings, sprocFiles);

            foreach (var name in sprocMap) {
                var template = sprocMap[name];
                if (!string.IsNullOrEmpty(template)) {
                    var sprocFile = Path.ChangeExtension(Path.Combine(targetFolder, name), ".sql");
                    _fs.WriteAllText(sprocFile, template);
                    targetProjectItem.ProjectItems.AddFromFile(sprocFile);
                }
            }
            return sprocMap;
        }
    }
}
