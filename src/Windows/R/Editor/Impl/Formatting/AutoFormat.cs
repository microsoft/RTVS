// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
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
    internal sealed class AutoFormat {
        private readonly IServiceContainer _services;
        private readonly IREditorSettings _settings;
        private readonly ITextView _textView;

        public AutoFormat(ITextView textView, IServiceContainer services) {
            _textView = textView;
            _services = services;
            _settings = services.GetService<IREditorSettings>();
        }

        public bool IsPreProcessAutoformatTriggerCharacter(char ch) => ch == ';';
        public bool IsPostProcessAutoformatTriggerCharacter(char ch) => ch.IsLineBreak() || ch == '}';
        
        public void HandleAutoformat(char typedChar) {
            if (!_settings.AutoFormat || (!_settings.FormatScope && typedChar == '}')) {
                return;
            }

            var rPoint = GetCaretPointInBuffer(_textView);
            if (!rPoint.HasValue) {
                return;
            }

            var document = _textView.TextBuffer.GetEditorDocument<IREditorDocument>();
            var ast = document.EditorTree.AstRoot;
            var editorView = _textView.ToEditorView();

            // Make sure we are not formatting damaging the projected range in R Markdown
            // which looks like ```{r. 'r' should not separate from {.
            if(!CanFormatContainedLanguageLine(editorView, document.EditorBuffer, rPoint.Value, typedChar)) {
                return;
            }

            // We don't want to auto-format inside strings
            if (ast.IsPositionInsideString(rPoint.Value.Position)) {
                return;
            }

            var subjectBuffer = rPoint.Value.Snapshot.TextBuffer;
            var editorBuffer = subjectBuffer.ToEditorBuffer();

            if (typedChar.IsLineBreak()) {
                // Special case for hitting caret after } and before 'else'. We do want to format
                // the construct as '} else {' but if user types Enter after } and we auto-format
                // it will look as if the editor just eats the Enter. Instead, we will not be
                // autoformatting in this specific case. User can always format either the document
                // or select the block and reformat it.
                if (!IsBetweenCurlyAndElse(subjectBuffer, rPoint.Value.Position)) {
                    var scopeStatement = GetFormatScope(_textView, subjectBuffer, ast);
                    // Do not format large scope blocks for performance reasons
                    if (scopeStatement != null && scopeStatement.Length < 200) {
                        FormatOperations.FormatNode(editorView, editorBuffer, _services, scopeStatement);
                    } else if (CanFormatLine(subjectBuffer, -1)) {
                        FormatOperations.FormatViewLine(editorView, editorBuffer, -1, _services);
                    }
                }
            } else if (typedChar == ';') {
                // Verify we are at the end of the string and not in a middle
                // of another string or inside a statement.
                var line = subjectBuffer.CurrentSnapshot.GetLineFromPosition(rPoint.Value.Position);
                var positionInLine = rPoint.Value.Position - line.Start;
                var lineText = line.GetText();
                if (positionInLine >= lineText.TrimEnd().Length) {
                    FormatOperations.FormatViewLine(editorView, editorBuffer, 0, _services);
                }
            } else if (typedChar == '}') {
                FormatOperations.FormatCurrentStatement(editorView, editorBuffer, _services, limitAtCaret: true, caretOffset: -1);
            }
        }

        /// <summary>
        /// Determines if contained language line can be formatted.
        /// </summary>
        /// <param name="editorView">Editor view</param>
        /// <param name="editorBuffer">Contained language buffer</param>
        /// <param name="position">Position in the contained language buffer</param>
        /// <param name="typedChar">Typed character</param>
        /// <remarks>In R Markdown lines with ```{r should not be formatted</remarks>
        private bool CanFormatContainedLanguageLine(IEditorView editorView, IEditorBuffer editorBuffer, int position, char typedChar) {
            // Make sure we are not formatting damaging the projected range in R Markdown
            // which looks like ```{r. 'r' should not separate from {.
            var textBuffer = editorBuffer.As<ITextBuffer>();
            var host = textBuffer.GetService<IContainedLanguageHost>();
            // If user typed enter, we should be asking for permission to format previous
            // line since automatic formatting is post-editing operation
            if (host != null) {
                var lineNumber = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(position);
                if (typedChar.IsLineBreak()) {
                    lineNumber--;
                }
                return host.CanFormatLine(editorView, textBuffer.ToEditorBuffer(), lineNumber);
            }
            return true;
        }

        private bool CanFormatLine(ITextBuffer textBuffer, int lineOffset) {
            // Do not format inside strings. At this point AST may be empty due to the nature 
            // of [destructive] changes made to the document. We have to resort to tokenizer. 
            // In order to keep performance good during typing we'll use token stream from the classifier.
            var caretPoint = _textView.GetCaretPosition(textBuffer);
            if (caretPoint.HasValue) {
                var snapshot = textBuffer.CurrentSnapshot;
                var lineNumber = snapshot.GetLineNumberFromPosition(caretPoint.Value.Position);
                var line = snapshot.GetLineFromLineNumber(lineNumber + lineOffset);

                var classifier = RClassifierProvider.GetRClassifier(textBuffer);
                var tokenIndex = classifier.Tokens.GetItemContaining(line.Start);

                return tokenIndex < 0 || classifier.Tokens[tokenIndex].TokenType != RTokenType.String;
            }
            return false;
        }

        private static bool IsBetweenCurlyAndElse(ITextBuffer textBuffer, int position) {
            // Note that this is post-typing to the construct is now '}<line_break>else'
            var lineNum = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(position);
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
                } catch (Exception ex)  when (!ex.IsCriticalException()) { }
            }
            return null;
        }
    }
}
