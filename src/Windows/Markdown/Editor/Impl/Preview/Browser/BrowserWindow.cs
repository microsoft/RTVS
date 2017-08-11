// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Controls;

namespace Microsoft.Markdown.Editor.Preview.Browser {
    internal sealed class BrowserWindow {
        private readonly WebBrowser _webBrowser;
        private readonly int _zoomFactor;
        private WebBrowserHostUIHandler _uiHandler;

        public BrowserWindow(WebBrowser webBrowser) {
            _webBrowser = webBrowser;
            _zoomFactor = GetZoomFactor();
            _uiHandler = new WebBrowserHostUIHandler(_webBrowser) { IsWebBrowserContextMenuEnabled = false };
        }

        public void Init() => Zoom(_zoomFactor);

        private void Zoom(int zoomFactor) {
            if (zoomFactor == 100) {
                return;
            }

            dynamic OLECMDEXECOPT_DODEFAULT = 0;
            dynamic OLECMDID_OPTICAL_ZOOM = 63;
            var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);

            var objComWebBrowser = fiComWebBrowser?.GetValue(_webBrowser);
            objComWebBrowser?.GetType().InvokeMember("ExecWB", BindingFlags.InvokeMethod, null, objComWebBrowser, new object[] {
                OLECMDID_OPTICAL_ZOOM,
                OLECMDEXECOPT_DODEFAULT,
                zoomFactor,
                IntPtr.Zero
            });
        }

        private static int GetZoomFactor() {
            using (var g = Graphics.FromHwnd(Process.GetCurrentProcess().MainWindowHandle)) {
                const int baseLine = 96;
                var dpi = g.DpiX;

                if (baseLine == (int)dpi) {
                    return 100;
                }

                // 150% scaling => 225
                // 250% scaling => 400
                double scale = dpi * ((dpi - baseLine) / baseLine + 1);
                return Convert.ToInt32(Math.Ceiling(scale / 25)) * 25; // round up to nearest 25
            }
        }
    }
}
