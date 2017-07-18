// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.Enums;
using Microsoft.Common.Core.Logging;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.Settings;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Editor.Test.Utility {
    [ExcludeFromCodeCoverage]
    public sealed class TestRSettings : IRSettings {
        private readonly ConnectionInfo[] _connections;

        public TestRSettings() : this("Test") {}

        public TestRSettings(string connectionName) {
            _connections = new[] { new ConnectionInfo (
                connectionName ?? "Test",
                new RInstallation().GetCompatibleEngines().FirstOrDefault()?.InstallPath,
                null,
                false
            )};
        }

        public ConnectionInfo[] Connections {
            get => _connections; set { }
        }

        public ConnectionInfo LastActiveConnection {
            get => _connections[0]; set { }
        }

        public string CranMirror {
            get => string.Empty; set { }
        }

        public bool EscInterruptsCalculation {
            get => true; set { }
        }

        public YesNo ShowWorkspaceSwitchConfirmationDialog {
            get => YesNo.Yes; set { }
        }

        public YesNo ShowSaveOnResetConfirmationDialog {
            get => YesNo.Yes; set { }
        }

        public YesNoAsk LoadRDataOnProjectLoad {
            get => YesNoAsk.Yes; set { }
        }

        public YesNoAsk SaveRDataOnProjectUnload {
            get => YesNoAsk.Yes; set { }
        }

        public bool AlwaysSaveHistory {
            get => true; set { }
        }

        public bool ClearFilterOnAddHistory {
            get => true; set { }
        }

        public bool MultilineHistorySelection {
            get => true; set { }
        }

        public void LoadFromStorage() { }

        public string WorkingDirectory { get; set; } = string.Empty;

        public bool ShowPackageManagerDisclaimer { get; set; } = true;

        public IEnumerable<string> WorkingDirectoryList { get; set; } = Enumerable.Empty<string>();

        public HelpBrowserType HelpBrowserType {
            get => HelpBrowserType.Automatic; set { }
        }

        public bool ShowDotPrefixedVariables { get; set; }

        public SurveyNewsPolicy SurveyNewsCheck {
            get => SurveyNewsPolicy.Disabled; set { }
        }

        public DateTime SurveyNewsLastCheck {
            get => DateTime.MinValue; set { }
        }

        public string SurveyNewsFeedUrl {
            get => string.Empty; set { }
        }

        public string SurveyNewsIndexUrl {
            get => string.Empty; set { }
        }

        public int RCodePage {
            get => 1252; set { }
        }

        public string WebHelpSearchString {
            get => "R site:stackoverflow.com"; set { }
        }

        public BrowserType WebHelpSearchBrowserType {
            get => BrowserType.Internal; set { }
        }

        public BrowserType HtmlBrowserType {
            get => BrowserType.External; set { }
        }

        public BrowserType MarkdownBrowserType {
            get => BrowserType.External; set { }
        }

        public bool EvaluateActiveBindings { get; set; } = false;

        public LogVerbosity LogVerbosity {
            get => LogVerbosity.None; set { }
        }

        public bool ShowRToolbar { get; set; } = true;

        public bool ShowHostLoadMeter { get; set; }

        #region IRPersistentSettings
        public void LoadSettings() { }
        public Task SaveSettingsAsync() => Task.CompletedTask;
        public void Dispose() { }
        #endregion

#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
