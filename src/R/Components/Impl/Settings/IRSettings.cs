// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using Microsoft.Common.Core.Enums;
using Microsoft.Common.Core.Logging;
using Microsoft.R.Components.ConnectionManager;

namespace Microsoft.R.Components.Settings {
    public interface IRSettings : INotifyPropertyChanged {
        YesNo ShowWorkspaceSwitchConfirmationDialog { get; set; }

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

        LogVerbosity LogVerbosity { get; set; }
    }
}
