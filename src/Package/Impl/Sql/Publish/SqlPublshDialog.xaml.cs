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
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    /// <summary>
    /// Interaction logic for SqlPublsh.xaml
    /// </summary>
    public partial class SqlPublshDialog : DialogWindow {
        private readonly SqlPublishDialogViewModel _model;
        private readonly IApplicationShell _appShell;
        private readonly IProjectSystemServices _pss;
        private readonly IFileSystem _fs;

        public SqlPublshDialog(IApplicationShell appShell, IProjectSystemServices pss, IEnumerable<string> sprocFiles) :
            this(appShell, pss, new FileSystem(), sprocFiles) {
            InitializeComponent();
        }

        internal SqlPublshDialog(IApplicationShell appShell, IProjectSystemServices pss, IFileSystem fs, IEnumerable<string> sprocFiles) {
            _appShell = appShell;
            _pss = pss;
            _fs = fs;
            _model = new SqlPublishDialogViewModel(pss, fs, appShell.SettingsStorage, sprocFiles);

            Title = Package.Resources.SqlPublishDialog_Title;
            DataContext = _model;
            CheckCanGenerate();
        }

        private void Close(bool saveSettings) {
            if (saveSettings) {
                _model.SaveSettings();
            }
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e) {
            try {
                _model.Settings.TargetProject = ProjectList.SelectedItem as string;
                var targetProject = GetSelectedProject(_model.Settings.TargetProject);
                Debug.Assert(targetProject != null);

                var generator = new SProcGenerator(_appShell, _pss, _fs);
                generator.Generate(_model.Settings, targetProject);
            } catch (Exception ex) {
                _appShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Package.Resources.Error_UnableGenerateSqlFiles, ex.Message));
                GeneralLog.Write(ex);
                if (ex.IsCriticalException()) {
                    throw ex;
                }
            }
            Close(true);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close(false);
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
    }
}
