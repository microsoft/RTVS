// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class ToggleMultilineHistorySelectionCommand : ViewCommand {
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;
        private readonly IRSettings _settings;
        private readonly IRHistory _history;

        public ToggleMultilineHistorySelectionCommand(ITextView textView, IRHistoryProvider historyProvider, IRInteractiveWorkflowVisual interactiveWorkflow)
            : base(textView, RGuidList.RCmdSetGuid, RPackageCommandId.icmdToggleMultilineSelection, false) {
            _history = historyProvider.GetAssociatedRHistory(textView);
            _settings = interactiveWorkflow.Shell.GetService<IRSettings>();
            _interactiveWorkflow = interactiveWorkflow;
        }

        public override CommandStatus Status(Guid guid, int id) {
            return _interactiveWorkflow.ActiveWindow != null
                ? _settings.MultilineHistorySelection 
                    ? CommandStatus.Latched | CommandStatus.SupportedAndEnabled
                    : CommandStatus.SupportedAndEnabled
                : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid guid, int id, object inputArg, ref object outputArg) {
            _settings.MultilineHistorySelection = !_settings.MultilineHistorySelection;
            _history.IsMultiline = _settings.MultilineHistorySelection;
            return CommandResult.Executed;
        }
    }
}