// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Common.Core.Enums;
using Microsoft.R.Components.Settings;
using Microsoft.R.Support.Settings.Definitions;

namespace Microsoft.R.Support.Test.Utility {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IRToolsSettings))]
    public sealed class TestRToolsSettings : IRToolsSettings {
        public string CranMirror {
            get { return string.Empty; }
            set { }
        }

        public string RBasePath {
            get {
                // Test settings are fixed and are unrelated to what is stored in VS.
                // Therefore we need to look up R when it is not in the registry.
                string programFiles = Environment.GetEnvironmentVariable("ProgramW6432");
                if (programFiles == null) {
                    return string.Empty;
                }

                var topDir = new DirectoryInfo(Path.Combine(programFiles, "R"));
                if (!topDir.Exists) {
                    topDir = new DirectoryInfo(Path.Combine(programFiles, @"Microsoft\MRO-for-RRE\8.0"));
                    if (!topDir.Exists) {
                        return string.Empty;
                    }
                }

                foreach (var dir in topDir.EnumerateDirectories()) {
                    if (dir.Name.StartsWith("R-3.2.")) {
                        return dir.FullName;
                    }
                }

                return string.Empty;
            }
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

        public string RCommandLineArguments { get; set; }

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
            get { return "stackoverflow.com"; }
            set { }
        }

        public bool EvaluateActiveBindings { get; set; } = false;
    }
}
