// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Components.Settings;

namespace Microsoft.VisualStudio.R.Package.SurveyNews {
    internal interface ISurveyNewsOptions {
        SurveyNewsPolicy SurveyNewsCheck { get; }
        DateTime SurveyNewsLastCheck { get; set; }
        string IndexUrl { get; }
        string FeedUrl { get; }
        string CannotConnectUrl { get; }
    }
}
