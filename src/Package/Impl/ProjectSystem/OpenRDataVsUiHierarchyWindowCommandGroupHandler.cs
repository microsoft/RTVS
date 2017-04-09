// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [ExportCommandGroup("60481700-078B-11D1-AAF8-00A0C9055A90")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class OpenRDataVsUiHierarchyWindowCommandGroupHandler : OpenRDataCommandGroupHandler {
        [ImportingConstructor]
        public OpenRDataVsUiHierarchyWindowCommandGroupHandler(UnconfiguredProject unconfiguredProject, IRInteractiveWorkflowProvider workflowProvider)
            : base(unconfiguredProject, workflowProvider, (long)VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_DoubleClick, (long)VSConstants.VsUIHierarchyWindowCmdIds.UIHWCMDID_EnterKey) {}

        protected override async Task<bool> TryHandleCommandAsyncInternal(IProjectTree rDataNode) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            // Don't do anything for file preview
            var uiShellOpenDocument = VsAppShell.Current.GetService<IVsUIShellOpenDocument3>(typeof(SVsUIShellOpenDocument));
            if (uiShellOpenDocument != null && ((__VSNEWDOCUMENTSTATE) uiShellOpenDocument.NewDocumentState).HasFlag(__VSNEWDOCUMENTSTATE.NDS_Provisional)) {
                return true;
            }

            return await base.TryHandleCommandAsyncInternal(rDataNode);
        }
    }
}