// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Enums;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.Settings;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.SurveyNews;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Options.R {
    [Export(typeof(IRSettings))]
    [Export(typeof(IRToolsSettings))]
    [Export(typeof(IRPersistentSettings))]
    internal sealed class RToolsSettingsImplementation : BindableBase, IRPersistentSettings {
        private const int MaxDirectoryEntries = 8;
        private readonly ISettingsStorage _settings;
        private readonly ILoggingPermissions _loggingPermissions;

        private string _cranMirror;
        private string _workingDirectory;
        private int _codePage;
        private ConnectionInfo[] _connections = new ConnectionInfo[0];
        private ConnectionInfo _lastActiveConnection;

        private YesNo _showConfirmationDialogWhenSwitch = YesNo.Yes;
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
        private BrowserType _htmlBrowserType = BrowserType.Internal;
        private BrowserType _markdownBrowserType = BrowserType.External;
        private LogVerbosity _logVerbosity = LogVerbosity.Normal;

        [ImportingConstructor]
        public RToolsSettingsImplementation(ISettingsStorage settings, ICoreShell coreShell) {
            _settings = settings;
            _loggingPermissions = coreShell.Services.LoggingPermissions;
            _workingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public YesNo ShowWorkspaceSwitchConfirmationDialog {
            get { return _showConfirmationDialogWhenSwitch; }
            set { SetProperty(ref _showConfirmationDialogWhenSwitch, value); }
        }

        /// <summary>
        /// Path to 64-bit R installation such as 
        /// 'C:\Program Files\R\R-3.2.2' without bin\x64
        /// </summary>
        public YesNoAsk LoadRDataOnProjectLoad {
            get { return _loadRDataOnProjectLoad; }
            set { SetProperty(ref _loadRDataOnProjectLoad, value); }
        }

        public YesNoAsk SaveRDataOnProjectUnload {
            get { return _saveRDataOnProjectUnload; }
            set { SetProperty(ref _saveRDataOnProjectUnload, value); }
        }

        public bool AlwaysSaveHistory {
            get { return _alwaysSaveHistory; }
            set { SetProperty(ref _alwaysSaveHistory, value); }
        }

        public bool ClearFilterOnAddHistory {
            get { return _clearFilterOnAddHistory; }
            set { SetProperty(ref _clearFilterOnAddHistory, value); }
        }

        public bool MultilineHistorySelection {
            get { return _multilineHistorySelection; }
            set { SetProperty(ref _multilineHistorySelection, value); }
        }

        public bool ShowPackageManagerDisclaimer {
            get { return _showPackageManagerDisclaimer; }
            set { SetProperty(ref _showPackageManagerDisclaimer, value); }
        }

        public string CranMirror {
            get { return _cranMirror; }
            set { SetProperty(ref _cranMirror, value); }
        }

        public int RCodePage {
            get { return _codePage; }
            set { SetProperty(ref _codePage, value); }
        }

        public ConnectionInfo[] Connections {
            get { return _connections; }
            set {
                SetProperty(ref _connections, value);
                SaveSettingsAsync().DoNotWait();
            }
        }

        public ConnectionInfo LastActiveConnection {
            get { return _lastActiveConnection; }
            set { SetProperty(ref _lastActiveConnection, value); }
        }

        public string WorkingDirectory {
            get { return _workingDirectory; }
            set {
                var newDirectory = value;
                var newDirectoryIsRoot = newDirectory.Length >= 2 && newDirectory[newDirectory.Length - 2] == Path.VolumeSeparatorChar;
                if (!newDirectoryIsRoot) {
                    newDirectory = newDirectory.TrimTrailingSlash();
                }

                SetProperty(ref _workingDirectory, newDirectory);
                UpdateWorkingDirectoryList(newDirectory);

                if (EditorShell.HasShell) {
                    VsAppShell.Current.DispatchOnUIThread(() => {
                        IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
                        shell.UpdateCommandUI(1);
                    });
                }
            }
        }

        public IEnumerable<string> WorkingDirectoryList { get; set; } = Enumerable.Empty<string>();
        public HelpBrowserType HelpBrowserType {
            get { return _helpBrowserType; }
            set { SetProperty(ref _helpBrowserType, value); }
        }

        public bool ShowDotPrefixedVariables {
            get { return _showDotPrefixedVariables; }
            set { SetProperty(ref _showDotPrefixedVariables, value); }
        }

        public SurveyNewsPolicy SurveyNewsCheck {
            get { return _surveyNewsCheck; }
            set { SetProperty(ref _surveyNewsCheck, value); }
        }

        public DateTime SurveyNewsLastCheck {
            get { return _surveyNewsLastCheck; }
            set { SetProperty(ref _surveyNewsLastCheck, value); }
        }

        public string SurveyNewsFeedUrl {
            get { return _surveyNewsFeedUrl; }
            set { SetProperty(ref _surveyNewsFeedUrl, value); }
        }

        public string SurveyNewsIndexUrl {
            get { return _surveyNewsIndexUrl; }
            set { SetProperty(ref _surveyNewsIndexUrl, value); }
        }

        public bool EvaluateActiveBindings {
            get { return _evaluateActiveBindings; }
            set { SetProperty(ref _evaluateActiveBindings, value); }
        }

        public string WebHelpSearchString {
            get { return _webHelpSearchString; }
            set { SetProperty(ref _webHelpSearchString, value); }
        }

        public BrowserType WebHelpSearchBrowserType {
            get { return _webHelpSearchBrowserType; }
            set { SetProperty(ref _webHelpSearchBrowserType, value); }
        }

        public BrowserType HtmlBrowserType {
            get { return _htmlBrowserType; }
            set { SetProperty(ref _htmlBrowserType, value); }
        }

        public BrowserType MarkdownBrowserType {
            get { return _markdownBrowserType; }
            set { SetProperty(ref _markdownBrowserType, value); }
        }

        public LogVerbosity LogVerbosity {
            get { return _logVerbosity; }
            set { SetProperty(ref _logVerbosity, value); }
        }

        public bool ShowRToolbar { get; set; } = true;

        private void UpdateWorkingDirectoryList(string newDirectory) {
            List<string> list = new List<string>(WorkingDirectoryList);
            if (!list.Contains(newDirectory, StringComparer.OrdinalIgnoreCase)) {
                list.Insert(0, newDirectory);
                if (list.Count > MaxDirectoryEntries) {
                    list.RemoveAt(list.Count - 1);
                }

                WorkingDirectoryList = list.ToArray();
            }
        }

        #region IRPersistentSettings
        public void LoadSettings() {
            _settings.LoadPropertyValues(this);
            _loggingPermissions.CurrentVerbosity = LogVerbosity;
        }

        public Task SaveSettingsAsync() {
            _settings.SavePropertyValues(this);
            return _settings.PersistAsync();
        }

        public void Dispose() {
            if (_settings != null) {
                SaveSettingsAsync().Wait(5000);
                ((IDisposable)_settings).Dispose();
            }
        }
        #endregion
    }
}
