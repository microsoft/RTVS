// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Selection;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.AST.Functions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.Selection;
using Microsoft.R.Editor.SmartIndent;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    internal static class RangeFormatter {
        public static bool FormatRange(ITextView textView, ITextBuffer textBuffer, ITextRange formatRange,
                                       AstRoot ast, RFormatOptions options) {
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
            return FormatRangeExact(textView, textBuffer, formatRange, ast, options, -1);
        }

        public static bool FormatRangeExact(ITextView textView, ITextBuffer textBuffer, ITextRange formatRange,
                                            AstRoot ast, RFormatOptions options, int scopeStatementPosition) {
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
                var selectionTracker = new RSelectionTracker(textView, textBuffer);
                RTokenizer tokenizer = new RTokenizer();
                IReadOnlyTextRangeCollection<RToken> oldTokens = tokenizer.Tokenize(spanText);
                IReadOnlyTextRangeCollection<RToken> newTokens = tokenizer.Tokenize(formattedText);
                IncrementalTextChangeApplication.ApplyChangeByTokens(
                    textBuffer,
                    new TextStream(spanText), new TextStream(formattedText),
                    oldTokens, newTokens,
                    formatRange,
                    Resources.AutoFormat, selectionTracker);

                // Now apply indentation
                IREditorDocument document = REditorDocument.FromTextBuffer(textBuffer);
                document.EditorTree.EnsureTreeReady();
                ast = document.EditorTree.AstRoot;
                IndentLines(textView, textBuffer, new TextRange(formatRange.Start, formattedText.Length), ast, options);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Appends indentation to each line so formatted text appears properly 
        /// indented inside the host document (script block in HTML page).
        /// </summary>
        private static void IndentLines(ITextView textView, ITextBuffer textBuffer, ITextRange range, AstRoot ast, RFormatOptions options) {
            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            ITextSnapshotLine firstLine = snapshot.GetLineFromPosition(range.Start);
            ITextSnapshotLine lastLine = snapshot.GetLineFromPosition(range.End);

            var selectionTracker = new RSelectionTracker(textView, textBuffer);
            using (var selectionUndo = new SelectionUndo(selectionTracker, Resources.AutoFormat, automaticTracking: false)) {
                using (ITextEdit edit = textBuffer.CreateEdit()) {
                    for (int i = lastLine.LineNumber; i >= firstLine.LineNumber; i++) {
                        ITextSnapshotLine line = snapshot.GetLineFromLineNumber(i);
                        int indent = SmartIndenter.GetSmartIndent(line, ast);
                        string indentString = IndentBuilder.GetIndentString(indent, options.IndentType, options.TabSize);
                        edit.Insert(line.Start, indentString);
                    }
                    edit.Apply();
                }
            }
        }

        private static bool LineBreakBeforePosition(ITextBuffer textBuffer, int position) {
            for (int i = position - 1; i >= 0; i--) {
                char ch = textBuffer.CurrentSnapshot[i];
                if (ch.IsLineBreak()) {
                    return true;
                }
                if (!char.IsWhiteSpace(ch)) {
                    break;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines if a given position is in area where user
        /// specified indentation must be respected. For example,
        /// in multi-line list of function arguments, 
        /// multi-line expressions and so on.
        /// </summary>
        private static bool RespectUserIndent(ITextBuffer textBuffer, AstRoot ast, int position) {
            // Look up nearest expression
            IAstNode node = ast.GetNodeOfTypeFromPosition<Expression>(position);
            if (IsMultilineNode(textBuffer, node)) {
                return true;
            }

            node = ast.GetNodeOfTypeFromPosition<FunctionDefinition>(position);
            if (IsMultilineNode(textBuffer, node)) {
                return true;
            }

            node = ast.GetNodeOfTypeFromPosition<FunctionCall>(position);
            if (IsMultilineNode(textBuffer, node)) {
                return true;
            }

            return false;
        }

        private static bool IsMultilineNode(ITextBuffer textBuffer, IAstNode node) {
            if (node == null) {
                return false;
            }

            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            int length = snapshot.Length;

            if (node.End < length) {
                ITextSnapshotLine startLine = textBuffer.CurrentSnapshot.GetLineFromPosition(node.Start);
                ITextSnapshotLine endLine = textBuffer.CurrentSnapshot.GetLineFromPosition(node.End);

                return startLine.LineNumber != endLine.LineNumber;
            }

            return true;
        }
    }
}
