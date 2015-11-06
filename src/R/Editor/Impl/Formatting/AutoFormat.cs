using System;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.Tree.Definitions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    internal static class AutoFormat {
        public static bool IsAutoformatTriggerCharacter(char ch) {
            return ch == '\n' || ch == '\r' || ch == ';' || ch == '}';
        }
        public static bool IgnoreOnce { get; set; }

        public static void HandleAutoformat(ITextView textView, char typedChar) {
            if (!REditorSettings.AutoFormat || IgnoreOnce) {
                IgnoreOnce = false;
                return;
            }

            IEditorTree tree;
            SnapshotPoint? rPoint = GetCaretPointInBuffer(textView, out tree);
            if (rPoint.HasValue) {
                ITextBuffer subjectBuffer = rPoint.Value.Snapshot.TextBuffer;
                if (typedChar == '\r' || typedChar == '\n') {
                    bool formatScope = ShouldFormatScope(textView, subjectBuffer, -1);
                    if (formatScope) {
                        FormatOperations.FormatCurrentNode<IStatement>(textView, subjectBuffer);
                    } else {
                        FormatOperations.FormatLine(textView, subjectBuffer, tree.AstRoot, -1);
                    }
                } else if (typedChar == ';') {
                    FormatOperations.FormatLine(textView, subjectBuffer, tree.AstRoot, 0);
                } else if (typedChar == '}') {
                    FormatOperations.FormatNode<IStatement>(textView, subjectBuffer, Math.Max(rPoint.Value - 1, 0));
                }
            }
        }

        private static SnapshotPoint? GetCaretPointInBuffer(ITextView textView, out IEditorTree tree) {
            tree = null;
            IREditorDocument document = REditorDocument.TryFromTextBuffer(textView.TextBuffer);
            if (document != null) {
                tree = document.EditorTree;
                tree.EnsureTreeReady();
                return textView.BufferGraph.MapDownToFirstMatch(
                    textView.Caret.Position.BufferPosition,
                    PointTrackingMode.Positive,
                    snapshot => snapshot.TextBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType),
                    PositionAffinity.Successor
                );
            }

            return null;
        }

        private static SnapshotPoint? MapCaretToBuffer(ITextView textView, ITextBuffer textBuffer) {
            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            return textView.MapDownToBuffer(textView.Caret.Position.BufferPosition, textBuffer);
        }

        private static bool ShouldFormatScope(ITextView textView, ITextBuffer textBuffer, int lineOffset) {
            IREditorDocument document = REditorDocument.TryFromTextBuffer(textBuffer);
            if (document != null) {
                IEditorTree tree = document.EditorTree;
                tree.EnsureTreeReady();

                SnapshotPoint? caret = MapCaretToBuffer(textView, textBuffer);
                if (caret.HasValue) {
                    try {
                        int lineNumber = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(caret.Value.Position);
                        ITextSnapshotLine line = textBuffer.CurrentSnapshot.GetLineFromLineNumber(Math.Max(lineNumber - 1, 0));
                        string lineText = line.GetText();
                        if (lineText.IndexOfAny(new char[] { '{', '}' }) >= 0) {
                            IKeywordScopeStatement scopeStatement = tree.AstRoot.GetNodeOfTypeFromPosition<IKeywordScopeStatement>(caret.Value);
                            return scopeStatement != null;
                        }
                    } catch (Exception) { }
                }
            }

            return false;
        }
    }
}
