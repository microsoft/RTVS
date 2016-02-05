using System;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class DeleteSelectedHistoryEntriesCommand : ViewCommand {
        private readonly IRHistory _history;

        public DeleteSelectedHistoryEntriesCommand(ITextView textView, IRHistoryProvider historyProvider)
            : base(textView, RGuidList.RCmdSetGuid, RPackageCommandId.icmdDeleteSelectedHistoryEntries, false) {
            _history = historyProvider.GetAssociatedRHistory(textView);
        }

        public override CommandStatus Status(Guid guid, int id) {
            return ReplWindow.ReplWindowExists && _history.HasSelectedEntries
                ? CommandStatus.SupportedAndEnabled
                : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (VsAppShell.Current.ShowMessage(Resources.DeleteSelectedHistoryEntries, MessageButtons.YesNo) == MessageButtons.Yes) {
                _history.DeleteSelectedHistoryEntries();
            }

            return CommandResult.Executed;
        }
    }
}