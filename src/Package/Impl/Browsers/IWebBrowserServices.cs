// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.VisualStudio.R.Package.Browsers {
    internal interface IWebBrowserServices {
        void OpenBrowser(WebBrowserRole role, string url);
    }
}
