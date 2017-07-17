// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Enums;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Common.Wpf.Controls;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.Settings;
using Microsoft.VisualStudio.R.Package.SurveyNews;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Options.R {
    internal sealed class RSettingsImplementation : BindableBase, IRSettings {
        private const int MaxDirectoryEntries = 8;
        private readonly IServiceContainer _services;
        private readonly ISettingsStorage _settingStorage;
        private readonly ILoggingPermissions _loggingPermissions;

        private string _cranMirror;
        private string _workingDirectory;
        private int _codePage;
        private ConnectionInfo[] _connections = new ConnectionInfo[0];
        private ConnectionInfo _lastActiveConnection;

        private YesNo _showConfirmationDialogWhenSwitch = YesNo.Yes;
        private YesNo _showShowSaveOnResetConfirmationDialog = YesNo.Yes;
        private YesNoAsk _loadRDataOnProjectLoad = YesNoAsk.No;
        private YesNoAsk _saveRDataOnProjectUnload = YesNoAsk.No;
        private bool _alwaysSaveHistory = true;
        private bool _clearFilterOnAddHistory = true;
        private bool _multilineHistorySelection = true;
        private bool _showPackageManagerDisclaimer = true;
        private HelpBrowserType _helpBrowserType = HelpBrowserType.Automatic;
        private bool _showDotPrefixedVariables;
        private SurveyNewsPolicy _surveyNewsCheck = SurveyNewsPolicy.CheckOnceWeek;
        private DateTime _surveyNewsLastCheck;
        private string _surveyNewsFeedUrl = SurveyNewsUrls.Feed;
        private string _surveyNewsIndexUrl = SurveyNewsUrls.Index;
        private bool _evaluateActiveBindings = true;
        private string _webHelpSearchString = "R site:stackoverflow.com";
        private BrowserType _webHelpSearchBrowserType = BrowserType.Internal;
        private BrowserType _htmlBrowserType = BrowserType.External;
        private BrowserType _markdownBrowserType = BrowserType.External;
        private LogVerbosity _logVerbosity = LogVerbosity.Normal;

        public RSettingsImplementation(IServiceContainer services) {
            _services = services;
            _settingStorage = services.GetService<ISettingsStorage>();
            _loggingPermissions = services.GetService<ILoggingPermissions>();
            _workingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public YesNo ShowWorkspaceSwitchConfirmationDialog {
            get => _showConfirmationDialogWhenSwitch;
            set => SetProperty(ref _showConfirmationDialogWhenSwitch, value);
        }

        public YesNo ShowSaveOnResetConfirmationDialog {
            get => _showShowSaveOnResetConfirmationDialog;
            set => SetProperty(ref _showShowSaveOnResetConfirmationDialog, value);
        }

        /// <summary>
        /// Path to 64-bit R installation such as 
        /// 'C:\Program Files\R\R-3.2.2' without bin\x64
        /// </summary>
        public YesNoAsk LoadRDataOnProjectLoad {
            get => _loadRDataOnProjectLoad;
            set => SetProperty(ref _loadRDataOnProjectLoad, value);
        }

        public YesNoAsk SaveRDataOnProjectUnload {
            get => _saveRDataOnProjectUnload;
            set => SetProperty(ref _saveRDataOnProjectUnload, value);
        }

        public bool AlwaysSaveHistory {
            get => _alwaysSaveHistory;
            set => SetProperty(ref _alwaysSaveHistory, value);
        }

        public bool ClearFilterOnAddHistory {
            get => _clearFilterOnAddHistory;
            set => SetProperty(ref _clearFilterOnAddHistory, value);
        }

        public bool MultilineHistorySelection {
            get => _multilineHistorySelection;
            set => SetProperty(ref _multilineHistorySelection, value);
        }

        public bool ShowPackageManagerDisclaimer {
            get => _showPackageManagerDisclaimer;
            set => SetProperty(ref _showPackageManagerDisclaimer, value);
        }

        public string CranMirror {
            get => _cranMirror;
            set => SetProperty(ref _cranMirror, value);
        }

        public int RCodePage {
            get => _codePage;
            set => SetProperty(ref _codePage, value);
        }

        public ConnectionInfo[] Connections {
            get => _connections;
            set => SetProperty(ref _connections, value);
        }

        public ConnectionInfo LastActiveConnection {
            get => _lastActiveConnection;
            set => SetProperty(ref _lastActiveConnection, value);
        }

        public string WorkingDirectory {
            get => _workingDirectory;
            set {
                var newDirectory = value;
                var newDirectoryIsRoot = newDirectory.Length >= 2 && newDirectory[newDirectory.Length - 2] == Path.VolumeSeparatorChar;
                if (!newDirectoryIsRoot) {
                    newDirectory = newDirectory.TrimTrailingSlash();
                }

                SetProperty(ref _workingDirectory, newDirectory);
                UpdateWorkingDirectoryList(newDirectory);

                
                _services?.MainThread().Post(() => {
                    var shell = _services.GetService<IVsUIShell>(typeof(SVsUIShell));
                    shell.UpdateCommandUI(1);
                });
            }
        }

        public IEnumerable<string> WorkingDirectoryList { get; set; } = Enumerable.Empty<string>();
        public HelpBrowserType HelpBrowserType {
            get => _helpBrowserType;
            set => SetProperty(ref _helpBrowserType, value);
        }

        public bool ShowDotPrefixedVariables {
            get => _showDotPrefixedVariables;
            set => SetProperty(ref _showDotPrefixedVariables, value);
        }

        public SurveyNewsPolicy SurveyNewsCheck {
            get => _surveyNewsCheck;
            set => SetProperty(ref _surveyNewsCheck, value);
        }

        public DateTime SurveyNewsLastCheck {
            get => _surveyNewsLastCheck;
            set => SetProperty(ref _surveyNewsLastCheck, value);
        }

        public string SurveyNewsFeedUrl {
            get => _surveyNewsFeedUrl;
            set => SetProperty(ref _surveyNewsFeedUrl, value);
        }

        public string SurveyNewsIndexUrl {
            get => _surveyNewsIndexUrl;
            set => SetProperty(ref _surveyNewsIndexUrl, value);
        }

        public bool EvaluateActiveBindings {
            get => _evaluateActiveBindings;
            set => SetProperty(ref _evaluateActiveBindings, value);
        }

        public string WebHelpSearchString {
            get => _webHelpSearchString;
            set => SetProperty(ref _webHelpSearchString, value);
        }

        public BrowserType WebHelpSearchBrowserType {
            get => _webHelpSearchBrowserType;
            set => SetProperty(ref _webHelpSearchBrowserType, value);
        }

        public BrowserType HtmlBrowserType {
            get => _htmlBrowserType;
            set => SetProperty(ref _htmlBrowserType, value);
        }

        public BrowserType MarkdownBrowserType {
            get => _markdownBrowserType;
            set => SetProperty(ref _markdownBrowserType, value);
        }

        public LogVerbosity LogVerbosity {
            get => _logVerbosity;
            set => SetProperty(ref _logVerbosity, value);
        }

        public bool ShowRToolbar { get; set; } = true;
        public bool ShowHostLoadMeter { get; set; }

        private void UpdateWorkingDirectoryList(string newDirectory) {
            List<string> list = new List<string>(WorkingDirectoryList ?? Enumerable.Empty<string>());
            if (!string.IsNullOrEmpty(newDirectory) && !list.Contains(newDirectory, StringComparer.OrdinalIgnoreCase)) {
                list.Insert(0, newDirectory);
                if (list.Count > MaxDirectoryEntries) {
                    list.RemoveAt(list.Count - 1);
                }

                WorkingDirectoryList = list.ToArray();
            }
        }

        #region IRPersistentSettings
        public void LoadSettings() {
            _settingStorage.LoadPropertyValues(this);
            // Correct setting if stored value exceed currently set maximum
            LogVerbosity = MathExtensions.Min<LogVerbosity>(LogVerbosity, _loggingPermissions.MaxVerbosity);
            _loggingPermissions.CurrentVerbosity = LogVerbosity;
        }

        public Task SaveSettingsAsync() {
            _settingStorage.SavePropertyValues(this);
            return _settingStorage.PersistAsync();
        }

        public void Dispose() {
            if (_settingStorage != null) {
                SaveSettingsAsync().Wait(5000);
                ((IDisposable)_settingStorage).Dispose();
            }
        }
        #endregion
    }
}
