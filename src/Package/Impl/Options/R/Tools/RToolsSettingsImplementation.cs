// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Enums;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.Settings.Mirrors;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.SurveyNews;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Options.R {
    [Export(typeof(IRSettings))]
    [Export(typeof(IRToolsSettings))]
    internal sealed class RToolsSettingsImplementation : IRToolsSettings {
        private const int MaxDirectoryEntries = 8;
        private string _cranMirror;
        private string _workingDirectory;
        private int _codePage;
        private bool _showPackageManagerDisclaimer = true;
        private IConnectionInfo[] _connections = new IConnectionInfo[0];
        private IConnectionInfo _lastActiveConnection;

        public YesNoAsk LoadRDataOnProjectLoad { get; set; } = YesNoAsk.No;

        public YesNoAsk SaveRDataOnProjectUnload { get; set; } = YesNoAsk.No;

        public bool AlwaysSaveHistory { get; set; } = true;

        public bool ClearFilterOnAddHistory { get; set; } = true;

        public bool MultilineHistorySelection { get; set; } = true;

        public bool ShowPackageManagerDisclaimer {
            get { return _showPackageManagerDisclaimer; }
            set {
                using (SaveSettings()) {
                    _showPackageManagerDisclaimer = value;
                }
            }
        }

        public string CranMirror {
            get { return _cranMirror; }
            set {
                _cranMirror = value;
                SetMirrorToSession().DoNotWait();
            }
        }

        public int RCodePage {
            get { return _codePage; }
            set {
                _codePage = value;
                SetSessionCodePage().DoNotWait();
            }
        }

        public IConnectionInfo[] Connections {
            get { return _connections; }
            set {
                using (SaveSettings()) {
                    _connections = value;
                }
            }
        }

        public IConnectionInfo LastActiveConnection {
            get { return _lastActiveConnection; }
            set {
                using (SaveSettings()) {
                    _lastActiveConnection = value;
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

                _workingDirectory = newDirectory;
                UpdateWorkingDirectoryList(newDirectory);

                if (EditorShell.HasShell) {
                    VsAppShell.Current.DispatchOnUIThread(() => {
                        IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
                        shell.UpdateCommandUI(1);
                    });
                }
            }
        }

        public string[] WorkingDirectoryList { get; set; } = new string[0];
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

        public RToolsSettingsImplementation() {
            _workingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private async Task SetMirrorToSession() {
            var sessions = GetRSessions();
            string mirrorName = RToolsSettings.Current.CranMirror;
            string mirrorUrl = CranMirrorList.UrlFromName(mirrorName);

            foreach (var s in sessions.Where(s => s.IsHostRunning)) {
                try {
                    using (var eval = await s.BeginEvaluationAsync()) {
                        await eval.SetVsCranSelectionAsync(mirrorUrl);
                    }
                } catch (RException) {
                } catch (OperationCanceledException) {
                }
            }
        }

        private async Task SetSessionCodePage() {
            var sessions = GetRSessions();
            var cp = RToolsSettings.Current.RCodePage;

            foreach (var s in sessions.Where(s => s.IsHostRunning)) {
                try {
                    using (var eval = await s.BeginEvaluationAsync()) {
                        await eval.SetCodePageAsync(cp);
                    }
                } catch (OperationCanceledException) { }
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

        private static IDisposable SaveSettings() {
            var page = (RToolsOptionsPage)RPackage.Current.GetDialogPage(typeof(RToolsOptionsPage));
            return page != null && !page.IsLoadingFromStorage
                ? Disposable.Create(() => page.SaveSettings())
                : Disposable.Empty;
        }

        private static IEnumerable<IRSession> GetRSessions() {
            var provider = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>();
            var instance = provider.Active;
            return instance != null ? instance.RSessions.GetSessions() : Enumerable.Empty<IRSession>();
        }
    }
}
