// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf;
using Microsoft.Languages.Core.Settings;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    internal sealed class SqlPublishOptionsDialogViewModel : BindableBase {
        private readonly ICoreShell _coreShell;
        private readonly IProjectSystemServices _pss;
        private readonly IWritableSettingsStorage _settingsStorage;
        private readonly IProjectConfigurationSettingsProvider _pcsp;

        private bool _canGenerate;
        private bool _targetHasName;
        private bool _generateTable;
        private int _selectedTargetIndex;
        private int _selectedTargetTypeIndex;
        private int _selectedCodePlacementIndex;
        private int _selectedQuoteTypeIndex;

        private IReadOnlyList<string> _targets;
        private IReadOnlyList<string> _targetProjects;
        private IReadOnlyList<string> _targetConnections;

        /// <summary>
        /// Target types: DACPAC, database, project
        /// </summary>
        public IReadOnlyList<string> TargetTypeNames { get; private set; }
        public int SelectedTargetTypeIndex {
            get { return _selectedTargetTypeIndex; }
            set {
                _selectedTargetTypeIndex = value;
                PopulateTargets();
                UpdateState();
            }
        }

        /// <summary>
        /// Code placement: inline or table
        /// </summary>
        public IReadOnlyList<string> CodePlacementNames { get; private set; }
        public int SelectedCodePlacementIndex {
            get { return _selectedCodePlacementIndex; }
            set {
                _selectedCodePlacementIndex = value;
                Settings.CodePlacement = (RCodePlacement)value;
                UpdateState();
            }
        }

        /// <summary>
        /// SQL name quoting type: none, square brackets or double quotes.
        /// </summary>
        public IReadOnlyList<string> QuoteTypeNames { get; private set; }
        public int SelectedQuoteTypeIndex {
            get { return _selectedQuoteTypeIndex; }
            set {
                _selectedQuoteTypeIndex = value;
                Settings.QuoteType = (SqlQuoteType)value;
            }
        }

        /// <summary>
        /// Publishing targets
        /// </summary>
        public IReadOnlyList<string> Targets {
            get { return _targets; }
            set { SetProperty(ref _targets, value); }
        }
        public int SelectedTargetIndex {
            get { return _selectedTargetIndex; }
            set {
                _selectedTargetIndex = value;
                if (Settings.TargetType == PublishTargetType.Database) {
                    Settings.TargetDatabaseConnection = Targets[_selectedTargetIndex];
                } else if (Settings.TargetType == PublishTargetType.Project) {
                    Settings.TargetProject = Targets[_selectedTargetIndex];
                }
            }
        }

        /// <summary>
        /// Determines if current settings are valid.
        /// Controls enabled/disabled state of OK button.
        /// </summary>
        public bool CanGenerate {
            get { return _canGenerate; }
            set { SetProperty(ref _canGenerate, value); }
        }

        /// <summary>
        /// Determines if list of targets is enabled. User can select between target 
        /// database projects, between database connection strings, but target DACPAC 
        /// has fixed name, same as name of the current project.
        /// </summary>
        public bool TargetHasName {
            get { return _targetHasName; }
            set { SetProperty(ref _targetHasName, value); }
        }

        /// <summary>
        /// Controls enabled/disabled state of the table name edit field.
        /// </summary>
        public bool GenerateTable {
            get { return _generateTable; }
            set { SetProperty(ref _generateTable, value); }
        }

        /// <summary>
        /// Publishing settings
        /// </summary>
        public SqlSProcPublishSettings Settings { get; }

        public SqlPublishOptionsDialogViewModel(
            ICoreShell coreShell, IProjectSystemServices pss,
            IWritableSettingsStorage settingsStorage, IProjectConfigurationSettingsProvider pcsp) {
            _coreShell = coreShell;
            _pss = pss;
            _settingsStorage = settingsStorage;
            _pcsp = pcsp;

            Settings = new SqlSProcPublishSettings(settingsStorage);

            PopulateTargets();
            SelectCodePlacementMode();
            SelectQuoteType();
            SelectTargetType();
        }

        public void SaveSettings() {
            Settings.Save(_settingsStorage);
        }

        public void UpdateState() {
            GenerateTable = Settings.CodePlacement == RCodePlacement.Table;
            CanGenerate = (!GenerateTable || !string.IsNullOrEmpty(Settings.TableName)) && Targets.Count > 0;
        }

        private void PopulateTargets() {
            switch (Settings.TargetType) {
                case PublishTargetType.Dacpac:
                    Targets = new List<string>();
                    TargetHasName = false;
                    break;

                case PublishTargetType.Project:
                    PopulateProjectList();
                    TargetHasName = true;
                    break;

                case PublishTargetType.Database:
                    PopulateConnectionsListAsync();
                    TargetHasName = false;
                    break;
            }
        }

        private void PopulateConnectionsListAsync() {
            GetDatabaseConnectionsAsync((connections) => {
                var index = -1;
                _targetConnections = connections;
                if (!string.IsNullOrEmpty(Settings.TargetDatabaseConnection)) {
                    var indices = _targetConnections.IndexWhere(x => x.EqualsIgnoreCase(Settings.TargetDatabaseConnection));
                    index = indices.DefaultIfEmpty(-1).First();
                }
                Targets = _targetConnections;
                SelectedTargetIndex = index >= 0 ? index : 0;
            });
        }

        private void PopulateProjectList() {
            _targetProjects = GetDatabaseProjects();
            if (_targetProjects.Count > 0) {
                var index = -1;
                if (!string.IsNullOrEmpty(Settings.TargetProject)) {
                    var indices = _targetProjects.IndexWhere(x => x.EqualsIgnoreCase(Settings.TargetProject));
                    index = indices.DefaultIfEmpty(-1).First();
                }
                Targets = _targetProjects;
                SelectedTargetIndex = index >= 0 ? index : 0;
            }
        }

        private IReadOnlyList<string> GetDatabaseProjects() {
            var projectMap = new Dictionary<string, EnvDTE.Project>();
            var solution = _pss.GetSolution();
            var projects = new List<string>();
            foreach (EnvDTE.Project project in solution.Projects) {
                try {
                    // Some projects throw 'not implemented'
                    var projectFileName = project.FileName;
                    if (!string.IsNullOrEmpty(projectFileName) && Path.GetExtension(projectFileName).EqualsIgnoreCase(".sqlproj")) {
                        projects.Add(project.Name);
                    }
                } catch (NotImplementedException) { } catch (COMException) { }
            }

            if (projects.Count == 0) {
                projects.Add(Resources.SqlPublishDialog_NoDatabaseProjects);
            }
            return projects;
        }

        private void GetDatabaseConnectionsAsync(Action<IReadOnlyList<string>> action) {
            var project = _pss.GetSelectedProject<IVsHierarchy>();
            if (project != null) {
                project.GetDbConnections(_pcsp).ContinueWith(t => {
                    var connections = new List<string>();
                    if (t.IsCompleted) {
                        connections.AddRange(t.Result);
                    }
                    if (connections.Count == 0) {
                        connections.Add(Resources.SqlPublishDialog_NoDatabaseConnections);
                    }
                    _coreShell.DispatchOnUIThread(() => action(connections));
                });
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
                Resources.SqlPublishDialog_DoubleQuote
            };
            SelectedQuoteTypeIndex = (int)Settings.QuoteType;
        }
        private void SelectTargetType() {
            TargetTypeNames = new string[] {
                Resources.SqlPublishDialog_TargetTypeDacpac,
                Resources.SqlPublishDialog_TargetTypeDatabase,
                Resources.SqlPublishDialog_TargetTypeProject
            };
            SelectedTargetTypeIndex = (int)Settings.TargetType;
        }
    }
}
