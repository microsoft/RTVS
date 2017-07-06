// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InfoBar;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.InfoBar {
    [Export(typeof(IInfoBarProvider))]
    internal sealed class VsInfoBarProvider : IInfoBarProvider {
        private readonly IServiceContainer _services;

        [ImportingConstructor]
        public VsInfoBarProvider(ICoreShell coreShell) {
            _services = coreShell.Services;
        }

        public IInfoBar Create(Decorator host) {
            object infoBarHostObject = new InfoBarHostControl();
            host.Child = (UIElement)infoBarHostObject;
            return new VsInfoBar((IVsInfoBarHost)infoBarHostObject, _services);
        }
    }
}