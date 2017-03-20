// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.VisualStudio.R.Package.ToolWindows;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal sealed class HideAllPlotWindowsCommand : PackageCommand {
        private readonly IUIService _ui;
        private readonly IVsUIShell4 _vsshell;

        public HideAllPlotWindowsCommand(ICoreShell shell) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdPlotWindowsHideAll) {
            _ui = shell.UI();
            _vsshell = shell.GetService<IVsUIShell4>(typeof(SVsUIShell));
        }

        protected override void SetStatus() {
            Enabled = true;
        }

        protected override void Handle() {
            // Note that we go through the VS windows rather than the visual components.
            // Visual components only exist for tool windows that are initialized.
            // Tool windows that have a visible tab but haven't clicked on yet are not initialized.
            try {
                var frames = _vsshell.EnumerateWindows(
                    __WindowFrameTypeFlags.WINDOWFRAMETYPE_Tool |
                    __WindowFrameTypeFlags.WINDOWFRAMETYPE_AllStates,
                    typeof(PlotDeviceWindowPane).GUID);

                foreach (var frame in frames) {
                    if (frame.IsVisible() == VSConstants.S_OK) {
                        frame.CloseFrame((int)__FRAMECLOSE.FRAMECLOSE_NoSave);
                    }
                }
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                _ui.ShowErrorMessage(ex.Message);
            }
        }
    }
}
