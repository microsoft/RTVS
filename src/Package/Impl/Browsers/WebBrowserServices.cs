// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Browsers {
    [Export(typeof(IWebBrowserServices))]
    internal class WebBrowserServices : IWebBrowserServices {
        private readonly ICoreShell _coreShell;
        private readonly IProcessServices _ps;
        private readonly IVsWebBrowsingService _wbs;
        private readonly IRToolsSettings _settings;

        [ImportingConstructor]
        public WebBrowserServices(ICoreShell coreShell) { 
            _coreShell = coreShell;
            _wbs = _coreShell.GetService<IVsWebBrowsingService>(typeof(SVsWebBrowsingService));
            _ps = _coreShell.Process();
            _settings = _coreShell.GetService<IRToolsSettings>();
        }

        #region IWebBrowserServices
        public void OpenBrowser(WebBrowserRole role, string url, bool onIdle = false) {
            if(role == WebBrowserRole.External || IsExternal(role)) {
                _ps.Start(url);
            } else {
                if (onIdle) {
                    NavigateOnIdle(role, url);
                } else {
                    OpenVsBrowser(role, url);
                }
            }
        }
        #endregion

        private void OpenVsBrowser(WebBrowserRole role, string url) {
            VsAppShell.Current.MainThread().Post(() => {
                DoOpenVsBrowser(role, url);
            });
        }

        private void NavigateOnIdle(WebBrowserRole role, string url) {
            if (!string.IsNullOrEmpty(url)) {
                IdleTimeAction.Create(() => {
                    OpenVsBrowser(role, url);
                }, 100, typeof(WebBrowserServices), VsAppShell.Current);
            }
        }

        private void DoOpenVsBrowser(WebBrowserRole role, string url) {
            IVsWindowFrame frame;
            IVsWebBrowser wb;
            var guid = GetRoleGuid(role);
            if(guid == Guid.Empty) {
                _wbs.Navigate(url, (uint)__VSWBNAVIGATEFLAGS.VSNWB_ForceNew, out frame);
            } else {
                var flags = (uint)(__VSCREATEWEBBROWSER.VSCWB_AutoShow | 
                                   __VSCREATEWEBBROWSER.VSCWB_ForceNew | 
                                   __VSCREATEWEBBROWSER.VSCWB_StartCustom |
                                   __VSCREATEWEBBROWSER.VSCWB_ReuseExisting);
                var title = GetRoleWindowTitle(role);
                _wbs.CreateWebBrowser(flags, guid, title, url, null, out wb, out frame);
            }
        }

        private Guid GetRoleGuid(WebBrowserRole role) {
            switch(role) {
                case WebBrowserRole.Help:
                    return RGuidList.WebHelpWindowGuid;
                case WebBrowserRole.Shiny:
                    return RGuidList.ShinyWindowGuid;
                case WebBrowserRole.Markdown:
                    return RGuidList.MarkdownWindowGuid;
            }
            return Guid.Empty;
        }

        private string GetRoleWindowTitle(WebBrowserRole role) {
            switch (role) {
                case WebBrowserRole.Help:
                    return Resources.WebHelpWindowTitle;
                case WebBrowserRole.News:
                    return Resources.NewsWindowTitle;
                case WebBrowserRole.Shiny:
                    return Resources.ShinyWindowTitle;
                case WebBrowserRole.Markdown:
                    return Resources.MarkdownWindowTitle;
            }
            return null;
        }

        private bool IsExternal(WebBrowserRole role) {
            switch (role) {
                case WebBrowserRole.Help:
                    return _settings.WebHelpSearchBrowserType == BrowserType.External;
                case WebBrowserRole.Shiny:
                    return _settings.HtmlBrowserType == BrowserType.External;
                case WebBrowserRole.Markdown:
                    return _settings.MarkdownBrowserType == BrowserType.External;
            }
            return false;
        }
    }
}
