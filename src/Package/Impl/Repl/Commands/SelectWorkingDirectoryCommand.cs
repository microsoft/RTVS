// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal sealed class SelectWorkingDirectoryCommand : PackageCommand {
        private readonly IRInteractiveWorkflow _workflow;

        public SelectWorkingDirectoryCommand(IRInteractiveWorkflow workflow) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSelectWorkingDirectory) {
            _workflow = workflow;
        }

        protected override void SetStatus() {
            Supported = true;
            Enabled = _workflow.ActiveWindow != null && _workflow.RSession.IsHostRunning && !_workflow.RSession.IsRemote;
        }

        protected override void Handle() {
            IVsUIShell uiShell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            IntPtr dialogOwner;
            uiShell.GetDialogOwnerHwnd(out dialogOwner);

            var currentDirectory = RToolsSettings.Current.WorkingDirectory;
            var newDirectory = Dialogs.BrowseForDirectory(dialogOwner, currentDirectory, Resources.ChooseDirectory);
            if (!string.IsNullOrEmpty(newDirectory)) {
                _workflow.RSession.SetWorkingDirectoryAsync(newDirectory)
                    .SilenceException<RException>()
                    .DoNotWait();
            }
        }
    }
}
