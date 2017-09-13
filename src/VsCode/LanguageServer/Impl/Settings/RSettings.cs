// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Common.Core.Enums;
using Microsoft.Common.Core.Logging;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.Settings;

namespace Microsoft.R.LanguageServer.Settings {
    internal sealed class RSettings : IRSettings {

        public event PropertyChangedEventHandler PropertyChanged;
        public void Dispose() { }

        public YesNo ShowWorkspaceSwitchConfirmationDialog { get; set; }
        public YesNo ShowSaveOnResetConfirmationDialog { get; set; }
        public bool AlwaysSaveHistory { get; set; }
        public bool ClearFilterOnAddHistory { get; set; }
        public bool MultilineHistorySelection { get; set; }
        public ConnectionInfo[] Connections { get; set; }
        public ConnectionInfo LastActiveConnection { get; set; }
        public string CranMirror { get; set; }
        public string WorkingDirectory { get; set; }
        public bool ShowPackageManagerDisclaimer { get; set; }
        public HelpBrowserType HelpBrowserType { get; set; }
        public int RCodePage { get; set; }
        public bool EvaluateActiveBindings { get; set; }
        public bool ShowDotPrefixedVariables { get; set; }
        public LogVerbosity LogVerbosity { get; set; }
        public void LoadSettings() {
            throw new NotImplementedException();
        }

        public Task SaveSettingsAsync() {
            throw new NotImplementedException();
        }

        public YesNoAsk LoadRDataOnProjectLoad { get; set; }
        public YesNoAsk SaveRDataOnProjectUnload { get; set; }
        public IEnumerable<string> WorkingDirectoryList { get; set; }
        public SurveyNewsPolicy SurveyNewsCheck { get; set; }
        public DateTime SurveyNewsLastCheck { get; set; }
        public string SurveyNewsFeedUrl { get; set; }
        public string SurveyNewsIndexUrl { get; set; }
        public string WebHelpSearchString { get; set; }
        public BrowserType WebHelpSearchBrowserType { get; set; }
        public BrowserType HtmlBrowserType { get; set; }
        public BrowserType MarkdownBrowserType { get; set; }
        public bool ShowRToolbar { get; set; }
        public bool ShowHostLoadMeter { get; set; }
        public bool GridDynamicEvaluation { get; set; }
    }
}
