// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public static class VsContextMenu {
        public static void Show(Guid guid, int id, ICommandTarget commandTarget, Point screenPosition) {
            var shell = VsAppShell.Current.GetGlobalService<IVsUIShell>();
            var pts = new POINTS[1];
            pts[0].x = (short)screenPosition.X;
            pts[0].y = (short)screenPosition.Y;
            shell.ShowContextMenu(0, guid, id, pts, new CommandTargetToOleShim(null, commandTarget));
        }
    }
}
