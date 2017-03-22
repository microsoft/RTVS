// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Packages.R;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Help {
    /// <summary>
    /// 'Help on ...' command that appears in the editor context menu.
    /// </summary>
    /// <remarks>
    /// Since command changes its name we have to make it package command
    /// since VS IDE no longer handles changing command names via OLE
    /// command target - it never calls IOlecommandTarget::QueryStatus
    /// with OLECMDTEXTF_NAME requesting changing names.
    /// </remarks>
    internal sealed class ShowHelpOnCurrentCommand : HelpOnCurrentCommandBase {
        public ShowHelpOnCurrentCommand(
            IRInteractiveWorkflow workflow,
            IActiveWpfTextViewTracker textViewTracker,
            IActiveRInteractiveWindowTracker activeReplTracker) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpOnCurrent,
                workflow, textViewTracker, activeReplTracker, Resources.OpenFunctionHelp) {
        }

        protected override void Handle(string item) {
            Workflow.RSession.ExecuteAsync(Invariant($"rtvs:::show_help({item.ToRStringLiteral()})"))
                .SilenceException<RException>()
                .DoNotWait();
        }
    }
}
