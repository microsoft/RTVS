// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudioTools;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal sealed class SelectWorkingDirectoryCommand : PackageCommand {
        private readonly IRInteractiveWorkflowVisual _workflow;

        public SelectWorkingDirectoryCommand(IRInteractiveWorkflowVisual workflow) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSelectWorkingDirectory) {
            _workflow = workflow;
        }

        protected override void SetStatus() {
            Supported = true;
            Enabled = _workflow.ActiveWindow != null && _workflow.RSession.IsHostRunning && !_workflow.RSession.IsRemote;
        }

        protected override void Handle() {
            var ps = _workflow.Shell.GetService<IPlatformServices>();
            var settings = _workflow.Shell.GetService<IRSettings>();
            var currentDirectory = settings.WorkingDirectory;
            var newDirectory = Dialogs.BrowseForDirectory(ps.ApplicationWindowHandle, currentDirectory, Resources.ChooseDirectory);
            if (!string.IsNullOrEmpty(newDirectory)) {
                _workflow.RSession.SetWorkingDirectoryAsync(newDirectory)
                    .SilenceException<RException>()
                    .DoNotWait();
            }
        }
    }
}
