// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Commands {
    internal sealed class ShowVariableWindowCommand : PackageCommand {
        public ShowVariableWindowCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdShowVariableExplorerWindow) { }

        internal override void Handle() {
            ToolWindowUtilities.ShowWindowPane<VariableWindowPane>(0, true);
        }
    }
}
