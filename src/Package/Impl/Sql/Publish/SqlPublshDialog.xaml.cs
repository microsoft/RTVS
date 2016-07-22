// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
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
        private readonly string _folder;

        public SqlPublshDialog(ICoreShell coreShell, IProjectSystemServices pss, string folder) :
            this(coreShell, pss, new FileSystem(), folder) {
            InitializeComponent();
            Title = Package.Resources.SqlPublishDialog_Title;
            CheckCanGenerate();

            this.HelpText.Text = string.Format(CultureInfo.InvariantCulture, Package.Resources.SqlPublishDialog_Help, 
                                    Environment.NewLine + Environment.NewLine,
                                    Environment.NewLine + Environment.NewLine);
            this.CodePlacementList.SelectionChanged += OnCodePlacementList_SelectionChanged;
            this.DataContext = _model;

        }

        public SqlPublshDialog(ICoreShell coreShell, IProjectSystemServices pss, IFileSystem fs, string folder) {
            _coreShell = coreShell;
            _pss = pss;
            _fs = fs;
            _folder = folder;
            _model = new SqlPublishDialogViewModel(coreShell, pss, fs, folder);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e) {
            try {
                _model.Settings.CodePlacement = GetSelectedCodePlacement();
                _model.Settings.TargetProject = ProjectList.SelectedItem as string;
                _model.Settings.Save(_pss, _folder);

                var targetProject = GetSelectedProject(_model.Settings.TargetProject);
                Debug.Assert(targetProject != null);

                var generator = new SProcGenerator(_pss, _fs);
                generator.Generate(_model.Settings, _folder, targetProject);
            } catch(Exception ex) {
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

        private void OnCodePlacementList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            _model.GenerateTable = GetSelectedCodePlacement() == RCodePlacement.Table;
            CheckCanGenerate();
        }

        private void TableName_TextChanged(object sender, TextChangedEventArgs e) {
            CheckCanGenerate();
        }

        private void CheckCanGenerate() {
            _model.CanGenerate = (GetSelectedCodePlacement() == RCodePlacement.Inline || !string.IsNullOrEmpty(TableName.Text)) && _model.TargetProjects.Count > 0;
        }

        private EnvDTE.Project GetSelectedProject(string projectName) {
            var projects = _pss.GetSolution().Projects;
            foreach(EnvDTE.Project p in projects) {
                if(p.Name.EqualsOrdinal(projectName)) {
                    return p;
                }
            }
            return null;
        }

        private RCodePlacement GetSelectedCodePlacement() {
            var s = ((ComboBoxItem)CodePlacementList.SelectedItem).Content as string;
            return s.EqualsOrdinal(Package.Resources.SqlPublishDialog_RCodeInline) ? RCodePlacement.Inline : RCodePlacement.Table;
        }
    }
}
