// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.OS;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Windows.Core.OS;

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
            VsAppShell.Current.DispatchOnUIThread(() => {
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
                WebBrowserService.Navigate(url, (uint)__VSWBNAVIGATEFLAGS.VSNWB_ForceNew, out frame);
            } else {
                var flags = (uint)(__VSCREATEWEBBROWSER.VSCWB_AutoShow | 
                                   __VSCREATEWEBBROWSER.VSCWB_ForceNew | 
                                   __VSCREATEWEBBROWSER.VSCWB_StartCustom |
                                   __VSCREATEWEBBROWSER.VSCWB_ReuseExisting);
                var title = GetRoleWindowTitle(role);
                WebBrowserService.CreateWebBrowser(flags, guid, title, url, null, out wb, out frame);
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
                    return Settings.WebHelpSearchBrowserType == BrowserType.External;
                case WebBrowserRole.Shiny:
                    return Settings.HtmlBrowserType == BrowserType.External;
                case WebBrowserRole.Markdown:
                    return Settings.MarkdownBrowserType == BrowserType.External;
            }
            return false;
        }
    }
}
