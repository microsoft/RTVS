// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InfoBar;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.InfoBar {
    internal sealed class VsInfoBar : IInfoBar {
        private readonly Lazy<IVsInfoBarUIFactory> _factoryLazy;
        private readonly IVsInfoBarHost _infoBarHost;

        public VsInfoBar(IVsInfoBarHost infoBarHost, IServiceContainer services) {
            _factoryLazy = new Lazy<IVsInfoBarUIFactory>(() => services.GetService<IVsInfoBarUIFactory>(typeof(SVsInfoBarUIFactory)));
            _infoBarHost = infoBarHost;
        }

        public IDisposable Add(InfoBarItem item) {
            VsAppShell.Current.AssertIsOnMainThread();

            var infoBarModel = new InfoBarModel(item.Text,
                item.LinkButtons.Select(kvp => new InfoBarHyperlink(kvp.Key, kvp.Value)), 
                KnownMonikers.StatusInformation, 
                item.ShowCloseButton);
            var infoBar = _factoryLazy.Value.CreateInfoBar(infoBarModel);
            return new InfoBarEvents(infoBar, _infoBarHost);
        }
    }
}