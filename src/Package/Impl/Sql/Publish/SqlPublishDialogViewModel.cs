// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Wpf;
using Microsoft.Languages.Core.Settings;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    internal sealed class SqlPublishDialogViewModel : BindableBase {
        private const string TargetProjectSettingName = "SqlSprocPublishTargetProject";
        private const string TableNameSettingName = "SqlSprocPublishTableName";
        private const string CodePlacementSettingName = "SqlSprocPublishCodePlacement";
        private const string QuoteTypeSettingName = "SqlSprocPublishQuoteType";

        private readonly IProjectSystemServices _pss;
        private readonly IWritableSettingsStorage _settingsStorage;
        private bool _canGenerate;
        private bool _generateTable;

        public IReadOnlyList<string> TargetProjects { get; private set; }
        public int SelectedTargetProjectIndex { get; set; }
        public IReadOnlyCollection<string> CodePlacementNames { get; private set; }
        public int SelectedCodePlacementIndex { get; set; }
        public IReadOnlyCollection<string> QuoteTypeNames { get; private set; }
        public int SelectedQuoteTypeIndex { get; set; }

        public SqlSProcPublishSettings Settings { get; }

        public bool CanGenerate {
            get { return _canGenerate; }
            set { SetProperty(ref _canGenerate, value); }
        }
        public bool GenerateTable {
            get { return _generateTable; }
            set { SetProperty(ref _generateTable, value); }
        }

        public SqlPublishDialogViewModel(IProjectSystemServices pss, IFileSystem fs, IWritableSettingsStorage settingsStorage, IEnumerable<string> sprocFiles) {
            _pss = pss;
            _settingsStorage = settingsStorage;

            Settings = new SqlSProcPublishSettings(sprocFiles, fs);
            LoadSettings();
            PopulateProjectList(pss);
            SelectCodePlacementMode();
            SelectQuoteType();
        }

        public void SaveSettings() {
            _settingsStorage.SetString(TargetProjectSettingName, Settings.TargetProject);
            _settingsStorage.SetString(TableNameSettingName, Settings.TableName);
            _settingsStorage.SetInteger(CodePlacementSettingName, (int)Settings.CodePlacement);
            _settingsStorage.SetInteger(QuoteTypeSettingName, (int)Settings.QuoteType);
        }

        private void LoadSettings() {
            Settings.TargetProject = _settingsStorage.GetString(TargetProjectSettingName, string.Empty);
            Settings.TableName =_settingsStorage.GetString(TableNameSettingName, SqlSProcPublishSettings.DefaultRCodeTableName);
            Settings.CodePlacement = (RCodePlacement)_settingsStorage.GetInteger(CodePlacementSettingName, (int)RCodePlacement.Inline);
            Settings.QuoteType = (SqlQuoteType)_settingsStorage.GetInteger(QuoteTypeSettingName, (int)SqlQuoteType.None);
        }

        private void PopulateProjectList(IProjectSystemServices pss) {
            TargetProjects = GetDatabaseProjectsInSolution(pss);
            if (TargetProjects.Count > 0) {
                var index = -1;
                if (!string.IsNullOrEmpty(Settings.TargetProject)) {
                    var indices = TargetProjects.IndexWhere(x => x.EqualsIgnoreCase(Settings.TargetProject));
                    index = indices.DefaultIfEmpty(-1).First();
                }
                SelectedTargetProjectIndex = index >= 0 ? index : 0;
                Settings.TargetProject = TargetProjects[SelectedCodePlacementIndex];
            }
        }

        private void SelectCodePlacementMode() {
            CodePlacementNames = new string[] {
                Resources.SqlPublishDialog_RCodeInline,
                Resources.SqlPublishDialog_RCodeInTable,
            };
            SelectedCodePlacementIndex = (int)Settings.CodePlacement;
        }

        private void SelectQuoteType() {
            QuoteTypeNames = new string[] {
                Resources.SqlPublishDialog_NoQuote,
                Resources.SqlPublishDialog_BracketQuote,
                Resources.SqlPublishDialog_DoubleQuote,
            };
            SelectedQuoteTypeIndex = (int)Settings.QuoteType;
        }

        public static IReadOnlyList<string> GetDatabaseProjectsInSolution(IProjectSystemServices pss) {
            var projectMap = new Dictionary<string, EnvDTE.Project>();
            var solution = pss.GetSolution();
            var projects = new List<string>();
            foreach (EnvDTE.Project project in solution.Projects) {
                try {
                    // Some projects throw 'not implemented'
                    var projectFileName = project.FileName;
                    if (!string.IsNullOrEmpty(projectFileName) && Path.GetExtension(projectFileName).EqualsIgnoreCase(".sqlproj")) {
                        projects.Add(project.Name);
                    }
                } catch (NotImplementedException) { } catch(COMException) { }
            }
            return projects;
        }
    }
}
