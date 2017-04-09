// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Drawing;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public static class AppShellExtensions {
        public static IntPtr GetDialogOwnerWindow(this ICoreShell shell) {
            IntPtr vsWindow;
            var uiShell = shell.GetService<IVsUIShell>(typeof(SVsUIShell));
            uiShell.GetDialogOwnerHwnd(out vsWindow);
            return vsWindow;
        }

        public static Font GetUiFont(this ICoreShell shell) {
            var fontSvc = shell.GetService<IUIHostLocale2>(typeof(SUIHostLocale));
            if (fontSvc != null) {
                var logFont = new UIDLGLOGFONT[1];
                int hr = fontSvc.GetDialogFont(logFont);
                if (hr == VSConstants.S_OK && logFont[0].lfFaceName != null) {
                    return IdeUtilities.FontFromUiDialogFont(logFont[0]);
                }
            }
            return null;
        }

        public static void PostCommand(this ICoreShell shell, Guid guid, int id) {
            var uiShell = shell.GetService<IVsUIShell>(typeof(SVsUIShell));
            var o = new object();
            uiShell.PostExecCommand(ref guid, (uint)id, 0, ref o);
        }
    }
}
