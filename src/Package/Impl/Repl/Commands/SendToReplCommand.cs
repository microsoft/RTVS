using System;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    public sealed class SendToReplCommand : ViewCommand {
        private ReplWindow _replWindow;
        private static readonly object _executedToEnd = new object();

        public SendToReplCommand(ITextView textView, ITextBuffer textBuffer) :
            base(textView, new[]
            {
                new CommandId(VSConstants.VsStd11, (int)VSConstants.VSStd11CmdID.ExecuteLineInInteractive),
                new CommandId(VSConstants.VsStd11, (int)VSConstants.VSStd11CmdID.ExecuteSelectionInInteractive)
            }, false) {
            ReplWindow.EnsureReplWindow().DoNotWait();
            _replWindow = ReplWindow.Current;
        }

        public override CommandStatus Status(Guid group, int id) {
            return (TextView.Selection.Mode == TextSelectionMode.Stream) ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            ITextSelection selection = TextView.Selection;
            ITextSnapshot snapshot = TextView.TextBuffer.CurrentSnapshot;
            ReplWindow replWindow = ReplWindow.Current;
            int position = selection.Start.Position;
            ITextSnapshotLine line = snapshot.GetLineFromPosition(position);

            if (replWindow == null)
            {
                return CommandResult.Disabled;
            }

            string text;
            if (selection.StreamSelectionSpan.Length == 0)
            {
                text = line.GetText();
            }
            else
            {
                text = TextView.Selection.StreamSelectionSpan.GetText();
                line = TextView.Selection.End.Position.GetContainingLine();
            }

            ReplWindow.Show();
            replWindow.EnqueueCode(text, addNewLine: true);

            var targetLine = line;
            while (targetLine.LineNumber < snapshot.LineCount - 1)
            {
                targetLine = snapshot.GetLineFromLineNumber(targetLine.LineNumber + 1);
                // skip over blank lines, unless it's the last line, in which case we want to land on it no matter what
                if (!String.IsNullOrWhiteSpace(targetLine.GetText()) || targetLine.LineNumber == snapshot.LineCount - 1)
                {
                    TextView.Caret.MoveTo(new SnapshotPoint(snapshot, targetLine.Start));
                    TextView.Caret.EnsureVisible();
                    break;
                }
            }

            // Take focus back if REPL window has stolen it
            if (!TextView.HasAggregateFocus)
            {
                IVsEditorAdaptersFactoryService adapterService = VsAppShell.Current.ExportProvider.GetExportedValue<IVsEditorAdaptersFactoryService>();
                IVsTextView tv = adapterService.GetViewAdapter(TextView);
                tv.SendExplicitFocus();
            }
            
            return CommandResult.Executed;
        }

        protected override void Dispose(bool disposing) {
            if (_replWindow != null) {
                _replWindow.Dispose();
                _replWindow = null;
            }

            base.Dispose(disposing);
        }
    }
}
