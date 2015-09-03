using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Search;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Selection;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting
{
    internal static class RangeFormatter
    {
        public static bool FormatRange(ITextView textView, ITextRange range, AstRoot ast, RFormatOptions options)
        {
            ITextBuffer textBuffer = textView.TextBuffer;
            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;

            // Expand span to include the entire line
            ITextSnapshotLine startLine = snapshot.GetLineFromPosition(range.Start);
            ITextSnapshotLine endLine = snapshot.GetLineFromPosition(range.End);

            Span spanToFormat = Span.FromBounds(startLine.Start, endLine.End);
            string spanText = snapshot.GetText(spanToFormat.Start, spanToFormat.Length).Trim();

            RFormatter formatter = new RFormatter(options);
            string formattedText = formatter.Format(spanText);

            formattedText = formattedText.Trim(); // there may be inserted line breaks after {
            formattedText = AppendIndent(textView.TextBuffer, spanToFormat.Start, ast, formattedText, options);

            if (!spanText.Equals(formattedText, StringComparison.Ordinal))
            {
                var selectionTracker = new RSelectionTracker(textView, textBuffer);
                IncrementalTextChangeApplication.ApplyChange(textBuffer, spanToFormat.Start,
                    spanToFormat.Length, formattedText, Resources.AutoFormat, selectionTracker, Int32.MaxValue);

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

            // Firgure out indent from the enclosing scope
            IScope scope = ast.GetSpecificNodeFromPosition(position, (IAstNode n) =>
            {
                return n is IScope;
            }) as IScope;

            int textIndentInSpaces = IndentSizeFromScope(textBuffer, scope, options);
            string indentString = IndentBuilder.GetIndentString(textIndentInSpaces, options.IndentType, options.TabSize);
            var sb = new StringBuilder();

            IList<string> lines = TextHelper.SplitTextIntoLines(formattedText);

            for (int i = 0; i < lines.Count; i++)
            {
                lineText = lines[i];

                // Leave empty lines alone
                if (!string.IsNullOrWhiteSpace(lineText))
                    sb.Append(indentString);

                sb.Append(lineText);

                if (i < lines.Count - 1)
                    sb.Append("\r\n");
            }

            return sb.ToString();
        }

        public static int IndentSizeFromScope(ITextBuffer textBuffer, IScope scope, RFormatOptions options)
        {
            if (scope != null && scope.OpenCurlyBrace != null)
            {
                ITextSnapshotLine scopeStartLine = textBuffer.CurrentSnapshot.GetLineFromPosition(scope.OpenCurlyBrace.Start);
                string scopeLineText = scopeStartLine.GetText();

                string leadingWhitespace = scopeLineText.Substring(0, scopeLineText.Length - scopeLineText.TrimStart().Length);
                IndentBuilder indentbuilder = new IndentBuilder(options.IndentType, options.IndentSize, options.TabSize);

                return IndentBuilder.TextIndentInSpaces(leadingWhitespace + indentbuilder.SingleIndentString, options.TabSize);
            }

            return 0;
        }
    }
}
