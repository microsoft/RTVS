using Microsoft.Languages.Editor.Completion.TypeThrough;
using Microsoft.Languages.Editor.EditorHelpers;
using Microsoft.Languages.Editor.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Completion.AutoCompletion
{
    public static class QuoteCompletion
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

        public static void CompleteQuotes(ITextView textView, int position, char typedChar)
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

            SimpleComplete(textView, position, typedChar);
        }

        private static void SimpleComplete(ITextView textView, int position, char typedChar)
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

            if (completeChar != '\0' && position < snapshot.Length)
            {
                RCompletionController completionController = ServiceManager.GetService<RCompletionController>(textView);
                ProvisionalText innerProvisional = (completionController != null) ? completionController.GetInnerProvisionalText() : null;

                if (!TypeThroughController.StaticCanCompleteBefore(
                    textView,
                    textView.TextBuffer,
                    position,
                    typedChar,
                    c => !char.IsWhiteSpace(c) && c != '<',
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

            var provisionalText = completionController.CreateProvisionalText(range, eatNextQuote);
            if (provisionalText != null)
            {
                provisionalText.Overtyping += OnProvisionalTextOvertype;
                textView.Caret.PositionChanged += OnCaretPositionChanged;
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
