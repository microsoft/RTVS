// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Common.Core.Enums;
using Microsoft.Common.Core.Logging;
using Microsoft.R.Components.ConnectionManager;

namespace Microsoft.R.Components.Settings {
    public interface IRSettings : INotifyPropertyChanged, IDisposable {
        YesNo ShowWorkspaceSwitchConfirmationDialog { get; set; }
        YesNo ShowSaveOnResetConfirmationDialog { get; set; }
        bool AlwaysSaveHistory { get; set; }
        bool ClearFilterOnAddHistory { get; set; }
        bool MultilineHistorySelection { get; set; }

        /// <summary>
        /// Array of saved connections
        /// Sorted by latest usage
        /// </summary>
        ConnectionInfo[] Connections { get; set; }

        /// <summary>
        /// Latest active connection
        /// May not be in list of  <see cref="Connections"/>
        /// </summary>
        ConnectionInfo LastActiveConnection { get; set; }

        /// <summary>
        /// Selected CRAN mirror
        /// </summary>
        string CranMirror { get; set; }
        
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

        bool ShowDotPrefixedVariables { get; set; }

        LogVerbosity LogVerbosity { get; set; }

        void LoadSettings();
        Task SaveSettingsAsync();

        YesNoAsk LoadRDataOnProjectLoad { get; set; }
        YesNoAsk SaveRDataOnProjectUnload { get; set; }

        /// <summary>
        /// Most recently used directories in REPL
        /// </summary>
        IEnumerable<string> WorkingDirectoryList { get; set; }

        /// <summary>
        /// Site to search in 'Search Web for'... commands
        /// </summary>
        string WebHelpSearchString { get; set; }

        BrowserType WebHelpSearchBrowserType { get; set; }
        BrowserType HtmlBrowserType { get; set; }
        BrowserType MarkdownBrowserType { get; set; }

        /// <summary>
        /// Controls visibility of R Toolbar
        /// </summary>
        bool ShowRToolbar { get; set; }

        /// <summary>
        /// Controls visibility of the host load meter control
        /// (CPU/Memory/Network load display).
        /// </summary>
        bool ShowHostLoadMeter { get; set; }

        /// <summary>
        /// Controls evaluation of the expression in the grid viewer.
        /// </summary>
        /// <remarks>
        /// By default View(x) takes snapshot of data as a data frame. This may consume subtantial 
        /// amount of memory with large data sets. With dynamic evaluation the expression is evaluated
        /// every time grid refreshes in order to only fetch part of the data for display. However, if 
        /// the variable changes the data in the grid will also change. This mode may be unsuitable 
        /// for dplyr pipe expressions.
        /// </remarks>
        bool GridDynamicEvaluation { get; set; }
    }
}
