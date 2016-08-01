// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.VisualStudio.R.Package.Shell;

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

        private void Close(bool saveSettings) {
            if (saveSettings) {
                _model.SaveSettings();
            }
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)  => Close(true);
        private void CancelButton_Click(object sender, RoutedEventArgs e) => Close(false);
        private void TableName_TextChanged(object sender, TextChangedEventArgs e) => _model.UpdateState();

        private void CodePlacementList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var s = CodePlacementList.SelectedItem as string;
            var placement = s.EqualsOrdinal(Package.Resources.SqlPublishDialog_RCodeInline) ? RCodePlacement.Inline : RCodePlacement.Table;
            _model.Settings.CodePlacement = placement;
            _model.UpdateState();
        }

        private void QuoteTypetList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var s = QuoteTypeList.SelectedItem as string;
            SqlQuoteType quoteType;
            if (s.EqualsOrdinal(Package.Resources.SqlPublishDialog_BracketQuote)) {
                quoteType = SqlQuoteType.Bracket;
            } else if(s.EqualsOrdinal(Package.Resources.SqlPublishDialog_NoQuote)) {
                quoteType = SqlQuoteType.None;
            } else {
                quoteType = SqlQuoteType.Quote;
            }
            _model.Settings.QuoteType = quoteType;
        }

        private void TargetTypeList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var s = TargetTypeList.SelectedItem as string;
            _model.TargetHasName = !s.EqualsOrdinal(Package.Resources.SqlPublishDialog_TargetTypeDacpac);
        }
    }
}
