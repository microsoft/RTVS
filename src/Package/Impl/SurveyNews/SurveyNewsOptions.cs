// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Settings;

namespace Microsoft.VisualStudio.R.Package.SurveyNews {
    [Export(typeof(ISurveyNewsOptions))]
    internal class SurveyNewsOptions : ISurveyNewsOptions {
        private readonly IRSettings _settings;

        [ImportingConstructor]
        public SurveyNewsOptions(ICoreShell coreShell) {
            _settings = coreShell.GetService<IRSettings>();
        }

        public SurveyNewsPolicy SurveyNewsCheck => _settings.SurveyNewsCheck;

        public DateTime SurveyNewsLastCheck {
            get { return _settings.SurveyNewsLastCheck; }
            set { _settings.SurveyNewsLastCheck = value; }
        }

        public string IndexUrl => _settings.SurveyNewsIndexUrl;
        public string FeedUrl => _settings.SurveyNewsFeedUrl;

        public string CannotConnectUrl {
            get {
                string assemblyPath = Assembly.GetExecutingAssembly().GetAssemblyPath();
                string url = Path.Combine(Path.GetDirectoryName(assemblyPath), "SurveyNews", "NoSurveyNewsFeed.mht");
                return url;
            }
        }
    }
}
