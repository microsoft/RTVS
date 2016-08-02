// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    /// <summary>
    /// Interaction logic for SqlPublsh.xaml
    /// </summary>
    public partial class SqlPublshOptionsDialog : DialogWindow {
        private readonly SqlPublishOptionsDialogViewModel _model;
        private readonly IApplicationShell _appShell;
        private readonly IProjectSystemServices _pss;

        public SqlPublshOptionsDialog(IApplicationShell appShell, IProjectSystemServices pss, IProjectConfigurationSettingsProvider pcsp) :
            this(appShell, pss, new FileSystem(), pcsp) {
            InitializeComponent();
        }

        internal SqlPublshOptionsDialog(IApplicationShell appShell, IProjectSystemServices pss, IFileSystem fs, IProjectConfigurationSettingsProvider pcsp) {
            _appShell = appShell;
            _pss = pss;
            _model = new SqlPublishOptionsDialogViewModel(appShell, pss, appShell.SettingsStorage, pcsp);

            Title = Package.Resources.SqlPublishDialog_Title;
            DataContext = _model;
            _model.UpdateState();
        }

        private void SaveSettingsAndClose() {
            _model.SaveSettings();

            var uiShell = _appShell.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            var guid = RGuidList.RCmdSetGuid;
            var o = new object();
            uiShell.PostExecCommand(ref guid, RPackageCommandId.icmdPublishSProc, 0, ref o);
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e) => SaveSettingsAndClose();
        private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
        private void TableName_TextChanged(object sender, TextChangedEventArgs e) => _model.UpdateState();
    }
}
