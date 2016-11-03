// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal sealed class ShowMainPlotWindowCommand : PackageCommand {
        private readonly IRPlotManager _plotManager;

        public ShowMainPlotWindowCommand(IRInteractiveWorkflow workflow) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdShowPlotWindow) {
            _plotManager = workflow.Plots;
        }

        protected override void SetStatus() {
            Enabled = true;
        }

        protected override void Handle() {
            var component = _plotManager.GetOrCreateMainPlotVisualComponent();
            component?.Container.Show(true, false);
        }
    }
}
