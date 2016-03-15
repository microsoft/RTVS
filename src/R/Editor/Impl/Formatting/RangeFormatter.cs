// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.Selection;
using Microsoft.R.Editor.SmartIndent;
using Microsoft.R.Editor.Undo;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    internal static class RangeFormatter {
        public static bool FormatRange(ITextView textView, ITextBuffer textBuffer, ITextRange formatRange,
                                       RFormatOptions options, int baseIndentPosition = -1) {
            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            int start = formatRange.Start;
            int end = formatRange.End;

            // When user clicks editor margin to select a line, selection actually
            // ends in the beginning of the next line. In order to prevent formatting
            // of the next line that user did not select, we need to shrink span to
            // format and exclude the trailing line break.
            ITextSnapshotLine line = snapshot.GetLineFromPosition(formatRange.End);

            if (line.Start.Position == formatRange.End && formatRange.Length > 0) {
                if (line.LineNumber > 0) {
                    line = snapshot.GetLineFromLineNumber(line.LineNumber - 1);
                    end = line.End.Position;
                    start = Math.Min(start, end);
                }
            }

            // Expand span to include the entire line
            ITextSnapshotLine startLine = snapshot.GetLineFromPosition(start);
            ITextSnapshotLine endLine = snapshot.GetLineFromPosition(end);

            formatRange = TextRange.FromBounds(startLine.Start, endLine.End);
            return FormatRangeExact(textView, textBuffer, formatRange, options, baseIndentPosition);
        }

        public static bool FormatRangeExact(ITextView textView, ITextBuffer textBuffer,
                                            ITextRange formatRange, RFormatOptions options,
                                            int scopeStatementPosition) {
            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            Span spanToFormat = new Span(formatRange.Start, formatRange.Length);
            string spanText = snapshot.GetText(spanToFormat.Start, spanToFormat.Length);
            string trimmedSpanText = spanText.Trim();

            RFormatter formatter = new RFormatter(options);
            string formattedText = formatter.Format(trimmedSpanText);

            formattedText = formattedText.Trim(); // There may be inserted line breaks after {
            // Apply formatted text without indentation. We then will update the parse tree 
            // so we can calculate proper line indents from the AST via the smart indenter.
            if (!spanText.Equals(formattedText, StringComparison.Ordinal)) {
                var selectionTracker = new RSelectionTracker(textView, textBuffer, formatRange);

                RTokenizer tokenizer = new RTokenizer();
                IReadOnlyTextRangeCollection<RToken> oldTokens = tokenizer.Tokenize(spanText);
                IReadOnlyTextRangeCollection<RToken> newTokens = tokenizer.Tokenize(formattedText);

                IncrementalTextChangeApplication.ApplyChangeByTokens(
                    textBuffer,
                    new TextStream(spanText), new TextStream(formattedText),
                    oldTokens, newTokens,
                    formatRange,
                    Resources.AutoFormat, selectionTracker,
                    () => {
                        var ast = UpdateAst(textBuffer);
                        // Apply indentation
                        IndentLines(textView, textBuffer, new TextRange(formatRange.Start, formattedText.Length), ast, options);
                    });

                return true;
            }

            return false;
        }

        private static AstRoot UpdateAst(ITextBuffer textBuffer) {
            IREditorDocument document = REditorDocument.TryFromTextBuffer(textBuffer);
            if (document != null) {
                document.EditorTree.EnsureTreeReady();
                return document.EditorTree.AstRoot;
            }
            return RParser.Parse(new TextProvider(textBuffer.CurrentSnapshot));
        }

        /// <summary>
        /// Appends indentation to each line so formatted text appears properly 
        /// indented inside the host document (script block in HTML page).
        /// </summary>
        private static void IndentLines(ITextView textView, ITextBuffer textBuffer, ITextRange range, AstRoot ast, RFormatOptions options) {
            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            ITextSnapshotLine firstLine = snapshot.GetLineFromPosition(range.Start);
            ITextSnapshotLine lastLine = snapshot.GetLineFromPosition(range.End);
            IREditorDocument document = REditorDocument.TryFromTextBuffer(textBuffer);

            for (int i = firstLine.LineNumber; i <= lastLine.LineNumber; i++) {
                // Snapshot is updated after each insertion so do not cache
                ITextSnapshotLine line = textBuffer.CurrentSnapshot.GetLineFromLineNumber(i);
                int indent = SmartIndenter.GetSmartIndent(line, ast);
                if (indent > 0 && line.Length > 0 && line.Start >= range.Start) {
                    // Check current indentation and correct for the difference
                    int currentIndentSize = line.Length - line.GetText().TrimStart().Length;
                    indent = Math.Max(0, indent - currentIndentSize);
                    if (indent > 0) {
                        string indentString = IndentBuilder.GetIndentString(indent, options.IndentType, options.TabSize);
                        textBuffer.Insert(line.Start, indentString);
                        if (document == null) {
                            // Typically this is a test scenario only. In the real editor
                            // instance document is available and automatically updates AST
                            // when whitespace inserted, not no manual update is necessary.
                            ast.ReflectTextChange(line.Start, 0, indentString.Length);
                        }
                    }
                }
            }
        }
    }
}
