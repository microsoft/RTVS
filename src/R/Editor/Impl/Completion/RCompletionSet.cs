using System;
using System.Collections.Generic;
using Microsoft.Languages.Editor.Completion;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Completion
{
    /// <summary>
    /// This represents a related list of values to show in the completion window
    /// </summary>
    public sealed class RCompletionSet : CompletionSet, IDisposable
    {
        private static readonly string CompletionSetMoniker = "RCompletion";
        private static readonly string CompletionSetDisplayName = "R";

        public RCompletionContext CompletionContext { get; private set; }
        public IList<IRCompletionListProvider> CompletionProviders { get; private set; }

        private ITextBuffer _textBuffer;

        public RCompletionSet(
            ITrackingSpan trackingSpan,
            IEnumerable<RCompletion> completions,
            IEnumerable<RCompletion> builders,
            RCompletionContext context,
            IList<IRCompletionListProvider> providers)
            : base(CompletionSetMoniker, CompletionSetDisplayName, trackingSpan, completions, builders)
        {
            CompletionContext = context;
            CompletionProviders = providers;

            _textBuffer = trackingSpan.TextBuffer;
            _textBuffer.Changed += TextBuffer_Changed;
        }

        void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            if (CompletionContext.Session == null)
            {
                return;
            }

            //bool dismissSession = false;
            //foreach (ITextChange textChange in e.Changes)
            //{
            //    if (textChange.OldPosition < _lastApplicableToSpan.Start)
            //    {
            //        dismissSession = true;
            //        break;
            //    }
            //    else if (textChange.OldEnd > _lastApplicableToSpan.End)
            //    {
            //        dismissSession = true;
            //        break;
            //    }
            //}

            //_lastApplicableToSpan = CompletionContext.Session. ApplicableToCommittedSpan.GetCurrentSpan();
            //if (dismissSession)
            //{
            //    CompletionContext.Session.Dismiss();
            //}
        }

        public void Dispose()
        {
            if (_textBuffer != null)
            {
                _textBuffer.Changed -= TextBuffer_Changed;
                _textBuffer = null;
            }
        }
    }
}
