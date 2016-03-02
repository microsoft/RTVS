// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class SaveHistoryCommand : ViewCommand {
        private readonly IRHistory _history;

        public SaveHistoryCommand(ITextView textView, IRHistoryProvider historyProvider)
            : base(textView, RGuidList.RCmdSetGuid, RPackageCommandId.icmdSaveHistory, false) {
            _history = historyProvider.GetAssociatedRHistory(textView);
        }

        public override CommandStatus Status(Guid guid, int id) {
            return ReplWindow.ReplWindowExists && _history.HasEntries
                ? CommandStatus.SupportedAndEnabled 
                : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var initialPath = RToolsSettings.Current.WorkingDirectory != null ? PathHelper.EnsureTrailingSlash(RToolsSettings.Current.WorkingDirectory) : null;
            var file = VsAppShell.Current.BrowseForFileSave(IntPtr.Zero, Resources.HistoryFileFilter, initialPath, Resources.SaveHistoryAsTitle);
            if (file != null) {
                _history.TrySaveToFile(file);
            }

            return CommandResult.Executed;
        }
    }
}