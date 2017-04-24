// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Selection;
using Microsoft.R.Editor.SmartIndent;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    internal static class RangeFormatter {
        public static bool FormatRange(ITextView textView, ITextBuffer textBuffer, ITextRange formatRange, RFormatOptions options, ICoreShell shell) {
            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            int start = formatRange.Start;
            int end = formatRange.End;

            if(!CanFormatRange(textView, textBuffer, formatRange, shell)) {
                return false;
            }

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

            // In case of formatting of multiline expressions formatter needs
            // to know the entire expression since otherwise it may not correctly
            // preserve user indentation. Consider 'x >% y' which is a plain statement
            // and needs to be indented at regular scope level vs
            //
            //      a %>% b %>%
            //          x %>% y
            //
            // where user indentation of 'x %>% y' must be preserved. We don't have
            // complete information here since expression may not be syntactically
            // correct and hence AST may not have correct information and besides,
            // the AST is damaged at this point. As a workaround, we will check 
            // if the previous line ends with an operator current line starts with 
            // an operator.
            int startPosition = FindStartOfExpression(textBuffer, startLine.Start);

            formatRange = TextRange.FromBounds(startPosition, endLine.End);
            return FormatRangeExact(textView, textBuffer, formatRange, options, shell);
        }

        public static bool FormatRangeExact(ITextView textView, ITextBuffer textBuffer, ITextRange formatRange, RFormatOptions options, ICoreShell shell) {
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
                // Extract existing indent before applying changes. Existing indent
                // may be used by the smart indenter for function argument lists.
                var startLine = snapshot.GetLineFromPosition(spanToFormat.Start);
                var originalIndentSizeInSpaces = IndentBuilder.TextIndentInSpaces(startLine.GetText(), options.IndentSize);

                var selectionTracker = new RSelectionTracker(textView, textBuffer, formatRange);
                RTokenizer tokenizer = new RTokenizer();
                IReadOnlyTextRangeCollection<RToken> oldTokens = tokenizer.Tokenize(spanText);
                IReadOnlyTextRangeCollection<RToken> newTokens = tokenizer.Tokenize(formattedText);

                IncrementalTextChangeApplication.ApplyChangeByTokens(
                    textBuffer,
                    new TextStream(spanText), new TextStream(formattedText),
                    oldTokens, newTokens,
                    formatRange,
                    Resources.AutoFormat, selectionTracker, shell,
                    () => {
                        var ast = UpdateAst(textBuffer);
                        // Apply indentation
                        IndentLines(textView, textBuffer, new TextRange(formatRange.Start, formattedText.Length), ast, options, originalIndentSizeInSpaces);
                    });

                return true;
            }

            return false;
        }

        private static AstRoot UpdateAst(ITextBuffer textBuffer) {
            IREditorDocument document = textBuffer.GetEditorDocument<IREditorDocument>();
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
        private static void IndentLines(ITextView textView, ITextBuffer textBuffer,
                                        ITextRange range, AstRoot ast,
                                        RFormatOptions options, int originalIndentSizeInSpaces) {
            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            ITextSnapshotLine firstLine = snapshot.GetLineFromPosition(range.Start);
            ITextSnapshotLine lastLine = snapshot.GetLineFromPosition(range.End);

            IREditorDocument document = textBuffer.GetEditorDocument<IREditorDocument>();

            for (int i = firstLine.LineNumber; i <= lastLine.LineNumber; i++) {
                // Snapshot is updated after each insertion so do not cache
                ITextSnapshotLine line = textBuffer.CurrentSnapshot.GetLineFromLineNumber(i);
                int indent = SmartIndenter.GetSmartIndent(line, options, ast, originalIndentSizeInSpaces, formatting: true);
                if (indent > 0 && line.Length > 0 && line.Start >= range.Start) {
                    // Check current indentation and correct for the difference
                    int currentIndentSize = IndentBuilder.TextIndentInSpaces(line.GetText(), options.TabSize);
                    indent = Math.Max(0, indent - currentIndentSize);
                    if (indent > 0) {
                        string indentString = IndentBuilder.GetIndentString(indent, options.IndentType, options.TabSize);
                        textBuffer.Insert(line.Start, indentString);
                        if (document == null) {
                            // Typically this is a test scenario only. In the real editor
                            // instance document is available and automatically updates AST
                            // when whitespace inserted, not no manual update is necessary.
                            ast.ReflectTextChange(line.Start, 0, indentString.Length, new TextProvider(textBuffer.CurrentSnapshot));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Given position in the buffer tries to detemine start of the expression.
        /// </summary>
        private static int FindStartOfExpression(ITextBuffer textBuffer, int position) {
            // Go up line by line, tokenize each line
            // and check if it starts or ends with an operator
            int lineNum = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(position);
            var tokenizer = new RTokenizer(separateComments: true);

            var text = textBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNum).GetText();
            var tokens = tokenizer.Tokenize(text);
            bool nextLineStartsWithOperator = tokens.Count > 0 && tokens[0].TokenType == RTokenType.Operator;

            for (int i = lineNum - 1; i >= 0; i--) {
                var line = textBuffer.CurrentSnapshot.GetLineFromLineNumber(i);
                text = line.GetText();
                tokens = tokenizer.Tokenize(text);

                if (tokens.Count > 0) {
                    if (!nextLineStartsWithOperator && tokens[tokens.Count - 1].TokenType != RTokenType.Operator) {
                        break;
                    }
                    position = tokens[0].Start + line.Start;
                    nextLineStartsWithOperator = tokens[0].TokenType == RTokenType.Operator;
                }
            }

            return position;
        }

        private static bool CanFormatRange(ITextView textView, ITextBuffer textBuffer, ITextRange formatRange, ICoreShell shell) {
            // Make sure we are not formatting damaging the projected range in R Markdown
            // which looks like ```{r. 'r' should not separate from {.
            var host = ContainedLanguageHost.GetHost(textView, textBuffer, shell);
            if (host != null) {
                ITextSnapshot snapshot = textBuffer.CurrentSnapshot;

                int startLine = snapshot.GetLineNumberFromPosition(formatRange.Start);
                int endLine = snapshot.GetLineNumberFromPosition(formatRange.End);
                for(int i = startLine; i<= endLine; i++) {
                    if (!host.CanFormatLine(textView, textBuffer, i)) {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
