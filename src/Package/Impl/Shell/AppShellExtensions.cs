// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public static class AppShellExtensions {
        public static IntPtr GetDialogOwnerWindow(this IApplicationShell appShell) {
            IntPtr vsWindow;
            var uiShell = appShell.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            uiShell.GetDialogOwnerHwnd(out vsWindow);
            return vsWindow;
        }
    }
}
