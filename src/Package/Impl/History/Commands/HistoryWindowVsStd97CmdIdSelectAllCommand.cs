// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class HistoryWindowVsStd97CmdIdSelectAllCommand : ViewCommand {
        private static readonly CommandId[] SelectAllCommandIds = {
            new CommandId(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.SelectAll),
            new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.SELECTALL)
        };

        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;
        private readonly IRHistory _history;

        public HistoryWindowVsStd97CmdIdSelectAllCommand(ITextView textView, IRHistoryProvider historyProvider, IRInteractiveWorkflowVisual interactiveWorkflow)
            : base(textView, SelectAllCommandIds, false) {
            _interactiveWorkflow = interactiveWorkflow;
            _history = historyProvider.GetAssociatedRHistory(textView);
        }

        public override CommandStatus Status(Guid guid, int id) {
            return _interactiveWorkflow.ActiveWindow != null && _history.HasEntries
                ? CommandStatus.SupportedAndEnabled
                : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            _history.SelectAllEntries();
            return CommandResult.Executed;
        }
    }
}