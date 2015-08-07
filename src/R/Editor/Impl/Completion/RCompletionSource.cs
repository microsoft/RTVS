using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Completion
{
    using Definitions;
    using Languages.Core.Text;
    using Engine;
    using Completion = Microsoft.VisualStudio.Language.Intellisense.Completion;
    using Languages.Editor.Completion;

    /// <summary>
    /// Provides actual content for the intellisense dropdown
    /// </summary>
    public class RCompletionSource : ICompletionSource
    {
        internal static readonly string CompletionTypeKey = "RCompletionType";
        internal static readonly string _asyncIntellisenseSession = "Async R Intellisense Session";

        private static readonly char[] _codeChars = new char[] { ' ', '<', '>', '(', ')', '{', '}', ':', '\\', '/' };

        private ITextBuffer _textBuffer;
        private ICompletionSession _asyncSession;

        public RCompletionSource(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
            _textBuffer.Changed += OnTextBufferChanged;
        }

        /// <summary>
        /// Primary entry point for intellisense. This is where intellisense list is getting created.
        /// </summary>
        /// <param name="session">Completion session</param>
        /// <param name="completionSets">Completion sets to populate</param>
        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            Debug.Assert(EditorShell.IsUIThread);

            if (_asyncSession != null)
                return;

            EditorDocument doc = EditorDocument.FromTextBuffer(_textBuffer);
            if (doc == null)
                return;

            int position = session.GetTriggerPoint(_textBuffer).GetPosition(_textBuffer.CurrentSnapshot);

            // If document changed but hasn't been parsed yet start async session
            bool documentDirty = false; // doc.HtmlEditorTree.IsDirty
            if (documentDirty)
            {
                IGlyphService glyphService = EditorShell.ExportProvider.GetExport<IGlyphService>().Value;

                List<Completion> completions = new List<Completion>();

                completions.Add(
                    new Completion(Resources.AsyncIntellisense,
                            String.Empty, String.Empty,
                            glyphService.GetGlyph(StandardGlyphGroup.GlyphInformation, StandardGlyphItem.GlyphItemPublic),
                            String.Empty));

                ITrackingSpan trackingSpan = _textBuffer.CurrentSnapshot.CreateTrackingSpan(new Span(position, 0), SpanTrackingMode.EdgeInclusive);

                CompletionSet completionSet = new CompletionSet(
                    null,
                    null,
                    trackingSpan,
                    completions,
                    null); // builders (none yet)

                completionSets.Add(completionSet);

                Debug.Assert(_asyncSession == null, "We should not be adding async session to existing completion session");

                _asyncSession = session;
                _asyncSession.Properties.AddProperty(_asyncIntellisenseSession, String.Empty);

                //doc.HtmlEditorTree.ProcessChangesAsync(TreeUpdatedCallback);
            }
            else
            {
                PopulateCompletionList(position, session, completionSets);
            }
        }

        private void TreeUpdatedCallback()
        {
            ICompletionSession session = _asyncSession;
            _asyncSession = null;

            if (session == null || session.Properties == null || !session.Properties.ContainsProperty(_asyncIntellisenseSession))
            {
                return;
            }

            RCompletionController controller = ServiceManager.GetService<RCompletionController>(session.TextView);
            if (controller != null)
            {
                if (!session.IsDismissed)
                    controller.DismissCompletionSession();

                controller.ShowCompletion(autoShownCompletion: true);
                controller.FilterCompletionSession();
            }
        }

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            DismissAsyncSession();
        }

        private void DismissAsyncSession()
        {
            if (_asyncSession != null && _asyncSession.Properties != null && _asyncSession.Properties.ContainsProperty(_asyncIntellisenseSession) && !_asyncSession.IsDismissed)
            {
                RCompletionController controller = ServiceManager.GetService<RCompletionController>(_asyncSession.TextView);
                if (controller != null)
                    controller.DismissCompletionSession();
            }

            _asyncSession = null;
        }

        private void PopulateCompletionList(int position, ICompletionSession session, IList<CompletionSet> completionSets)
        {
            // If we ever get called on a background thread, something is drastically wrong.
            Debug.Assert(EditorShell.IsUIThread);

            EditorDocument doc = EditorDocument.FromTextBuffer(_textBuffer);
            if (doc == null)
                return;

            bool autoShownCompletion = true;
            if (session.TextView.Properties.ContainsProperty(CompletionController.AutoShownCompletion))
                autoShownCompletion = session.TextView.Properties.GetProperty<bool>(CompletionController.AutoShownCompletion);

            IReadOnlyCollection<IRCompletionListProvider> providers =
                RCompletionEngine.GetCompletionForLocation(doc.EditorTree.AstRoot, position, autoShownCompletion);

            Span applicableSpan = GetApplicableSpan(position, session);
            ITrackingSpan trackingSpan = _textBuffer.CurrentSnapshot.CreateTrackingSpan(applicableSpan, SpanTrackingMode.EdgeInclusive);
            List<RCompletion> completions = new List<RCompletion>();
            RCompletionContext context = new RCompletionContext();

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
        protected virtual void Dispose(bool disposing)
        {
            if (_textBuffer != null)
            {
                _textBuffer.Changed -= OnTextBufferChanged;
                _textBuffer = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
