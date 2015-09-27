using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands
{
    public sealed class SendToReplCommand : ViewCommand
    {
        private ReplWindow _replWindow;

        public SendToReplCommand(ITextView textView, ITextBuffer textBuffer) :
            base(textView, new CommandId[] {
                new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSendToRepl)
            }, false)
        {
            ReplWindow.EnsureReplWindow();
            _replWindow = ReplWindow.Current;
        }

        public override CommandStatus Status(Guid group, int id)
        {
            return (TextView.Selection.Mode == TextSelectionMode.Stream) ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg)
        {
            ITextSelection selection = TextView.Selection;
            ITextSnapshot snapshot = TextView.TextBuffer.CurrentSnapshot;
            string selectedText;
            ITextSnapshotLine line = null;

            if (selection.StreamSelectionSpan.Length == 0)
            {
                int position = selection.Start.Position;
                line = snapshot.GetLineFromPosition(position);
                selectedText = line.GetText();
            }
            else
            {
                selectedText = TextView.Selection.StreamSelectionSpan.GetText();
            }

            ReplWindow replWindow = ReplWindow.Current;
            // Send text to REPL
            if (replWindow != null)
            {
                replWindow.ExecuteCode(selectedText);

                if (line != null && line.LineNumber < snapshot.LineCount - 1)
                {
                    ITextSnapshotLine nextLine = snapshot.GetLineFromLineNumber(line.LineNumber + 1);
                    TextView.Caret.MoveTo(new SnapshotPoint(snapshot, nextLine.Start));
                    TextView.Caret.EnsureVisible();
                }
            }

            return CommandResult.Executed;
        }

        protected override void Dispose(bool disposing)
        {
            if(_replWindow != null)
            {
                _replWindow.Dispose();
                _replWindow = null;
            }

            base.Dispose(disposing);
        }
    }
}
