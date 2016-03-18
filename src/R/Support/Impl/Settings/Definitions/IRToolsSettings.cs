// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Enums;
using Microsoft.R.Components.Settings;

namespace Microsoft.R.Support.Settings.Definitions {
    public interface IRToolsSettings : IRSettings {
        YesNoAsk LoadRDataOnProjectLoad { get; set; }
        YesNoAsk SaveRDataOnProjectUnload { get; set; }

        /// <summary>
        /// Most recently used directories in REPL
        /// </summary>
        string[] WorkingDirectoryList { get; set; }

        /// <summary>
        /// Determines if R Tools should always be using external Web browser or
        /// try and send Help pages to the Help window and other Web requests 
        /// to the external default Web browser.
        /// </summary>
        HelpBrowserType HelpBrowser { get; set; }

        bool ShowDotPrefixedVariables { get; set; }

        /// <summary>
        /// The frequency at which to check for updated news. Default is once per week.
        /// </summary>
        SurveyNewsPolicy SurveyNewsCheck { get; set; }

        /// <summary>
        /// The date/time when the last check for news occurred.
        /// </summary>
        DateTime SurveyNewsLastCheck { get; set; }

        string SurveyNewsFeedUrl { get; set; }

        string SurveyNewsIndexUrl { get; set; }
    }
}
