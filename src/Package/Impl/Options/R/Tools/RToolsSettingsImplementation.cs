// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Enums;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Wpf;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.Settings;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.SurveyNews;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Options.R {
    [Export(typeof(IRSettings))]
    [Export(typeof(IRToolsSettings))]
    internal sealed class RToolsSettingsImplementation : IRToolsSettings, IRPersistentSettings {
        private const int MaxDirectoryEntries = 8;
        private readonly ISettingsStorage _settings;

        private string _cranMirror;
        private string _workingDirectory;
        private int _codePage;

        [ImportingConstructor]
        public RToolsSettingsImplementation(ISettingsStorage settings) {
            _settings = settings;

            // Default settings. Will be overwritten with actual
            // settings (if any) when settings are loaded from storage
            RBasePath = RInstallation.GetRInstallPath(null, null);
            _workingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        /// <summary>
        /// Path to 64-bit R installation such as 
        /// 'C:\Program Files\R\R-3.2.2' without bin\x64
        /// </summary>
        public string RBasePath { get; set; }
        public YesNoAsk LoadRDataOnProjectLoad { get; set; } = YesNoAsk.No;
        public YesNoAsk SaveRDataOnProjectUnload { get; set; } = YesNoAsk.No;
        public bool AlwaysSaveHistory { get; set; } = true;
        public bool ClearFilterOnAddHistory { get; set; } = true;
        public bool MultilineHistorySelection { get; set; } = true;
        public bool ShowPackageManagerDisclaimer { get; set; }

        public string CranMirror {
            get { return _cranMirror; }
            set { SetProperty(ref _cranMirror, value); }
        }

        public int RCodePage {
            get { return _codePage; }
            set { SetProperty(ref _codePage, value); }
        }

        public IConnectionInfo[] Connections {
            get { return _connections; }
            set {
                using (SaveSettings()) {
                    SetProperty(ref _connections, value);
                }
            }
        }

        public IConnectionInfo LastActiveConnection {
            get { return _lastActiveConnection; }
            set {
                using (SaveSettings()) {
                    SetProperty(ref _lastActiveConnection, value);
                }
            }
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
        public string RCommandLineArguments { get; set; }
        public HelpBrowserType HelpBrowserType { get; set; }
        public bool ShowDotPrefixedVariables { get; set; }
        public SurveyNewsPolicy SurveyNewsCheck { get; set; } = SurveyNewsPolicy.CheckOnceWeek;
        public DateTime SurveyNewsLastCheck { get; set; }
        public string SurveyNewsFeedUrl { get; set; } = SurveyNewsUrls.Feed;
        public string SurveyNewsIndexUrl { get; set; } = SurveyNewsUrls.Index;
        public bool EvaluateActiveBindings { get; set; } = true;
        public string WebHelpSearchString { get; set; } = "R site:stackoverflow.com";
        public BrowserType WebHelpSearchBrowserType { get; set; } = BrowserType.Internal;
        public BrowserType ShinyBrowserType { get; set; } = BrowserType.Internal;
        public BrowserType MarkdownBrowserType { get; set; } = BrowserType.External;

        private async Task SetMirrorToSession() {
            IRSessionProvider sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            var sessions = sessionProvider.GetSessions();
            string mirrorName = RToolsSettings.Current.CranMirror;
            string mirrorUrl = CranMirrorList.UrlFromName(mirrorName);

            foreach (var s in sessions.Where(s => s.IsHostRunning)) {
                try {
                    await s.SetVsCranSelectionAsync(mirrorUrl);
                } catch (RException) { } catch (MessageTransportException) { } catch (OperationCanceledException) { }
            }
        }

        private async Task SetSessionCodePage() {
            IRSessionProvider sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            var sessions = sessionProvider.GetSessions();
            var cp = RToolsSettings.Current.RCodePage;

            foreach (var s in sessions.Where(s => s.IsHostRunning)) {
                try {
                    await s.SetCodePageAsync(cp);
                } catch (RException) { } catch (MessageTransportException) {} catch (OperationCanceledException) { }
            }
        }

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
            var properties = this.GetType().GetProperties(BindingFlags.Public);
            foreach (var p in properties) {
                if (_settings.SettingExists(p.Name)) {
                    var value = _settings.GetSetting(p.Name, p.PropertyType);
                    p.SetValue(this, value);
                }
            }
        }

        public void SaveSettings() {
            var dict = ToDictionary();
            foreach (var kvp in dict) {
                _settings.SetSetting(kvp.Key, kvp.Value);
            }
        }
        public IDictionary<string, object> ToDictionary() {
            var dict = new Dictionary<string, object>();
            var properties = this.GetType().GetProperties(BindingFlags.Public);
            foreach (var p in properties) {
                var value = p.GetValue(this);
                dict[p.Name] = value;
            }
            return dict;
        }

        #endregion
    }
}
