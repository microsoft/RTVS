// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.AST.Functions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Selection;
using Microsoft.R.Editor.SmartIndent;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    internal static class RangeFormatter {
        public static bool FormatRange(ITextView textView, ITextBuffer textBuffer, ITextRange formatRange,
                                       AstRoot ast, RFormatOptions options, bool respectUserIndent = true) {
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
            return FormatRangeExact(textView, textBuffer, formatRange, ast, options, -1, respectUserIndent);
        }

        public static bool FormatRangeExact(ITextView textView, ITextBuffer textBuffer, ITextRange formatRange,
                                            AstRoot ast, RFormatOptions options, 
                                            int scopeStatementPosition, bool respectUserIndent = true) {
            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            Span spanToFormat = new Span(formatRange.Start, formatRange.Length);
            string spanText = snapshot.GetText(spanToFormat.Start, spanToFormat.Length);
            string trimmedSpanText = spanText.Trim();

            if (trimmedSpanText == "}") {
                // Locate opening { and its statement
                var scopeNode = ast.GetNodeOfTypeFromPosition<IAstNodeWithScope>(spanToFormat.Start);
                if (scopeNode != null) {
                    scopeStatementPosition = scopeNode.Start;
                }
            }

            RFormatter formatter = new RFormatter(options);
            string formattedText = formatter.Format(trimmedSpanText);

            formattedText = formattedText.Trim(); // there may be inserted line breaks after {
            formattedText = IndentLines(textBuffer, spanToFormat.Start, ast, formattedText, options, scopeStatementPosition, respectUserIndent);

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
                return true;
            }

            return false;
        }

        /// <summary>
        /// Appends indentation to each line so formatted text appears properly 
        /// indented inside the host document (script block in HTML page).
        /// </summary>
        private static string IndentLines(ITextBuffer textBuffer, int rangeStartPosition, AstRoot ast,
                                           string formattedText, RFormatOptions options,
                                           int scopeStatementPosition, bool respectUserIndent = true) {
            ITextSnapshotLine firstLine = textBuffer.CurrentSnapshot.GetLineFromPosition(rangeStartPosition);
            string firstLineText = firstLine.GetText();
            int baseIndentInSpaces;

            if (scopeStatementPosition >= 0) {
                // If parent statement position is provided, use it to determine indentation
                ITextSnapshotLine statementLine = textBuffer.CurrentSnapshot.GetLineFromPosition(scopeStatementPosition);
                baseIndentInSpaces = SmartIndenter.GetSmartIndent(statementLine, ast);
            } else if (respectUserIndent && RespectUserIndent(textBuffer, ast, rangeStartPosition)) {
                // Determine indent from fist line in multiline constructs
                // such as when function argument list spans multiple lines
                baseIndentInSpaces = IndentBuilder.TextIndentInSpaces(firstLineText, options.TabSize);
            } else {
                baseIndentInSpaces = SmartIndenter.GetSmartIndent(firstLine, ast);
            }

            // There are three major cases with range formatting:
            //  1. Formatting of a scope when } closes.
            //  2. Formatting of a single line on Enter or ;
            //  3. Formatting of a user-selected range.
            //
            // Indentation in (1) is relatively easy since complete scope is known.
            // (2) Is the most difficult is to figure out proper indent of a single }.
            //     Normally we get statementPosition of the statement that define the scope
            // (3) Theoretically may end up with odd indents but users rarely intentionally
            //     select strange ranges

            string indentString = IndentBuilder.GetIndentString(baseIndentInSpaces, options.IndentType, options.TabSize);

            var sb = new StringBuilder();
            IList<string> lines = TextHelper.SplitTextIntoLines(formattedText);

            for (int i = 0; i < lines.Count; i++) {
                string lineText = lines[i];

                if (i == 0 && lineText.Trim() == "{") {
                    if (options.BracesOnNewLine && !LineBreakBeforePosition(textBuffer, rangeStartPosition)) {
                        sb.Append("\r\n");
                    }
                    if (scopeStatementPosition < 0 || options.BracesOnNewLine) {
                        sb.Append(indentString);
                    }
                    sb.Append('{');
                    if (i < lines.Count - 1) {
                        sb.Append("\r\n");
                    }
                    continue;
                }

                if (i == lines.Count - 1 && lineText.Trim() == "}") {
                    sb.Append(indentString);
                    sb.Append('}');
                    break;
                }

                // Leave empty lines alone
                if (!string.IsNullOrWhiteSpace(lineText)) {
                    sb.Append(indentString);
                }

                sb.Append(lineText);
                if (i < lines.Count - 1) {
                    sb.Append("\r\n");
                }
            }

            return sb.ToString();
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
