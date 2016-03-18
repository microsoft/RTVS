// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.SurveyNews {
    [Export(typeof(ISurveyNewsBrowserLauncher))]
    internal class SurveyNewsBrowserLauncher : ISurveyNewsBrowserLauncher {
        public void Navigate(string url) {
            OpenVsBrowser(url);
        }

        public void NavigateOnIdle(string url) {
            if (!string.IsNullOrEmpty(url)) {
                IdleTimeAction.Create(() => {
                    Navigate(url);
                }, 100, typeof(SurveyNewsBrowserLauncher));
            }
        }

        private static void OpenExternalBrowser(string url) {
            var uri = new Uri(url);
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
            return;
        }

        private static void OpenVsBrowser(string url) {
            VsAppShell.Current.DispatchOnUIThread(() => {
                IVsWebBrowsingService web = VsAppShell.Current.GetGlobalService<IVsWebBrowsingService>(typeof(SVsWebBrowsingService));
                if (web == null) {
                    OpenExternalBrowser(url);
                    return;
                }

                try {
                    IVsWindowFrame frame;
                    ErrorHandler.ThrowOnFailure(web.Navigate(url, (uint)__VSWBNAVIGATEFLAGS.VSNWB_ForceNew, out frame));
                    frame.Show();
                } catch (COMException) {
                    OpenExternalBrowser(url);
                }
            });
        }
    }
}
