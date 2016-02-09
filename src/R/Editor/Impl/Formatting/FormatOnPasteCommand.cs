using System;
using System.Windows;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.Formatting.Data;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    public class FormatOnPasteCommand : EditingCommand {
        internal IClipboardDataProvider ClipboardDataProvider { get; set; }

        public FormatOnPasteCommand(ITextView textView, ITextBuffer textBuffer) :
            base(textView, new CommandId(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Paste)) {
            ClipboardDataProvider = new ClipboardDataProvider();
        }

        public override CommandStatus Status(Guid group, int id) {
            if (REditorSettings.FormatOnPaste &&
                (ClipboardDataProvider.ContainsData(DataFormats.Text) || ClipboardDataProvider.ContainsData(DataFormats.UnicodeText))) {
                return CommandStatus.SupportedAndEnabled;
            }
            return CommandStatus.NotSupported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (!REditorSettings.FormatOnPaste || TextView.Selection.Mode != TextSelectionMode.Stream) {
                return CommandResult.NotSupported;
            }

            string text = ClipboardDataProvider.GetData(DataFormats.UnicodeText) as string;
            if (text == null) {
                text = ClipboardDataProvider.GetData(DataFormats.Text) as string;
            }

            if (text != null) {
                var rSpans = TextView.BufferGraph.MapDownToFirstMatch(
                    TextView.Selection.StreamSelectionSpan.SnapshotSpan,
                    SpanTrackingMode.EdgeInclusive,
                    snapshot => snapshot.TextBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType)
                );
                if (rSpans.Count > 0) {
                    var targetSpan = rSpans[rSpans.Count - 1];

                    IREditorDocument document = REditorDocument.TryFromTextBuffer(targetSpan.Snapshot.TextBuffer);
                    if (document != null) {
                        int insertionPoint = targetSpan.Start;
                        targetSpan.Snapshot.TextBuffer.Replace(targetSpan, text);
                        document.EditorTree.EnsureTreeReady();

                        // We don't want to auto-format inside strings
                        TokenNode node = document.EditorTree.AstRoot.NodeFromPosition(insertionPoint) as TokenNode;
                        if (node == null || node.Token.TokenType != RTokenType.String) {
                            RangeFormatter.FormatRange(TextView, targetSpan.Snapshot.TextBuffer,
                                new TextRange(insertionPoint, text.Length), document.EditorTree.AstRoot,
                                REditorSettings.FormatOptions);
                        }
                    }
                }
            }
            return CommandResult.Executed;
        }
    }
}
