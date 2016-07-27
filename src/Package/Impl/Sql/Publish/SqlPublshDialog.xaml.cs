// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.ProjectSystem;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    /// <summary>
    /// Interaction logic for SqlPublsh.xaml
    /// </summary>
    public partial class SqlPublshDialog : DialogWindow {
        private readonly SqlPublishDialogViewModel _model;
        private readonly ICoreShell _coreShell;
        private readonly IProjectSystemServices _pss;
        private readonly IFileSystem _fs;
        private readonly IEnumerable<string> _selectedFiles;
        private readonly string _projectFolder;

        public SqlPublshDialog(ICoreShell coreShell, IProjectSystemServices pss, IEnumerable<string> selectedFiles, string projectFolder) :
            this(coreShell, pss, new FileSystem(), selectedFiles, projectFolder) {
            InitializeComponent();
        }

        internal SqlPublshDialog(ICoreShell coreShell, IProjectSystemServices pss, IFileSystem fs, IEnumerable<string> selectedFiles, string projectFolder) {
            _coreShell = coreShell;
            _pss = pss;
            _fs = fs;
            _selectedFiles = selectedFiles;
            _projectFolder = projectFolder;
            _model = new SqlPublishDialogViewModel(coreShell, pss, fs, selectedFiles, projectFolder);

            Title = Package.Resources.SqlPublishDialog_Title;
            DataContext = _model;
            CheckCanGenerate();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e) {
            try {
                _model.Settings.TargetProject = ProjectList.SelectedItem as string;
                var targetProject = GetSelectedProject(_model.Settings.TargetProject);
                Debug.Assert(targetProject != null);

                var generator = new SProcGenerator(_coreShell, _pss, _fs);
                generator.Generate(_model.Settings, _selectedFiles, targetProject);
            } catch (Exception ex) {
                _coreShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Package.Resources.Error_UnableGenerateSqlFiles, ex.Message));
                GeneralLog.Write(ex);
                if (ex.IsCriticalException()) {
                    throw ex;
                }
            }
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void TableName_TextChanged(object sender, TextChangedEventArgs e) {
            CheckCanGenerate();
        }

        private void CheckCanGenerate() {
            _model.GenerateTable = _model.Settings.CodePlacement == RCodePlacement.Table;
            _model.CanGenerate = (!_model.GenerateTable || !string.IsNullOrEmpty(TableName?.Text)) && _model.TargetProjects.Count > 0;
        }

        private EnvDTE.Project GetSelectedProject(string projectName) {
            var projects = _pss.GetSolution().Projects;
            foreach (EnvDTE.Project p in projects) {
                if (p.Name.EqualsOrdinal(projectName)) {
                    return p;
                }
            }
            return null;
        }

        private void CodePlacementList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var s = CodePlacementList.SelectedItem as string;
            var placement = s.EqualsOrdinal(Package.Resources.SqlPublishDialog_RCodeInline) ? RCodePlacement.Inline : RCodePlacement.Table;
            _model.Settings.CodePlacement = placement;
            CheckCanGenerate();
        }
    }
}
