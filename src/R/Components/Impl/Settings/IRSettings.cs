// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.Settings {
    public interface IRSettings {
        bool AlwaysSaveHistory { get; set; }
        bool ClearFilterOnAddHistory { get; set; }
        bool MultilineHistorySelection { get; set; }

        /// <summary>
        /// Path to 64-bit R installation such as 
        /// 'C:\Program Files\R\R-3.2.2' without bin\x64
        /// </summary>
        string RBasePath { get; set; }

        Uri BrokerUri { get; set; }

        /// <summary>
        /// Selected CRAN mirror
        /// </summary>
        string CranMirror { get; set; }

        /// <summary>
        /// Additional command line arguments to pass
        /// to the R Host process
        /// </summary>
        string RCommandLineArguments { get; set; }

        /// <summary>
        /// Current working directory for REPL
        /// </summary>
        string WorkingDirectory { get; set; }

        /// <summary>
        /// Show/Hide legal disclaimer for package manager
        /// </summary>
        bool ShowPackageManagerDisclaimer { get; set; }

        /// <summary>
        /// Determines if R Tools should always be using external Web browser or
        /// try and send Help pages to the Help window and other Web requests 
        /// to the external default Web browser.
        /// </summary>
        HelpBrowserType HelpBrowserType { get; set; }

        /// <summary>
        /// R code page (LC_CTYPE)
        /// </summary>
        int RCodePage { get; set; }

        /// <summary>
        /// This is used in describe_children to evaluate active bindings. This
        /// is false by default.
        /// </summary>
        bool EvaluateActiveBindings { get; set; }
    }
}
