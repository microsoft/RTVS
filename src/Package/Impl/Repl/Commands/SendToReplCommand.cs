using System;
using System.Collections.Generic;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands
{
    public sealed class SendToReplCommand : ViewCommand
    {
        private ReplWindow _replWindow;

        public SendToReplCommand(ITextView textView, ITextBuffer textBuffer) :
            base(textView, new[] 
            {
                new CommandId(VSConstants.VsStd11, (int)VSConstants.VSStd11CmdID.ExecuteLineInInteractive),
                new CommandId(VSConstants.VsStd11, (int)VSConstants.VSStd11CmdID.ExecuteSelectionInInteractive)
            }, false)
        {
            ReplWindow.EnsureReplWindow().DoNotWait();
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
            List<string> selectedLines = new List<string>();
            ITextSnapshotLine line = null;

            if (selection.StreamSelectionSpan.Length == 0)
            {
                int position = selection.Start.Position;
                line = snapshot.GetLineFromPosition(position);
                selectedLines.Add(line.GetText());
            }
            else
            {
                VirtualSnapshotSpan span = TextView.Selection.StreamSelectionSpan;
                ITextSnapshot s = span.Snapshot;
                int start = span.Start.Position.Position;
                int end = span.End.Position.Position;

                int startLineNumber = s.GetLineNumberFromPosition(start);
                int endLineNumber = s.GetLineNumberFromPosition(end);

                if (end == s.GetLineFromLineNumber(endLineNumber).Start)
                {
                    endLineNumber--;
                }

                for (int i = startLineNumber; i <= endLineNumber; i++)
                {
                    line = s.GetLineFromLineNumber(i);
                    selectedLines.Add(line.GetText());
                }
            }

            ReplWindow replWindow = ReplWindow.Current;

            // Send text to REPL. In case when multiple lines are selected
            // send lines one by one as if user typed them manually.
            if (replWindow != null)
            {
                if(selectedLines.Count == 1)
                {
                    replWindow.ExecuteCode(selectedLines[0]);
                }
                else if(selectedLines.Count > 0)
                {
                    replWindow.SubmitAsync(selectedLines);
                }

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
            if (_replWindow != null)
            {
                _replWindow.Dispose();
                _replWindow = null;
            }

            base.Dispose(disposing);
        }
    }
}
