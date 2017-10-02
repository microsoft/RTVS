// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Classification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    public sealed class VsAutoFormat : AutoFormat {
        private readonly ITextView _textView;

        public VsAutoFormat(ITextView textView, IServiceContainer services) :
            base(textView.ToEditorView(), null, services) {
            _textView = textView;
        }

        public void HandleTyping(char typedChar) {
            var rPoint = GetCaretPointInBuffer(_textView);
            if(rPoint.HasValue) {
                HandleTyping(typedChar, rPoint.Value.Position, rPoint.Value.Snapshot.TextBuffer.ToEditorBuffer());
            }
        }

        /// <summary>
        /// Determines if contained language line can be formatted.
        /// </summary>
        /// <param name="position">Position in the contained language buffer</param>
        /// <param name="typedChar">Typed character</param>
        /// <remarks>In R Markdown lines with ```{r should not be formatted</remarks>
        protected override bool CanFormatContainedLanguageLine(int position, char typedChar) {
            // Make sure we are not formatting damaging the projected range in R Markdown
            // which looks like ```{r. 'r' should not separate from {.
            var textBuffer = EditorBuffer.As<ITextBuffer>();
            var host = textBuffer.GetService<IContainedLanguageHost>();
            // If user typed enter, we should be asking for permission to format previous
            // line since automatic formatting is post-editing operation
            if (host != null) {
                var lineNumber = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(position);
                if (typedChar.IsLineBreak()) {
                    lineNumber--;
                }
                return host.CanFormatLine(EditorView, EditorBuffer, lineNumber);
            }
            return true;
        }
        protected override bool CanFormatLine(int position, int lineOffset) {
            // Do not format inside strings. At this point AST may be empty due to the nature 
            // of [destructive] changes made to the document. We have to resort to tokenizer. 
            // In order to keep performance good during typing we'll use token stream from the classifier.
            var snapshot = EditorBuffer.CurrentSnapshot;
            var lineNumber = snapshot.GetLineNumberFromPosition(position);
            var line = snapshot.GetLineFromLineNumber(lineNumber + lineOffset);

            var classifier = RClassifierProvider.GetRClassifier(EditorBuffer.As<ITextBuffer>());
            var tokenIndex = classifier.Tokens.GetItemContaining(line.Start);

            return tokenIndex < 0 || classifier.Tokens[tokenIndex].TokenType != RTokenType.String;
        }

        private static SnapshotPoint? GetCaretPointInBuffer(ITextView textView) {
            return textView.BufferGraph.MapDownToFirstMatch(
                textView.Caret.Position.BufferPosition,
                PointTrackingMode.Positive,
                snapshot => snapshot.TextBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType),
                PositionAffinity.Successor
            );
        }
    }
}
