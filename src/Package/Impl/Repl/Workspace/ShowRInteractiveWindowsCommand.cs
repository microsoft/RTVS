// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace {
    internal sealed class ShowRInteractiveWindowsCommand : PackageCommand {
        private readonly IRInteractiveWorkflowVisualProvider _interactiveWorkflowProvider;

        public ShowRInteractiveWindowsCommand(IRInteractiveWorkflowVisualProvider interactiveWorkflowProvider) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdShowReplWindow) {
            _interactiveWorkflowProvider = interactiveWorkflowProvider;
        }

        protected override void Handle() {
            var interactiveWorkflow = _interactiveWorkflowProvider.GetOrCreate();
            var window = interactiveWorkflow.ActiveWindow;
            if (window != null) {
                window.Container.Show(focus: true, immediate: false);
                 return;
            }

            interactiveWorkflow
                .GetOrCreateVisualComponentAsync()
                .ContinueOnRanToCompletion(w => w.Container.Show(focus: true, immediate: false));
        }
    }
}
