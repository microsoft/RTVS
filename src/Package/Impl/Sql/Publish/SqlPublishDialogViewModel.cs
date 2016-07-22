// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf;
using Microsoft.VisualStudio.R.Package.ProjectSystem;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    internal sealed class SqlPublishDialogViewModel : BindableBase {
        private readonly Dictionary<string, EnvDTE.Project> _projectMap = new Dictionary<string, EnvDTE.Project>();
        private bool _canGenerate;
        private bool _generateTable;

        public IReadOnlyCollection<string> TargetProjects { get; private set; }
        public int SelectedTargetProjectIndex { get; set; }
        public SqlSProcPublishSettings Settings { get; private set; }
        public int SelectedCodePlacementIndex { get; set; }

        public bool CanGenerate {
            get { return _canGenerate; }
            set { SetProperty(ref _canGenerate, value); }
        }
        public bool GenerateTable {
            get { return _generateTable; }
            set { SetProperty(ref _generateTable, value); }
        }

        public SqlPublishDialogViewModel(ICoreShell coreShell, IProjectSystemServices pss, IFileSystem fs, string folder) {
            Settings = SqlSProcPublishSettings.LoadSettings(coreShell, pss, fs, folder);
            PopulateProjectList(pss);
            SelectCodePlacementMode();
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
            }
        }

        private void SelectCodePlacementMode() {
            SelectedCodePlacementIndex = (int)Settings.CodePlacement;
        }

        private IReadOnlyCollection<string> GetDatabaseProjectsInSolution(IProjectSystemServices pss) {
            var solution = pss.GetSolution();
            var projects = new List<string>();
            foreach (EnvDTE.Project project in solution.Projects) {
                var projectFileName = project.FileName;
                if (!string.IsNullOrEmpty(projectFileName) && Path.GetExtension(projectFileName).EqualsIgnoreCase(".sqlproj")) {
                    projects.Add(project.Name);
                    _projectMap[project.Name] = project;
                }
            }
            return projects;
        }
    }
}
