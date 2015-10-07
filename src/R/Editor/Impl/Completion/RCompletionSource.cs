using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Languages.Core.Utility;
using Microsoft.Languages.Editor.Completion;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Editor.Completion.Engine;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Completion
{
    /// <summary>
    /// Provides actual content for the intellisense dropdown
    /// </summary>
    public sealed class RCompletionSource : ICompletionSource
    {
        private ITextBuffer _textBuffer;

        public RCompletionSource(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
        }

        /// <summary>
        /// Primary entry point for intellisense. This is where intellisense list is getting created.
        /// </summary>
        /// <param name="session">Completion session</param>
        /// <param name="completionSets">Completion sets to populate</param>
        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            Debug.Assert(EditorShell.IsUIThread);

            IREditorDocument doc = REditorDocument.TryFromTextBuffer(_textBuffer);
            if (doc == null)
                return;

            int position = session.GetTriggerPoint(_textBuffer).GetPosition(_textBuffer.CurrentSnapshot);
            PopulateCompletionList(position, session, completionSets, doc.EditorTree.AstRoot);
        }

        internal void PopulateCompletionList(int position, ICompletionSession session, IList<CompletionSet> completionSets, AstRoot ast)
        {
            RCompletionContext context = new RCompletionContext(session, _textBuffer, ast, position);

            bool autoShownCompletion = true;
            if (session.TextView.Properties.ContainsProperty(CompletionController.AutoShownCompletion))
                autoShownCompletion = session.TextView.Properties.GetProperty<bool>(CompletionController.AutoShownCompletion);

            IReadOnlyCollection<IRCompletionListProvider> providers =
                RCompletionEngine.GetCompletionForLocation(context, autoShownCompletion);

            Span applicableSpan = GetApplicableSpan(position, session);
            ITrackingSpan trackingSpan = _textBuffer.CurrentSnapshot.CreateTrackingSpan(applicableSpan, SpanTrackingMode.EdgeInclusive);
            List<RCompletion> completions = new List<RCompletion>();

            foreach (IRCompletionListProvider provider in providers)
            {
                IReadOnlyCollection<RCompletion> entries = provider.GetEntries(context);
                Debug.Assert(entries != null);

                if (entries.Count > 0)
                {
                    completions.AddRange(entries);
                }
            }

            completions.Sort(RCompletion.Compare);
            completions.RemoveDuplicates();

            CompletionSet completionSet = new CompletionSet(
                "R Completion",
                "R Completion",
                trackingSpan,
                completions,
                Enumerable.Empty<RCompletion>());

            completionSets.Add(completionSet);
        }

        private Span GetApplicableSpan(int position, ICompletionSession session)
        {
            var selectedSpans = session.TextView.Selection.SelectedSpans;
            if (selectedSpans.Count == 1 && selectedSpans[0].Span.Length > 0)
            {
                return selectedSpans[0].Span;
            }

            ITextSnapshot snapshot = _textBuffer.CurrentSnapshot;
            ITextSnapshotLine line = snapshot.GetLineFromPosition(position);
            string lineText = snapshot.GetText(line.Start, line.Length);
            int linePosition = position - line.Start;

            int start = 0;
            int end = line.Length;

            for (int i = linePosition - 1; i >= 0; i--)
            {
                char ch = lineText[i];
                if (!Char.IsLetterOrDigit(ch) && ch != '_' && ch != '.')
                {
                    start = i + 1;
                    break;
                }
            }

            for (int i = linePosition; i < lineText.Length; i++)
            {
                char ch = lineText[i];
                if (!Char.IsLetterOrDigit(ch) && ch != '_' && ch != '.')
                {
                    end = i;
                    break;
                }
            }

            if (start < end)
            {
                return new Span(start + line.Start, end - start);
            }

            return new Span(position, 0);
        }

        #region Dispose
        public void Dispose()
        {
            if (_textBuffer != null)
            {
                _textBuffer = null;
            }
        }
        #endregion
    }
}
