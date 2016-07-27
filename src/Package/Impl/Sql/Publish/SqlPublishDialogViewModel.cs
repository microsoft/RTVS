// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf;
using Microsoft.VisualStudio.R.Package.ProjectSystem;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    internal sealed class SqlPublishDialogViewModel : BindableBase, IDisposable {
        // Store settings during session so they can be reused.
        // Settings are generally per project. Consider saving
        // them in the project file.
        private static string _targetProject;
        private static string _tableName;
        private static RCodePlacement _codePlacement = RCodePlacement.Inline;

        private bool _canGenerate;
        private bool _generateTable;

        public IReadOnlyList<string> TargetProjects { get; private set; }
        public int SelectedTargetProjectIndex { get; set; }
        public IReadOnlyCollection<string> CodePlacementNames { get; private set; }
        public int SelectedCodePlacementIndex { get; set; }
        public SqlSProcPublishSettings Settings { get; private set; }

        public bool CanGenerate {
            get { return _canGenerate; }
            set { SetProperty(ref _canGenerate, value); }
        }
        public bool GenerateTable {
            get { return _generateTable; }
            set { SetProperty(ref _generateTable, value); }
        }

        public SqlPublishDialogViewModel(ICoreShell coreShell, IProjectSystemServices pss, IFileSystem fs, 
                                         IEnumerable<string> filePaths, string projectFolder) {
             PopulateProjectList(pss);
            SelectCodePlacementMode();
        }

        public void Dispose() {
            _targetProject = Settings.TargetProject;
            _tableName = Settings.TableName;
            _codePlacement = Settings.CodePlacement;
        }

        private void PopulateProjectList(IProjectSystemServices pss) {
            TargetProjects = GetDatabaseProjectsInSolution(pss);
            if (TargetProjects.Count > 0) {
                var index = -1;
                if (!string.IsNullOrEmpty(Settings.TargetProject)) {
                    var indices = TargetProjects.IndexWhere(x => x.EqualsIgnoreCase(Settings.TargetProject));
                    index = indices.Count() > 0 ? indices.First() : -1;
                }
                SelectedTargetProjectIndex = index >= 0 ? index : 0;
                Settings.TargetProject = TargetProjects[SelectedCodePlacementIndex];
            }
        }

        private void SelectCodePlacementMode() {
            CodePlacementNames = new string[] {
                Resources.SqlPublishDialog_RCodeInline,
                Resources.SqlPublishDialog_RCodeInTable,
            };
            SelectedCodePlacementIndex = (int)Settings.CodePlacement;
        }

        public static IReadOnlyList<string> GetDatabaseProjectsInSolution(IProjectSystemServices pss) {
            var projectMap = new Dictionary<string, EnvDTE.Project>();
            var solution = pss.GetSolution();
            var projects = new List<string>();
            foreach (EnvDTE.Project project in solution.Projects) {
                try {
                    // Some projects throw 'not implemented'
                    var projectFileName = project.FileName;
                    if (!string.IsNullOrEmpty(projectFileName) && Path.GetExtension(projectFileName).EqualsIgnoreCase(".sqlproj")) {
                        projects.Add(project.Name);
                    }
                } catch (NotImplementedException) { } catch(COMException) { }
            }
            return projects;
        }
    }
}
