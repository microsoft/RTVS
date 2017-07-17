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

namespace Microsoft.R.Components.Test.Stubs {
    [ExcludeFromCodeCoverage]
    public sealed class RSettingsStub : IRSettings {
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

        public void LoadSettings() { }
        public Task SaveSettingsAsync() => Task.CompletedTask;

        public YesNoAsk LoadRDataOnProjectLoad { get; set; } = YesNoAsk.No;
        public YesNoAsk SaveRDataOnProjectUnload { get; set; } = YesNoAsk.No;

        /// <summary>
        /// Most recently used directories in REPL
        /// </summary>
        public IEnumerable<string> WorkingDirectoryList { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// The frequency at which to check for updated news. Default is once per week.
        /// </summary>
        public SurveyNewsPolicy SurveyNewsCheck { get; set; } = SurveyNewsPolicy.Disabled;

        /// <summary>
        /// The date/time when the last check for news occurred.
        /// </summary>
        public DateTime SurveyNewsLastCheck { get; set; } = DateTime.Now;

        public string SurveyNewsFeedUrl { get; set; }

        public string SurveyNewsIndexUrl { get; set; }

        /// <summary>
        /// Site to search in 'Search Web for'... commands
        /// </summary>
        public string WebHelpSearchString { get; set; }

        public BrowserType WebHelpSearchBrowserType { get; set; } = BrowserType.External;
        public BrowserType HtmlBrowserType { get; set; } = BrowserType.External;
        public BrowserType MarkdownBrowserType { get; set; } = BrowserType.External;

        /// <summary>
        /// Controls visibility of R Toolbar
        /// </summary>
        public bool ShowRToolbar { get; set; } = true;

        /// <summary>
        /// Controls visibility of the host load meter control
        /// (CPU/Memory/Network load display).
        /// </summary>
        public bool ShowHostLoadMeter { get; set; }

        public void Dispose() { }
#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
