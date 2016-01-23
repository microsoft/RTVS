using System;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class CopySelectedHistoryCommand : ViewCommand {
        private readonly IRHistory _history;
        private readonly IEditorOperations _editorOperations;

        public CopySelectedHistoryCommand(ITextView textView, IRHistoryProvider historyProvider, IEditorOperationsFactoryService editorOperationsService)
            : base(textView, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Copy, false) {
            _history = historyProvider.GetAssociatedRHistory(textView);
            _editorOperations = editorOperationsService.GetEditorOperations(textView);
        }

        public override CommandStatus Status(Guid guid, int id) {
            return ReplWindow.ReplWindowExists() ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            _history.CopySelection();
            return CommandResult.Executed;
        }
    }
}