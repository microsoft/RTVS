// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.OS;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Browsers {
    [Export(typeof(IWebBrowserServices))]
    internal class WebBrowserServices : IWebBrowserServices {
        private readonly IProcessServices _ps;

        private readonly IVsWebBrowsingService _wbs;
        private IVsWebBrowsingService WebBrowserService => _wbs ?? VsAppShell.Current.GetGlobalService<IVsWebBrowsingService>(typeof(SVsWebBrowsingService));

        private readonly IRToolsSettings _settings;
        private IRToolsSettings Settings => _settings ?? RToolsSettings.Current;

        public WebBrowserServices() : 
            this(null, new ProcessServices(), null) {
        }

        public WebBrowserServices(IVsWebBrowsingService wbs, IProcessServices ps, IRToolsSettings settings) {
            _wbs = wbs;
            _ps = ps;
            _settings = settings;
        }

        #region IWebBrowserServices
        public void OpenBrowser(WebBrowserRole role, string url) => _ps.Start(url);
        #endregion
    }
}
