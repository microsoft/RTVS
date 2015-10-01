using Microsoft.Languages.Editor.Completion.TypeThrough;
using Microsoft.Languages.Editor.EditorHelpers;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Completion.AutoCompletion
{
    public static class SeparatorCompletion
    {
        private static bool _suppressCompletion;

        /// <summary>
        /// Temporary solution: Using _suppressCompletion is a bad design and needs to be removed.
        /// There must be a better way to ignore a typed quote after it was used for overtype.
        /// </summary>
        internal static void CancelSuppression()
        {
            _suppressCompletion = false;
        }

        public static void Complete(ITextView textView, char typedChar)
        {
            if (_suppressCompletion)
            {
                _suppressCompletion = false;
                return;
            }

            if (!TextViewHelpers.IsAutoInsertAllowed(textView))
            {
                return;
            }

            // Do not complete '\"' in strings or at the end of a string token
            if (typedChar == '\"' || typedChar == '\'')
            {
                AstRoot ast = REditorDocument.FromTextBuffer(textView.TextBuffer).EditorTree.AstRoot;
                int position = textView.Selection.SelectedSpans[0].Start;

                TokenNode node = ast.GetNodeOfTypeFromPosition<TokenNode>(position, includeEnd: true);
                if (node != null && node.Token.TokenType == RTokenType.String)
                {
                    return;
                }
            }

            SimpleComplete(textView, typedChar);
        }

        private static void SimpleComplete(ITextView textView, char typedChar)
        {
            char completeChar = '\0';

            switch (typedChar)
            {
                case '\'':
                case '\"':
                    completeChar = typedChar;
                    break;

                case '{':
                    completeChar = '}';
                    break;

                case '(':
                    completeChar = ')';
                    break;

                case '[':
                    completeChar = ']';
                    break;
            }

            ITextSnapshot snapshot = textView.TextBuffer.CurrentSnapshot;
            int position = textView.Selection.SelectedSpans[0].Start;

            if (completeChar != '\0' && position < snapshot.Length)
            {
                RCompletionController completionController = ServiceManager.GetService<RCompletionController>(textView);
                ProvisionalText innerProvisional = (completionController != null) ? completionController.GetInnerProvisionalText() : null;

                if (!TypeThroughController.StaticCanCompleteBefore(
                    textView,
                    textView.TextBuffer,
                    position,
                    typedChar,
                    innerProvisional))
                {
                    completeChar = '\0';
                }
            }

            if (completeChar != '\0')
            {
                RCompletionController completionController = ServiceManager.GetService<RCompletionController>(textView);

                ProvisionalText.IgnoreChange = true;
                textView.TextBuffer.Replace(new Span(position, 0), completeChar.ToString());
                ProvisionalText.IgnoreChange = false;

                textView.Caret.MoveTo(new SnapshotPoint(textView.TextBuffer.CurrentSnapshot, position));

                CreateProvisionalText(textView, position, '\0');
            }
        }

        private static void CreateProvisionalText(ITextView textView, int position, char eatNextQuote)
        {
            CreateProvisionalText(textView, new Span(position - 1, 2), eatNextQuote);
        }

        public static void CreateProvisionalText(ITextView textView, Span range, char eatNextQuote)
        {
            var completionController = ServiceManager.GetService<RCompletionController>(textView);
            if (completionController != null)
            {
                var provisionalText = completionController.CreateProvisionalText(range, eatNextQuote);
                if (provisionalText != null)
                {
                    provisionalText.Overtyping += OnProvisionalTextOvertype;
                    textView.Caret.PositionChanged += OnCaretPositionChanged;
                }
            }
        }

        private static void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            CancelSuppression();
            e.TextView.Caret.PositionChanged -= OnCaretPositionChanged;
        }

        private static void OnProvisionalTextOvertype(object sender, System.EventArgs e)
        {
            var provisionalText = sender as ProvisionalText;

            _suppressCompletion = true;
            provisionalText.Overtyping -= OnProvisionalTextOvertype;
        }
    }
}
