// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Microsoft.Common.Core;
using Microsoft.R.Support.Settings;

namespace Microsoft.VisualStudio.R.Package.SurveyNews {
    [Export(typeof(ISurveyNewsOptions))]
    internal class SurveyNewsOptions : ISurveyNewsOptions {
        public SurveyNewsPolicy SurveyNewsCheck {
            get { return RToolsSettings.Current.SurveyNewsCheck; }
        }

        public DateTime SurveyNewsLastCheck {
            get { return RToolsSettings.Current.SurveyNewsLastCheck; }
            set { RToolsSettings.Current.SurveyNewsLastCheck = value; }
        }

        public string IndexUrl { get { return RToolsSettings.Current.SurveyNewsIndexUrl; } }

        public string FeedUrl { get { return RToolsSettings.Current.SurveyNewsFeedUrl; } }

        public string CannotConnectUrl {
            get {
                string assemblyPath = Assembly.GetExecutingAssembly().GetAssemblyPath();
                string url = Path.Combine(Path.GetDirectoryName(assemblyPath), "SurveyNews", "NoSurveyNewsFeed.mht");
                return url;
            }
        }
    }
}
