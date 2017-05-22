// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal sealed class VsPlatformServices : IPlatformServices {
        public VsPlatformServices() {
            var uiShell = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
            uiShell.GetDialogOwnerHwnd(out IntPtr handle);
            ApplicationWindowHandle = handle;
        }

        public IntPtr ApplicationWindowHandle { get; }
    }
}
