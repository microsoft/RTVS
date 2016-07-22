// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
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
            _model.Settings.TargetProject = listProjects.SelectedItem as string;
            _model.Settings.Save(_pss, _folder);

            var targetFolder = GetSelectedProjectFolder(_model.Settings.TargetProject);
            Debug.Assert(!string.IsNullOrEmpty(targetFolder));

            var generator = new SProcGenerator(_pss, _fs);
            generator.Generate(_model.Settings, _folder, targetFolder);

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void TableName_TextChanged(object sender, TextChangedEventArgs e) {
            CheckCanGenerate();
        }

        private void CheckCanGenerate() {
            _model.CanGenerate = !string.IsNullOrEmpty(TableName.Text) && _model.TargetProjects.Count > 0;
        }

        private string GetSelectedProjectFolder(string projectName) {
            var projects = _pss.GetSolution().Projects;
            foreach(EnvDTE.Project p in projects) {
                if(p.Name.EqualsOrdinal(projectName)) {
                    return Path.GetDirectoryName(p.FullName);
                }
            }
            return null;
        }
    }
}
