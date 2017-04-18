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
using PathHelper = Microsoft.VisualStudio.ProjectSystem.PathHelper;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class LoadHistoryCommand : ViewCommand {
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;
        private readonly IRHistory _history;

        public LoadHistoryCommand(ITextView textView, IRHistoryProvider historyProvider, IRInteractiveWorkflowVisual interactiveWorkflow)
            : base(textView, RGuidList.RCmdSetGuid, RPackageCommandId.icmdLoadHistory, false) {
            _interactiveWorkflow = interactiveWorkflow;
            _history = historyProvider.GetAssociatedRHistory(textView);
        }

        public override CommandStatus Status(Guid guid, int id) {
            return _interactiveWorkflow.ActiveWindow != null ?
                CommandStatus.SupportedAndEnabled :
                CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var settings = _interactiveWorkflow.Shell.GetService<IRSettings>();
            var initialPath = settings.WorkingDirectory != null ? PathHelper.EnsureTrailingSlash(settings.WorkingDirectory) : null;
            var file = _interactiveWorkflow.Shell.FileDialog().ShowOpenFileDialog(Resources.HistoryFileFilter, initialPath, Resources.LoadHistoryTitle);
            if (file != null) {
                _history.TryLoadFromFile(file);
            }
            return CommandResult.Executed;
        }
    }
}
