// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal sealed class SetDirectoryToProjectCommand : PackageCommand {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly IProjectSystemServices _pss;

        public SetDirectoryToProjectCommand(IRInteractiveWorkflow interactiveWorkflow, IProjectSystemServices pss) :
            base(RGuidList.RCmdSetGuid, (int)RPackageCommandId.icmdSetDirectoryToProjectCommand) {
            _interactiveWorkflow = interactiveWorkflow;
            _pss = pss;
        }

        protected override void SetStatus() {
            Supported = true;
            Enabled = _pss.GetActiveProject() != null;
        }

        protected override void Handle() {
            string projectFile = null;
            var project = _pss.GetSelectedProject<IVsProject>();
            if (VSConstants.S_OK == project?.GetMkDocument((uint)VSConstants.VSITEMID.Root, out projectFile)) {
                if (!string.IsNullOrEmpty(projectFile)) {
                    _interactiveWorkflow.RSession.SetWorkingDirectoryAsync(Path.GetDirectoryName(projectFile))
                        .SilenceException<RException>()
                        .SilenceException<MessageTransportException>()
                        .DoNotWait();
                }
            }
        }
    }
}
