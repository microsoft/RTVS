// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class CopySelectedHistoryCommand : ViewCommand {
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;
        private readonly IRHistory _history;

        public CopySelectedHistoryCommand(ITextView textView, IRHistoryProvider historyProvider, IRInteractiveWorkflowVisual interactiveWorkflow)
            : base(textView, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Copy, false) {
            _interactiveWorkflow = interactiveWorkflow;
            _history = historyProvider.GetAssociatedRHistory(textView);
        }

        public override CommandStatus Status(Guid guid, int id) {
            return _interactiveWorkflow.ActiveWindow != null ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            _history.CopySelection();
            return CommandResult.Executed;
        }
    }
}