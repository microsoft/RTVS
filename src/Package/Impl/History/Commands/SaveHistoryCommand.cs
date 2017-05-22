// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text.Editor;
using CommandStatus = Microsoft.Common.Core.UI.Commands.CommandStatus;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class SaveHistoryCommand : ViewCommand {
        private readonly IUIService _ui;
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;
        private readonly IRSettings _settings;
        private readonly IRHistory _history;

        public SaveHistoryCommand(ITextView textView, IRHistoryProvider historyProvider, IRInteractiveWorkflowVisual interactiveWorkflow)
            : base(textView, RGuidList.RCmdSetGuid, RPackageCommandId.icmdSaveHistory, false) {
            _ui = interactiveWorkflow.Shell.UI();
            _interactiveWorkflow = interactiveWorkflow;
            _settings = _interactiveWorkflow.Shell.GetService<IRSettings>();
            _history = historyProvider.GetAssociatedRHistory(textView);
        }

        public override CommandStatus Status(Guid guid, int id) {
            return _interactiveWorkflow.ActiveWindow != null && _history.HasEntries
                ? CommandStatus.SupportedAndEnabled 
                : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var initialPath = _settings.WorkingDirectory != null ? PathHelper.EnsureTrailingSlash(_settings.WorkingDirectory) : null;
            var file = _ui.FileDialog.ShowSaveFileDialog(Resources.HistoryFileFilter, initialPath, Resources.SaveHistoryAsTitle);
            if (file != null) {
                _history.TrySaveToFile(file);
            }
            return CommandResult.Executed;
        }
    }
}