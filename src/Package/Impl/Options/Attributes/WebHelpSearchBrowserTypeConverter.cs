// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Support.Settings.Definitions;

namespace Microsoft.VisualStudio.R.Package.Options.Attributes {
    internal class WebHelpSearchBrowserTypeConverter : EnumTypeConverter<WebHelpSearchBrowserType> {
        public WebHelpSearchBrowserTypeConverter() :
            base(Resources.WebHelpSearchBrowserType_Internal, Resources.WebHelpSearchBrowserType_External) { }
    }
}
