// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal sealed class SetDirectoryToSourceCommand : PackageCommand {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly IActiveWpfTextViewTracker _viewTracker;

        public SetDirectoryToSourceCommand(IRInteractiveWorkflow interactiveWorkflow, IActiveWpfTextViewTracker viewTracker) :
            base(RGuidList.RCmdSetGuid, (int)RPackageCommandId.icmdSetDirectoryToSourceCommand) {
            _interactiveWorkflow = interactiveWorkflow;
            _viewTracker = viewTracker;
        }

        protected override void SetStatus() {
            Supported = true;
            Enabled = _viewTracker.LastActiveTextView != null && _interactiveWorkflow.RSession.IsHostRunning && !_interactiveWorkflow.RSession.IsRemote;
        }

        protected override void Handle() {
            var filePath = _viewTracker.LastActiveTextView?.GetFilePath();
            if (!string.IsNullOrEmpty(filePath)) {
                _interactiveWorkflow.RSession.SetWorkingDirectoryAsync(Path.GetDirectoryName(filePath))
                    .SilenceException<RException>()
                    .DoNotWait();
            }
        }
    }
}
