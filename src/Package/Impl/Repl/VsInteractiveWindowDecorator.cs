// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl {
    internal sealed class VsInteractiveWindowDecorator : IVsInteractiveWindowDecorator {
        public void GetToolbarInfo(out Guid cmdSet, out uint id) {
            cmdSet = RGuidList.RCmdSetGuid;
            id = RPackageCommandId.replWindowToolBarId;
        }
    }
}
