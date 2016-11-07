// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Common.Core.Enums;
using Microsoft.R.Components.Settings;

namespace Microsoft.R.Support.Settings {
    public interface IRToolsSettings : IRSettings {

        YesNoAsk LoadRDataOnProjectLoad { get; set; }
        YesNoAsk SaveRDataOnProjectUnload { get; set; }

        /// <summary>
        /// Most recently used directories in REPL
        /// </summary>
        IEnumerable<string> WorkingDirectoryList { get; set; }

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

        /// <summary>
        /// Site to search in 'Search Web for'... commands
        /// </summary>
        string WebHelpSearchString { get; set; }

        BrowserType WebHelpSearchBrowserType { get; set; }
        BrowserType HtmlBrowserType { get; set; }
        BrowserType MarkdownBrowserType { get; set; }
    }
}
