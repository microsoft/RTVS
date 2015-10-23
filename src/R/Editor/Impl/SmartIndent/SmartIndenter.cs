using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.SmartIndent
{
    /// <summary>
    /// Provides block and smart indentation in R code
    /// </summary>
    internal sealed class SmartIndenter : ISmartIndent
    {
        private ITextView _textView;

        public static SmartIndenter Attach(ITextView textView)
        {
            SmartIndenter indenter = ServiceManager.GetService<SmartIndenter>(textView);

            if (indenter == null)
            {
                indenter = new SmartIndenter(textView);
            }

            return indenter;
        }

        private SmartIndenter(ITextView textView)
        {
            _textView = textView;
        }

        #region ISmartIndent;
        public int? GetDesiredIndentation(ITextSnapshotLine line)
        {
            int? res = GetDesiredIndentation(line, REditorSettings.IndentStyle);
            if (res != null && line.Snapshot.TextBuffer != _textView.TextBuffer) {
                var target = _textView.BufferGraph.MapUpToBuffer(
                    line.Start,
                    PointTrackingMode.Positive,
                    PositionAffinity.Successor,
                    _textView.TextBuffer
                );

                if (target != null) {
                    // The indentation level is relative to the line in the text view when
                    // we were created, not to the line we were provided with on this call.
                    var diff = target.Value.Position - target.Value.GetContainingLine().Start.Position;
                    return diff + res;
                }
            }
            return res;
        }

        public int? GetDesiredIndentation(ITextSnapshotLine line, IndentStyle indentStyle)
        {
            if (line != null)
            {
                if (indentStyle == IndentStyle.Block)
                {
                    return GetBlockIndent(line);
                }
                else if (indentStyle == IndentStyle.Smart)
                {
                    return GetSmartIndent(line);
                }
            }

            return null;
        }

        public void Dispose()
        {
        }
        #endregion

        public static int GetBlockIndent(ITextSnapshotLine line)
        {
            int lineNumber = line.LineNumber;

            //Scan the previous lines for the first line that isn't an empty line.
            while (--lineNumber >= 0)
            {
                ITextSnapshotLine previousLine = line.Snapshot.GetLineFromLineNumber(lineNumber);
                if (previousLine.Length > 0)
                {
                    return OuterIndentSizeFromLine(previousLine, REditorSettings.FormatOptions);
                }
            }

            return 0;
        }

        public static int GetSmartIndent(ITextSnapshotLine line, AstRoot ast = null)
        {
            ITextBuffer textBuffer = line.Snapshot.TextBuffer;
            IScope scope;

            if (ast == null)
            {
                IREditorDocument document = REditorDocument.TryFromTextBuffer(textBuffer);
                if (document == null)
                {
                    return 0;
                }

                ast = document.EditorTree.AstRoot;
            }

            // Try conditional without scope first
            if (line.LineNumber > 0)
            {
                ITextSnapshotLine prevLine = line.Snapshot.GetLineFromLineNumber(line.LineNumber - 1);

                string prevLineText = prevLine.GetText();
                int nonWsPosition = prevLine.Start + (prevLineText.Length - prevLineText.TrimStart().Length) + 1;

                IKeywordScopeStatement scopeStatement = ast.GetNodeOfTypeFromPosition<IKeywordScopeStatement>(nonWsPosition);
                if (scopeStatement != null && (scopeStatement.Scope == null || scopeStatement.Scope is SimpleScope))
                {
                    // There is if with a simple scope above. However, we need to check 
                    // if the line that is being formatted is actually part of this scope.
                    if (scopeStatement.Scope == null || (scopeStatement.Scope != null && line.Start < scopeStatement.Scope.End))
                    {
                        return GetBlockIndent(line) + REditorSettings.IndentSize;
                    }
                    else
                    {
                        scope = ast.GetNodeOfTypeFromPosition<IScope>(scopeStatement.Start);
                        return InnerIndentSizeFromScope(textBuffer, scope, REditorSettings.FormatOptions);
                    }
                }
            }

            scope = ast.GetNodeOfTypeFromPosition<IScope>(line.Start);
            if (scope != null && scope.OpenCurlyBrace != null)
            {
                return InnerIndentSizeFromScope(textBuffer, scope, REditorSettings.FormatOptions);
            }

            return 0;
        }

        public static int InnerIndentSizeFromScope(ITextBuffer textBuffer, IScope scope, RFormatOptions options)
        {
            if (scope != null && scope.OpenCurlyBrace != null)
            {
                ITextSnapshotLine scopeStartLine = textBuffer.CurrentSnapshot.GetLineFromPosition(scope.OpenCurlyBrace.Start);
                return InnerIndentSizeFromLine(scopeStartLine, options);
            }

            return 0;
        }

        public static int OuterIndentSizeFromScope(ITextBuffer textBuffer, IScope scope, RFormatOptions options)
        {
            if (scope != null && scope.OpenCurlyBrace != null)
            {
                ITextSnapshotLine scopeStartLine = textBuffer.CurrentSnapshot.GetLineFromPosition(scope.OpenCurlyBrace.Start);
                return OuterIndentSizeFromLine(scopeStartLine, options);
            }

            return 0;
        }

        public static int InnerIndentSizeFromLine(ITextSnapshotLine line, RFormatOptions options)
        {
            string lineText = line.GetText();
            string leadingWhitespace = lineText.Substring(0, lineText.Length - lineText.TrimStart().Length);
            IndentBuilder indentbuilder = new IndentBuilder(options.IndentType, options.IndentSize, options.TabSize);

            return IndentBuilder.TextIndentInSpaces(leadingWhitespace + indentbuilder.SingleIndentString, options.TabSize);
        }

        public static int OuterIndentSizeFromLine(ITextSnapshotLine line, RFormatOptions options)
        {
            string lineText = line.GetText();
            string leadingWhitespace = lineText.Substring(0, lineText.Length - lineText.TrimStart().Length);

            return IndentBuilder.TextIndentInSpaces(leadingWhitespace, options.TabSize);
        }
    }
}
