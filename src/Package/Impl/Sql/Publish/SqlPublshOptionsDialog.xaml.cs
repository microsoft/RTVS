// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using EnvDTE;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Wpf;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    /// <summary>
    /// Interaction logic for SqlPublsh.xaml
    /// </summary>
    public partial class SqlPublshOptionsDialog : PlatformDialogWindow {
        private readonly ICoreShell _shell;
        private readonly IProjectSystemServices _pss;
        private readonly IProjectConfigurationSettingsProvider _pcsp;
        private readonly ISettingsStorage _settings;
        private SqlPublishOptionsDialogViewModel _model;

        public static async Task<SqlPublshOptionsDialog> CreateAsync(
            ICoreShell shell, IProjectSystemServices pss, IProjectConfigurationSettingsProvider pcsp, ISettingsStorage settings) {
            var dialog = new SqlPublshOptionsDialog(shell, pss, shell.FileSystem(), pcsp, settings);
            await dialog.InitializeModelAsync();
            return dialog;
        }

        private SqlPublshOptionsDialog(ICoreShell shell, IProjectSystemServices pss, IFileSystem fs, IProjectConfigurationSettingsProvider pcsp, ISettingsStorage settings) {
            _shell = shell;
            _pss = pss;
            _pcsp = pcsp;
            _settings = settings;

            Title = Package.Resources.SqlPublishDialog_Title;
        }

        public async Task InitializeAsync() {
            await _shell.SwitchToMainThreadAsync();
            InitializeComponent();
            InitializeUI();
        }

        private async Task InitializeModelAsync() {
            var settings = new SqlSProcPublishSettings(_settings);
            _model = await SqlPublishOptionsDialogViewModel.CreateAsync(settings, _shell, _pss, _pcsp);
            DataContext = _model;
        }

        private void InitializeUI() {
            TargetTypeList.SelectedIndex = _model.SelectedTargetTypeIndex;
            TargetList.SelectedIndex = _model.SelectedTargetIndex;
            CodePlacementList.SelectedIndex = _model.SelectedQuoteTypeIndex;
            QuoteTypeList.SelectedIndex = _model.SelectedQuoteTypeIndex;
        }

        private void SaveSettingsAndClose() {
            _model.Settings.Save(_settings);

            // Make sure all files are saved and up to date on disk.
            var dte = _shell.GetService<DTE>(typeof(DTE));
            dte.ExecuteCommand("File.SaveAll");

            _shell.PostCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdPublishSProc);
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e) => SaveSettingsAndClose();
        private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
        private void TableName_TextChanged(object sender, TextChangedEventArgs e) => _model.UpdateState();

        private void TargetTypeList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            _model.SelectTargetTypeAsync(TargetTypeList.SelectedIndex).ContinueWith(t => {
                _shell.MainThread().Post(() => {
                    TargetList.SelectedIndex = _model.SelectedTargetIndex;
                    _model.SelectTarget(TargetList.SelectedIndex);
                });
            }).DoNotWait();
        }

        private void TargetList_SelectionChanged(object sender, SelectionChangedEventArgs e) 
            => _model.SelectTarget(TargetList.SelectedIndex);

        private void CodePlacementList_SelectionChanged(object sender, SelectionChangedEventArgs e) 
            => _model.SelectCodePlacement(CodePlacementList.SelectedIndex);

        private void QuoteTypeList_SelectionChanged(object sender, SelectionChangedEventArgs e) 
            => _model.SelectQuoteType(QuoteTypeList.SelectedIndex);
    }
}
