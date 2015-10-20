using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    public sealed class SendToReplCommand : ViewCommand {
        private ReplWindow _replWindow;
        private static object _executedToEnd = new object();

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
            else if (TextView.Properties.ContainsProperty(_executedToEnd))
            {
                return CommandResult.Executed;
            }

            string text;
            bool addNewLine = false;
            if (selection.StreamSelectionSpan.Length == 0)
            {
                text = line.GetText();
                addNewLine = true;
            }
            else
            {
                text = TextView.Selection.StreamSelectionSpan.GetText();
                line = TextView.Selection.End.Position.GetContainingLine();
            }

            ReplWindow.Show();
            replWindow.EnqueueCode(text, addNewLine);

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

            if (targetLine == line && 
                selection.StreamSelectionSpan.Length == 0)
            {
                // we're at the end of the buffer, we don't want to continue executing
                TextView.Caret.PositionChanged += Caret_PositionChanged;
                TextView.Properties[_executedToEnd] = this;
            }

            // Take focus back if REPL window has stolen it
            if (!TextView.HasAggregateFocus)
            {
                IVsEditorAdaptersFactoryService adapterService = EditorShell.Current.ExportProvider.GetExportedValue<IVsEditorAdaptersFactoryService>();
                IVsTextView tv = adapterService.GetViewAdapter(TextView);
                tv.SendExplicitFocus();
            }
            
            return CommandResult.Executed;
        }

        private void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            TextView.Properties.RemoveProperty(_executedToEnd);
            TextView.Caret.PositionChanged -= Caret_PositionChanged;
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
