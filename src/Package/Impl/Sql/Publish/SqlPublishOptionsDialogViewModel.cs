// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf.Collections;
using Microsoft.R.Common.Wpf.Controls;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    internal sealed class SqlPublishOptionsDialogViewModel : BindableBase {
        private readonly ICoreShell _coreShell;
        private readonly IProjectSystemServices _pss;
        private readonly IProjectConfigurationSettingsProvider _pcsp;

        private bool _canPublish;
        private bool _targetHasName;
        private bool _generateTable;
        private int _selectedTargetIndex;
        private int _selectedTargetTypeIndex;
        private int _selectedCodePlacementIndex;
        private int _selectedQuoteTypeIndex;
        private string _targetTooltip;
        private bool _noDbConnections;
        private bool _noDbProjects;

        class DbConnectionData {
            public string Name;
            public string ConnectionString;
        }

        private BatchObservableCollection<string> _targets = new BatchObservableCollection<string>();
        private IReadOnlyList<string> _targetProjects = new List<string>();
        private IReadOnlyList<DbConnectionData> _targetConnections = new List<DbConnectionData>();

        public int SelectedTargetIndex => _selectedTargetIndex;
        public int SelectedTargetTypeIndex => _selectedTargetTypeIndex;
        public int SelectedCodePlacementIndex => _selectedCodePlacementIndex;
        public int SelectedQuoteTypeIndex => _selectedQuoteTypeIndex;

        /// <summary>
        /// Target types: DACPAC, database, project
        /// </summary>
        public IReadOnlyList<string> TargetTypeNames => new string[] {
                Resources.SqlPublishDialog_TargetTypeDacpac,
                Resources.SqlPublishDialog_TargetTypeDatabase,
                Resources.SqlPublishDialog_TargetTypeProject,
                Resources.SqlPublishDialog_TargetTypeFile
            };

        /// <summary>
        /// Code placement: inline or table
        /// </summary>
        public IReadOnlyList<string> CodePlacementNames => new string[] {
                Resources.SqlPublishDialog_RCodeInline,
                Resources.SqlPublishDialog_RCodeInTable,
            };

        /// <summary>
        /// SQL name quoting type: none, square brackets or double quotes.
        /// </summary>
        public IReadOnlyList<string> QuoteTypeNames => new string[] {
                Resources.SqlPublishDialog_NoQuote,
                Resources.SqlPublishDialog_BracketQuote,
                Resources.SqlPublishDialog_DoubleQuote
            };

        public string TargetTooltip {
            get => _targetTooltip;
            set => SetProperty(ref _targetTooltip, value);
        }

        /// <summary>
        /// Determines if current settings are valid.
        /// Controls enabled/disabled state of OK button.
        /// </summary>
        public bool CanPublish {
            get => _canPublish;
            set => SetProperty(ref _canPublish, value);
        }

        /// <summary>
        /// Determines if list of targets is enabled. User can select between target 
        /// database projects, between database connection strings, but target DACPAC 
        /// has fixed name, same as name of the current project.
        /// </summary>
        public bool TargetHasName {
            get => _targetHasName;
            set => SetProperty(ref _targetHasName, value);
        }

        /// <summary>
        /// Controls enabled/disabled state of the table name edit field.
        /// </summary>
        public bool GenerateTable {
            get => _generateTable;
            set => SetProperty(ref _generateTable, value);
        }

        /// <summary>
        /// Publishing settings
        /// </summary>
        public SqlSProcPublishSettings Settings { get; }

        public static async Task<SqlPublishOptionsDialogViewModel> CreateAsync(
            SqlSProcPublishSettings settings,
            ICoreShell coreShell, IProjectSystemServices pss,
            IProjectConfigurationSettingsProvider pcsp) {
            var model = new SqlPublishOptionsDialogViewModel(settings, coreShell, pss, pcsp);
            await model.InitializeAsync();
            return model;
        }

        private SqlPublishOptionsDialogViewModel(SqlSProcPublishSettings settings,
            ICoreShell coreShell, IProjectSystemServices pss,
            IProjectConfigurationSettingsProvider pcsp) {
            _coreShell = coreShell;
            _pss = pss;
            _pcsp = pcsp;

            Settings = settings;
        }

        private Task InitializeAsync() {
            _selectedCodePlacementIndex = (int)Settings.CodePlacement;
            _selectedQuoteTypeIndex = (int)Settings.QuoteType;
            return SelectTargetTypeAsync((int)Settings.TargetType);
        }

        /// <summary>
        /// Publishing targets
        /// </summary>
        public IReadOnlyList<string> Targets {
            get => _targets;
            set {
                _targets.ReplaceWith(value);
                _selectedTargetIndex = -1;
            }
        }

        public void SelectTarget(int index) {
            if (_selectedTargetIndex != index) {
                TargetTooltip = string.Empty;
                if (index >= 0) {
                    var name = Targets[index];
                    switch (Settings.TargetType) {
                        case PublishTargetType.Database:
                            Settings.TargetDatabaseConnection = _targetConnections.FirstOrDefault(c => c.Name.EqualsOrdinal(name))?.ConnectionString;
                            TargetTooltip = Settings.TargetDatabaseConnection;
                            break;

                        case PublishTargetType.Project:
                            Settings.TargetProject = Targets[index];
                            TargetTooltip = Settings.TargetProject;
                            break;

                        case PublishTargetType.File:
                            Settings.TargetProject = _pss.GetSelectedProject<IVsHierarchy>()?.GetDTEProject().Name;
                            TargetTooltip = Settings.TargetProject;
                            break;
                    }
                }
                _selectedTargetIndex = index;
            }
        }

        public async Task SelectTargetTypeAsync(int index) {
            if (_selectedTargetTypeIndex != index) {
                _selectedTargetTypeIndex = index;
                Settings.TargetType = TargetTypeFromName(TargetTypeNames[_selectedTargetTypeIndex]);
                await PopulateTargetsAsync();
                UpdateState();
            }
        }
        public void SelectCodePlacement(int index) {
            if (_selectedCodePlacementIndex != index) {
                _selectedCodePlacementIndex = index;
                Settings.CodePlacement = (RCodePlacement)index;
                UpdateState();
            }
        }
        public void SelectQuoteType(int index) {
            if (_selectedQuoteTypeIndex != index) {
                Settings.QuoteType = (SqlQuoteType)index;
                _selectedQuoteTypeIndex = index;
            }
        }

        public void UpdateState() {
            TargetHasName = Settings.TargetType == PublishTargetType.Database || Settings.TargetType == PublishTargetType.Project;
            GenerateTable = Settings.CodePlacement == RCodePlacement.Table;
            CanPublish = !GenerateTable || !string.IsNullOrEmpty(Settings.TableName);
            if (Settings.TargetType == PublishTargetType.Database || Settings.TargetType == PublishTargetType.Project) {
                CanPublish &= Targets?.Count > 0;
            }
            if (Settings.TargetType == PublishTargetType.Database) {
                CanPublish &= !_noDbConnections;
            }
            if (Settings.TargetType == PublishTargetType.Project) {
                CanPublish &= !_noDbProjects;
            }
        }

        private async Task PopulateTargetsAsync() {
            var project = _pss.GetSelectedProject<IVsHierarchy>().GetConfiguredProject();

            switch (Settings.TargetType) {
                case PublishTargetType.Dacpac:
                    Targets = new List<string>();
                    break;

                case PublishTargetType.Project:
                    PopulateProjectList();
                    break;

                case PublishTargetType.Database:
                    await PopulateDatabaseConnectionsListAsync(project);
                    break;
            }
        }

        private async Task PopulateDatabaseConnectionsListAsync(ConfiguredProject project) {
            var connections = await GetDatabaseConnectionsAsync(project);
            var index = -1;
            _targetConnections = connections;

            if (!string.IsNullOrEmpty(Settings.TargetDatabaseConnection)) {
                var indices = _targetConnections.IndexWhere(c => c.ConnectionString.EqualsIgnoreCase(Settings.TargetDatabaseConnection));
                index = indices.DefaultIfEmpty(-1).First();
            }

            Targets = _targetConnections.Select(c => c.Name).ToList();
            _selectedTargetIndex = index >= 0 ? index : 0;
            UpdateState();
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
                _selectedTargetIndex = index >= 0 ? index : 0;
            }
        }

        private IReadOnlyList<string> GetDatabaseProjects() {
            _noDbProjects = false;
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
                _noDbProjects = true;
                projects.Add(Resources.SqlPublishDialog_NoDatabaseProjects);
            }
            return projects;
        }

        private async Task<IReadOnlyList<DbConnectionData>> GetDatabaseConnectionsAsync(ConfiguredProject project) {
            _noDbConnections = false;
            var connections = new List<DbConnectionData>();
            if (project != null) {
                var result = await project.GetDatabaseConnections(_pcsp);
                foreach (var s in result) {
                    connections.Add(new DbConnectionData { Name = s.Name, ConnectionString = s.Value });
                }
            }
            if (connections.Count == 0) {
                connections.Add(new DbConnectionData { Name = Resources.SqlPublishDialog_NoDatabaseConnections });
                _noDbConnections = true;
            }
            return connections;
        }

        private static PublishTargetType TargetTypeFromName(string name) {
            if (name.EqualsOrdinal(Resources.SqlPublishDialog_TargetTypeDacpac)) {
                return PublishTargetType.Dacpac;
            }
            if (name.EqualsOrdinal(Resources.SqlPublishDialog_TargetTypeDatabase)) {
                return PublishTargetType.Database;
            }
            if (name.EqualsOrdinal(Resources.SqlPublishDialog_TargetTypeProject)) {
                return PublishTargetType.Project;
            }
            if (name.EqualsOrdinal(Resources.SqlPublishDialog_TargetTypeFile)) {
                return PublishTargetType.File;
            }
            Debug.Fail("Unknown target type name");
            return PublishTargetType.Dacpac;
        }
    }
}
