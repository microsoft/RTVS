// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Windows {
    internal sealed class GotoSolutionExplorerCommand : PackageCommand {
        private readonly ICoreShell _coreShell;

        public GotoSolutionExplorerCommand(ICoreShell coreShell) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdShowSolutionExplorer) {
            _coreShell = coreShell;
        }

        protected override void SetStatus() => Supported = Enabled = true;

        protected override void Handle()
            => _coreShell.PostCommand(typeof(VSConstants.VSStd97CmdID).GUID, (int)VSConstants.VSStd97CmdID.ProjectExplorer);
    }
}
