using System;
using System.Linq;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Selection {
    public sealed class SelectWordCommand: ViewCommand {
        public SelectWordCommand(ITextView textView, ITextBuffer textBuffer) :
            base(textView, typeof(VSConstants.VSStd2KCmdID).GUID, (int)VSConstants.VSStd2KCmdID.SELECTCURRENTWORD, needCheckout: false) {
        }

        public override CommandStatus Status(Guid group, int id) {
            return CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var caretPosition = TextView.Caret.Position.BufferPosition;
            ITextSnapshotLine line = null;
            try {
                line = TextView.TextBuffer.CurrentSnapshot.GetLineFromPosition(caretPosition);
            } catch (Exception) { }

            // Tokenize current line
            if(line != null) {
                var text = line.GetText();
                var t = new RTokenizer();
                var tokens = t.Tokenize(text);
                var token = tokens.FirstOrDefault(x => x.Start >= caretPosition && caretPosition < x.End);
                if(token != null) {
                    TextView.Selection.Select(new SnapshotSpan(TextView.TextBuffer.CurrentSnapshot, new Span(token.Start, token.Length)), isReversed: false);
                }
            }
            return CommandResult.Executed;
        }
    }
}
