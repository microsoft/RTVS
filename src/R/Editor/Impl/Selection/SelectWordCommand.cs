using System;
using System.Linq;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.R.Components.Controller;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Selection {
    public sealed class SelectWordCommand : ViewCommand {
        public SelectWordCommand(ITextView textView, ITextBuffer textBuffer) :
            base(textView, typeof(VSConstants.VSStd2KCmdID).GUID, (int)VSConstants.VSStd2KCmdID.SELECTCURRENTWORD, needCheckout: false) {
        }

        public override CommandStatus Status(Guid group, int id) {
            return CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (!TextView.Caret.InVirtualSpace) {
                int caretPosition = TextView.Caret.Position.BufferPosition.Position;
                SnapshotPoint? rPosition = TextView.MapDownToR(caretPosition);
                if (rPosition.HasValue) {
                    int rCaretPosition = rPosition.Value.Position;
                    ITextSnapshotLine line = rPosition.Value.Snapshot.GetLineFromPosition(rCaretPosition);
                    // Tokenize current line
                    if (line != null) {
                        Span? spanToSelect = null;
                        var text = line.GetText();
                        var tokenizer = new RTokenizer();
                        var tokens = tokenizer.Tokenize(text);
                        var positionInLine = rCaretPosition - line.Start;
                        var token = tokens.FirstOrDefault(t => t.Contains(positionInLine));
                        if (token != null) {
                            if (token.TokenType == RTokenType.String) {
                                // Select word inside string
                                spanToSelect = GetWordSpan(text, line.Start, positionInLine);
                            } else {
                                spanToSelect = new Span(token.Start + line.Start, token.Length);
                            }
                        }
                        if (spanToSelect.HasValue && spanToSelect.Value.Length > 0) {
                            NormalizedSnapshotSpanCollection spans = TextView.BufferGraph.MapUpToBuffer(
                                new SnapshotSpan(rPosition.Value.Snapshot, spanToSelect.Value),
                                SpanTrackingMode.EdgePositive, TextView.TextBuffer);
                            if (spans.Count == 1) {
                                TextView.Selection.Select(new SnapshotSpan(TextView.TextBuffer.CurrentSnapshot, spans[0]), isReversed: false);
                                return CommandResult.Executed;
                            }
                        }
                    }
                }
            }
            return CommandResult.NotSupported;
        }

        private static Span GetWordSpan(string text, int lineStart, int position) {
            int start = position;
            int end = position;
            for (start = position; start >= 0; start--) {
                if (IsSeparator(text[start])) {
                    start++;
                    break;
                }
            }
            for (end = position + 1; end < text.Length; end++) {
                if (IsSeparator(text[end])) {
                    break;
                }
            }
            return Span.FromBounds(start + lineStart, end + lineStart);
        }

        private static bool IsSeparator(char ch) {
            return char.IsWhiteSpace(ch) || ch == '\'' || ch == '\"' || ch == '\\';
        }
    }
}
