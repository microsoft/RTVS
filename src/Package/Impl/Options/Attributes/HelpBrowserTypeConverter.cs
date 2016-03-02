// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Support.Settings.Definitions;

namespace Microsoft.VisualStudio.R.Package.Options.Attributes {
    internal class HelpBrowserTypeConverter : EnumTypeConverter<HelpBrowserType> {
        public HelpBrowserTypeConverter() : base(Resources.HelpBrowser_Automatic, Resources.HelpBrowser_External) {}
    }
}
