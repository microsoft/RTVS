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
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Selection;
using Microsoft.R.Editor.SmartIndent;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting
{
    internal static class RangeFormatter
    {
        public static bool FormatRange(ITextView textView, ITextBuffer textBuffer, ITextRange range, AstRoot ast, RFormatOptions options)
        {
            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;

            int start = range.Start;
            int end = range.End;

            // When user clicks editor margin to select a line, selection actually
            // ends in the beginning of the next line. In order to prevent formatting
            // of the next line that user did not select, we need to shrink span to
            // format and exclude the trailing line break.
            ITextSnapshotLine line = snapshot.GetLineFromPosition(range.End);
            if (line.Start.Position == range.End && range.Length > 0)
            {
                if (line.LineNumber > 0)
                {
                    line = snapshot.GetLineFromLineNumber(line.LineNumber - 1);
                    end = line.End.Position;
                    start = Math.Min(start, end);
                }
            }

            // Expand span to include the entire line
            ITextSnapshotLine startLine = snapshot.GetLineFromPosition(start);
            ITextSnapshotLine endLine = snapshot.GetLineFromPosition(end);

            Span spanToFormat = Span.FromBounds(startLine.Start, endLine.End);
            string spanText = snapshot.GetText(spanToFormat.Start, spanToFormat.Length);
            string trimmedSpanText = spanText.Trim();

            RFormatter formatter = new RFormatter(options);
            string formattedText = formatter.Format(trimmedSpanText);

            formattedText = formattedText.Trim(); // there may be inserted line breaks after {
            formattedText = AppendIndent(textBuffer, spanToFormat.Start, ast, formattedText, options);

            if (!spanText.Equals(formattedText, StringComparison.Ordinal))
            {
                textView.Properties["InFormatting"] = true;
                try {
                    var selectionTracker = new RSelectionTracker(textView, textBuffer);
                    IncrementalTextChangeApplication.ApplyChange(textBuffer, spanToFormat.Start,
                        spanToFormat.Length, formattedText, Resources.AutoFormat, selectionTracker, Int32.MaxValue);
                }
                finally {
                    textView.Properties["InFormatting"] = false;
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Appends indentation to each line so formatted text appears properly 
        /// indented inside the host document (script block in HTML page).
        /// </summary>
        private static string AppendIndent(ITextBuffer textBuffer, int position, AstRoot ast, string formattedText, RFormatOptions options)
        {
            ITextSnapshotLine line = textBuffer.CurrentSnapshot.GetLineFromPosition(position);
            string lineText = line.GetText();
            int textIndentInSpaces;

            if (RespectUserIndent(textBuffer, ast, position))
            {
                textIndentInSpaces = IndentBuilder.TextIndentInSpaces(lineText, options.TabSize);
            }
            else
            {
                textIndentInSpaces = SmartIndenter.GetSmartIndent(line, ast);
            }

            // Figure out indent from the enclosing scope
            IScope scope = ast.GetNodeOfTypeFromPosition<IScope>(position);

            // Check if position is actually inside the scope.
            // In range formatting it may be outside as in |{...}|
            // and in this case we want one level less of indentation.          

            string indentString = IndentBuilder.GetIndentString(textIndentInSpaces, options.IndentType, options.TabSize);
            int outerIndentInSpaces = SmartIndenter.OuterIndentSizeFromScope(textBuffer, scope, options);
            string outerIndentString = IndentBuilder.GetIndentString(outerIndentInSpaces, options.IndentType, options.TabSize);

            var sb = new StringBuilder();
            IList<string> lines = TextHelper.SplitTextIntoLines(formattedText);

            for (int i = 0; i < lines.Count; i++)
            {
                lineText = lines[i];

                if (i == lines.Count - 1 && lineText.Trim() == "}")
                {
                    sb.Append(outerIndentString);
                    sb.Append('}');
                    break;
                }

                // Leave empty lines alone
                if (!string.IsNullOrWhiteSpace(lineText))
                {
                    sb.Append(indentString);
                }

                sb.Append(lineText);

                if (i < lines.Count - 1)
                    sb.Append("\r\n");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Determines if a given position is in area where user
        /// specified indentation must be respected. For example,
        /// in multi-line list of function arguments, 
        /// multi-line expressions and so on.
        /// </summary>
        private static bool RespectUserIndent(ITextBuffer textBuffer, AstRoot ast, int position)
        {
            // Look up nearest expression
            IAstNode node = ast.GetNodeOfTypeFromPosition<Expression>(position);
            if(IsMultilineNode(textBuffer, node))
            {
                return true;
            }

            node = ast.GetNodeOfTypeFromPosition<FunctionDefinition>(position);
            if (IsMultilineNode(textBuffer, node))
            {
                return true;
            }

            node = ast.GetNodeOfTypeFromPosition<FunctionCall>(position);
            if (IsMultilineNode(textBuffer, node))
            {
                return true;
            }

            return false;
        }

        private static bool IsMultilineNode(ITextBuffer textBuffer, IAstNode node)
        {
            if(node == null)
            {
                return false;
            }

            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            int length = snapshot.Length;

            if (node.End < length)
            {
                ITextSnapshotLine startLine = textBuffer.CurrentSnapshot.GetLineFromPosition(node.Start);
                ITextSnapshotLine endLine = textBuffer.CurrentSnapshot.GetLineFromPosition(node.End);

                return startLine.LineNumber != endLine.LineNumber;
            }

            return true;
        }
    }
}
