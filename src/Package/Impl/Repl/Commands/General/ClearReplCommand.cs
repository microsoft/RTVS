// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    public sealed class ClearReplCommand : ViewCommand {
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;

        public ClearReplCommand(ITextView textView, IRInteractiveWorkflowVisual interactiveWorkflow) :
            base(textView, new CommandId(RGuidList.RCmdSetGuid, (int)RPackageCommandId.icmdClearRepl), false) {
            _interactiveWorkflow = interactiveWorkflow;
        }

        public override CommandStatus Status(Guid group, int id) {
            var window = _interactiveWorkflow.ActiveWindow;
            if (window == null) {
                return CommandStatus.Supported;
            }
            return CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var window = _interactiveWorkflow.ActiveWindow;
            if (window == null) {
                return CommandResult.Disabled;
            }

            window.InteractiveWindow?.Operations?.ClearView();
            return CommandResult.Executed;
        }
    }
}
