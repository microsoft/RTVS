// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf;
using Microsoft.VisualStudio.R.Package.ProjectSystem;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    internal sealed class SqlPublishDialogViewModel : BindableBase {
        private bool _canGenerate;

        public IReadOnlyCollection<string> TargetProjects { get; private set; }
        public int SelectedTargetProjectIndex { get; private set; }
        public SqlSProcPublishSettings Settings { get; private set; }
        public ObservableCollection<SProcInfo> SProcInfoEntries { get; private set; }

        public bool CanGenerate {
            get { return _canGenerate; }
            set { SetProperty(ref _canGenerate, value); }
        }

        public SqlPublishDialogViewModel(ICoreShell coreShell, IProjectSystemServices pss, IFileSystem fs, string folder) {
            Settings = SqlSProcPublishSettings.LoadSettings(coreShell, pss, fs, folder);
            SProcInfoEntries = new ObservableCollection<SProcInfo>(Settings.SProcInfoEntries);
            PopulateProjectList(pss);
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

        private IReadOnlyCollection<string> GetDatabaseProjectsInSolution(IProjectSystemServices pss) {
            var solution = pss.GetSolution();
            var projects = new List<string>();
            foreach (EnvDTE.Project project in solution.Projects) {
                foreach (var prop in project.Properties) {
                }
                projects.Add(project.Name);
            }
            return projects;
        }
    }
}
