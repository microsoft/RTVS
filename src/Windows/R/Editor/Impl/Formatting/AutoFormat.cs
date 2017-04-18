// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Classification;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    internal static class AutoFormat {
        public static bool IsPreProcessAutoformatTriggerCharacter(char ch) => ch == ';';
        public static bool IsPostProcessAutoformatTriggerCharacter(char ch) => ch.IsLineBreak() || ch == '}';

        public static void HandleAutoformat(ITextView textView, ICoreShell shell, char typedChar) {
            var settings = shell.GetService<IREditorSettings>();
            if (!settings.AutoFormat || (!settings.FormatScope && typedChar == '}')) {
                return;
            }

            SnapshotPoint? rPoint = GetCaretPointInBuffer(textView);
            if (!rPoint.HasValue) {
                return;
            }

            var document = textView.TextBuffer.GetEditorDocument<IREditorDocument>();
            var ast = document.EditorTree.AstRoot;

            // Make sure we are not formatting damaging the projected range in R Markdown
            // which looks like ```{r. 'r' should not separate from {.
            var textBuffer = document.EditorBuffer.As<ITextBuffer>();
            var host = ContainedLanguageHost.GetHost(textView, textBuffer, shell);
            if (host != null && !host.CanFormatLine(textView, textBuffer, textBuffer.CurrentSnapshot.GetLineNumberFromPosition(rPoint.Value))) {
                return;
            }

            // We don't want to auto-format inside strings
            if (ast.IsPositionInsideString(rPoint.Value.Position)) {
                return;
            }

            ITextBuffer subjectBuffer = rPoint.Value.Snapshot.TextBuffer;
            if (typedChar.IsLineBreak()) {
                // Special case for hitting caret after } and before 'else'. We do want to format
                // the construct as '} else {' but if user types Enter after } and we auto-format
                // it will look as if the editor just eats the Enter. Instead, we will not be
                // autoformatting in this specific case. User can always format either the document
                // or select the block and reformat it.
                if (!IsBetweenCurlyAndElse(subjectBuffer, rPoint.Value.Position)) {
                    var scopeStatement = GetFormatScope(textView, subjectBuffer, ast);
                    // Do not format large scope blocks for performance reasons
                    if (scopeStatement != null && scopeStatement.Length < 200) {
                        FormatOperations.FormatNode(textView, subjectBuffer, shell, scopeStatement);
                    } else if (CanFormatLine(textView, subjectBuffer, -1)) {
                        FormatOperations.FormatViewLine(textView, subjectBuffer, -1, shell);
                    }
                }
            } else if (typedChar == ';') {
                // Verify we are at the end of the string and not in a middle
                // of another string or inside a statement.
                ITextSnapshotLine line = subjectBuffer.CurrentSnapshot.GetLineFromPosition(rPoint.Value.Position);
                int positionInLine = rPoint.Value.Position - line.Start;
                string lineText = line.GetText();
                if (positionInLine >= lineText.TrimEnd().Length) {
                    FormatOperations.FormatViewLine(textView, subjectBuffer, 0, shell);
                }
            } else if (typedChar == '}') {
                FormatOperations.FormatCurrentStatement(textView, subjectBuffer, shell, limitAtCaret: true, caretOffset: -1);
            }
        }

        private static bool CanFormatLine(ITextView textView, ITextBuffer textBuffer, int lineOffset) {
            // Do not format inside strings. At this point AST may be empty due to the nature 
            // of [destructive] changes made to the document. We have to resort to tokenizer. 
            // In order to keep performance good during typing we'll use token stream from the classifier.
            var caretPoint = textView.GetCaretPosition(textBuffer);
            if (caretPoint.HasValue) {
                var snapshot = textBuffer.CurrentSnapshot;
                int lineNumber = snapshot.GetLineNumberFromPosition(caretPoint.Value.Position);
                var line = snapshot.GetLineFromLineNumber(lineNumber + lineOffset);

                var classifier = textBuffer.GetService<RClassifier>();
                var tokenIndex = classifier.Tokens.GetItemContaining(line.Start);

                return tokenIndex < 0 || classifier.Tokens[tokenIndex].TokenType != RTokenType.String;
            }
            return false;
        }

        private static bool IsBetweenCurlyAndElse(ITextBuffer textBuffer, int position) {
            // Note that this is post-typing to the construct is now '}<line_break>else'
            int lineNum = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(position);
            if (lineNum < 1) {
                return false;
            }

            var prevLine = textBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNum - 1);

            var leftSide = prevLine.GetText().TrimEnd();
            if (!leftSide.EndsWith("}", StringComparison.Ordinal)) {
                return false;
            }

            var currentLine = textBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNum);
            var rightSide = currentLine.GetText().TrimStart();
            if (!rightSide.StartsWith("else", StringComparison.Ordinal)) {
                return false;
            }

            return true;
        }

        private static SnapshotPoint? GetCaretPointInBuffer(ITextView textView) {
            return textView.BufferGraph.MapDownToFirstMatch(
                textView.Caret.Position.BufferPosition,
                PointTrackingMode.Positive,
                snapshot => snapshot.TextBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType),
                PositionAffinity.Successor
            );
        }

        private static IKeywordScopeStatement GetFormatScope(ITextView textView, ITextBuffer textBuffer, AstRoot ast) {
            var caret = textView.GetCaretPosition(textBuffer);
            if (caret.HasValue) {
                try {
                    var lineNumber = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(caret.Value.Position);
                    var line = textBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber);
                    var lineText = line.GetText();
                    if (lineText.TrimEnd().EndsWith("}", StringComparison.Ordinal)) {
                        var scopeStatement = ast.GetNodeOfTypeFromPosition<IKeywordScopeStatement>(caret.Value);
                        return scopeStatement;
                    }
                } catch (Exception) { }
            }
            return null;
        }
    }
}
