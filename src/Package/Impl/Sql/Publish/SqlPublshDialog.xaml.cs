// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
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

        public SqlPublshDialog(ICoreShell coreShell, IProjectSystemServices pss, string folder) :
            this(coreShell, pss, new FileSystem(), folder) {
            InitializeComponent();
            Title = Package.Resources.SqlPublishDialog_Title;
            CheckCanGenerate();
            this.DataContext = _model;

        }

        public SqlPublshDialog(ICoreShell coreShell, IProjectSystemServices pss, IFileSystem fs, string folder) {
            _model = new SqlPublishDialogViewModel(coreShell, pss, fs, folder);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e) {
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

        private void DataGrid_CellGotFocus(object sender, RoutedEventArgs e) {
            // Lookup for the source to be DataGridCell
            if (e.OriginalSource.GetType() == typeof(DataGridCell)) {
                // Starts the Edit on the row;
                DataGrid grd = (DataGrid)sender;
                grd.BeginEdit(e);
            }
        }
    }
}
