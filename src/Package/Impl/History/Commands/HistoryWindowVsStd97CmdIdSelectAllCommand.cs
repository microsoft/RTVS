using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class HistoryWindowVsStd97CmdIdSelectAllCommand : ViewCommand {
        private static readonly CommandId[] SelectAllCommandIds = {
            new CommandId(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.SelectAll),
            new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.SELECTALL)
        };

        private readonly IRHistory _history;

        public HistoryWindowVsStd97CmdIdSelectAllCommand(ITextView textView, IRHistoryProvider historyProvider)
            : base(textView, SelectAllCommandIds, false) {
            _history = historyProvider.GetAssociatedRHistory(textView);
        }

        public override CommandStatus Status(Guid guid, int id) {
            return ReplWindow.ReplWindowExists() && _history.HasEntries
                ? CommandStatus.SupportedAndEnabled
                : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            _history.SelectAllEntries();
            return CommandResult.Executed;
        }
    }
}