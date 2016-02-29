// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace {
    internal sealed class ResetReplCommand : PackageCommand {
        public ResetReplCommand() : 
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdResetRepl) {
        }

        internal override void SetStatus() {
            Visible = false;
            Enabled = false;

            if (ReplWindow.Current.IsActive) {
                IVsInteractiveWindow window = ReplWindow.Current.GetInteractiveWindow();
                if (window != null && window.InteractiveWindow != null) {
                    Visible = true;
                    Enabled = true;
                }
            }
        }

        internal override void Handle() {
            if (ReplWindow.Current.IsActive) {
                IVsInteractiveWindow window = ReplWindow.Current.GetInteractiveWindow();
                if (window != null && window.InteractiveWindow != null) {
                    window.InteractiveWindow.Operations.ResetAsync().DoNotWait();
                }
            }
        }
    }
}
