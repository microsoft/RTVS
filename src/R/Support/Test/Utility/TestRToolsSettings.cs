// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Enums;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.ConnectionManager.Implementation;
using Microsoft.R.Components.Settings;
using Microsoft.R.Interpreters;
using Microsoft.R.Support.Settings.Definitions;

namespace Microsoft.R.Support.Test.Utility {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IRSettings))]
    [Export(typeof(IRToolsSettings))]
    public sealed class TestRToolsSettings : IRToolsSettings {
        private static readonly IConnectionInfo[] _connections = {new ConnectionInfo {
            Name = "Test",
            Path = new Uri(new RInstallation().GetRInstallPath()).LocalPath
        }};

        public IConnectionInfo[] Connections {
            get { return _connections; }
            set { }
        }

        public IConnectionInfo LastActiveConnection {
            get { return _connections[0]; }
            set { }
        }

        public string CranMirror {
            get { return string.Empty; }
            set { }
        }

        public bool EscInterruptsCalculation {
            get { return true; }
            set { }
        }

        public YesNoAsk LoadRDataOnProjectLoad {
            get { return YesNoAsk.Yes; }
            set { }
        }

        public YesNoAsk SaveRDataOnProjectUnload {
            get { return YesNoAsk.Yes; }
            set { }
        }

        public bool AlwaysSaveHistory {
            get { return true; }
            set { }
        }

        public bool ClearFilterOnAddHistory {
            get { return true; }
            set { }
        }

        public bool MultilineHistorySelection {
            get { return true; }
            set { }
        }

        public void LoadFromStorage() {
        }

        public string WorkingDirectory { get; set; } = string.Empty;

        public bool ShowPackageManagerDisclaimer { get; set; } = true;

        public string[] WorkingDirectoryList { get; set; } = new string[0];
        
        public HelpBrowserType HelpBrowserType {
            get { return HelpBrowserType.Automatic; }
            set { }
        }

        public bool ShowDotPrefixedVariables { get; set; }

        public SurveyNewsPolicy SurveyNewsCheck {
            get { return SurveyNewsPolicy.Disabled; }
            set { }
        }

        public DateTime SurveyNewsLastCheck {
            get { return DateTime.MinValue; }
            set { }
        }

        public string SurveyNewsFeedUrl {
            get { return string.Empty; }
            set { }
        }

        public string SurveyNewsIndexUrl {
            get { return string.Empty; }
            set { }
        }

        public int RCodePage {
            get { return 1252; }
            set { }
        }

        public string WebHelpSearchString {
            get { return "R site:stackoverflow.com"; }
            set { }
        }

        public BrowserType WebHelpSearchBrowserType {
            get { return BrowserType.Internal; }
            set { }
        }

        public BrowserType ShinyBrowserType {
            get { return BrowserType.Internal; }
            set { }
        }

        public BrowserType MarkdownBrowserType {
            get { return BrowserType.External; }
            set { }
        }

        public bool EvaluateActiveBindings { get; set; } = false;
    }
}
