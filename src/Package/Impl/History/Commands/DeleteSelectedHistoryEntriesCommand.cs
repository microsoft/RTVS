// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controller.Commands;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class DeleteSelectedHistoryEntriesCommand : ViewCommand {
        private readonly IUIService _ui;
        private readonly IRHistory _history;
        private readonly IRInteractiveWorkflow _interactiveWorkflow;

        public DeleteSelectedHistoryEntriesCommand(ITextView textView, IRHistoryProvider historyProvider, IRInteractiveWorkflow interactiveWorkflow, IUIService ui)
            : base(textView, RGuidList.RCmdSetGuid, RPackageCommandId.icmdDeleteSelectedHistoryEntries, false) {
            _interactiveWorkflow = interactiveWorkflow;
            _history = historyProvider.GetAssociatedRHistory(textView);
            _ui = ui;
        }

        public override CommandStatus Status(Guid guid, int id) {
            return _interactiveWorkflow.ActiveWindow != null && _history.HasSelectedEntries
                ? CommandStatus.SupportedAndEnabled
                : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (_ui.ShowMessage(Resources.DeleteSelectedHistoryEntries, MessageButtons.YesNo) == MessageButtons.Yes) {
                _history.DeleteSelectedHistoryEntries();
            }
            return CommandResult.Executed;
        }
    }
}