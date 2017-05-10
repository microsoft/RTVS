// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.InfoBar {
    internal sealed class InfoBarEvents : IVsInfoBarUIEvents, IDisposable {
        private readonly IVsInfoBarUIElement _infoBar;
        private readonly IVsInfoBarHost _infoBarHost;
        private readonly uint _cookie;

        public InfoBarEvents(IVsInfoBarUIElement infoBar, IVsInfoBarHost infoBarHost) {
            _infoBar = infoBar;
            _infoBarHost = infoBarHost;
            _infoBar.Advise(this, out _cookie);
            _infoBarHost.AddInfoBar(infoBar);
        }

        public void Dispose() {
            VsAppShell.Current.AssertIsOnMainThread();

            _infoBarHost.RemoveInfoBar(_infoBar);
            _infoBar.Unadvise(_cookie);
            _infoBar.Close();
        }

        void IVsInfoBarUIEvents.OnClosed(IVsInfoBarUIElement infoBarUiElement) {
            Dispose();
        }

        void IVsInfoBarUIEvents.OnActionItemClicked(IVsInfoBarUIElement infoBarUiElement, IVsInfoBarActionItem actionItem) {
            ((Action)actionItem.ActionContext)();
        }
    }
}