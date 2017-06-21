// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Common.Core.UI.Commands;
using static Microsoft.Markdown.Editor.Preview.NativeMethods;

namespace Microsoft.Markdown.Editor.Preview.Browser {
    internal sealed class WebBrowserHostUIHandler : IDocHostUIHandler {
        private const uint E_NOTIMPL = 0x80004001;
        private const uint S_OK = 0;
        private const uint S_FALSE = 1;

        public WebBrowserHostUIHandler(WebBrowser browser) {
            Browser = browser;
            browser.LoadCompleted += OnLoadCompleted;
            browser.Navigated += OnNavigated;
            IsWebBrowserContextMenuEnabled = true;
            Flags |= NativeMethods.HostUIFlags.ENABLE_REDIRECT_NOTIFICATION;
        }

        public WebBrowser Browser { get; private set; }
        public HostUIFlags Flags { get; set; }
        public bool IsWebBrowserContextMenuEnabled { get; set; }
        public bool ScriptErrorsSuppressed { get; set; }

        private void OnNavigated(object sender, NavigationEventArgs e) {
            SetSilent(Browser, ScriptErrorsSuppressed);
        }

        private void OnLoadCompleted(object sender, NavigationEventArgs e) {
            var doc = Browser.Document as ICustomDoc;
            doc?.SetUIHandler(this);
        }

        int IDocHostUIHandler.ShowContextMenu(int dwID, POINT pt, object pcmdtReserved, object pdispReserved)
            => IsWebBrowserContextMenuEnabled ? VSConstants.S_FALSE : VSConstants.S_OK;

        int IDocHostUIHandler.GetHostInfo(ref DOCHOSTUIINFO info) {
            info.dwFlags = (int)Flags;
            info.dwDoubleClick = 0;
            return VSConstants.S_OK;
        }

        int IDocHostUIHandler.ShowUI(int dwID, object activeObject, object commandTarget, object frame, object doc) => VSConstants.E_NOTIMPL;
        int IDocHostUIHandler.HideUI() => VSConstants.E_NOTIMPL;
        int IDocHostUIHandler.UpdateUI() => VSConstants.E_NOTIMPL;
        int IDocHostUIHandler.EnableModeless(bool fEnable) => VSConstants.E_NOTIMPL;
        int IDocHostUIHandler.OnDocWindowActivate(bool fActivate) => VSConstants.E_NOTIMPL;
        int IDocHostUIHandler.OnFrameWindowActivate(bool fActivate) => VSConstants.E_NOTIMPL;
        int IDocHostUIHandler.ResizeBorder(COMRECT rect, object doc, bool fFrameWindow) => VSConstants.E_NOTIMPL;
        int IDocHostUIHandler.TranslateAccelerator(ref System.Windows.Forms.Message msg, ref Guid group, int nCmdID) => VSConstants.S_FALSE;
        int IDocHostUIHandler.GetOptionKeyPath(string[] pbstrKey, int dw) => VSConstants.E_NOTIMPL;

        int IDocHostUIHandler.GetDropTarget(object pDropTarget, out object ppDropTarget) {
            ppDropTarget = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDocHostUIHandler.GetExternal(out object ppDispatch) {
            ppDispatch = Browser.ObjectForScripting;
            return VSConstants.S_OK;
        }

        int IDocHostUIHandler.TranslateUrl(int dwTranslate, string strURLIn, out string pstrURLOut) {
            pstrURLOut = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDocHostUIHandler.FilterDataObject(IDataObject pDO, out IDataObject ppDORet) {
            ppDORet = null;
            return VSConstants.E_NOTIMPL;
        }

        public static void SetSilent(WebBrowser browser, bool silent) {
            var sp = browser.Document as IOleServiceProvider;
            if (sp != null) {
                var IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                var IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out var webBrowser);
                webBrowser?.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
            }
        }
    }
}
