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
    using Completion = Microsoft.VisualStudio.Language.Intellisense.Completion;

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

            REditorDocument doc = REditorDocument.FromTextBuffer(_textBuffer);
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

            REditorDocument doc = REditorDocument.FromTextBuffer(_textBuffer);
            if (doc == null)
                return;

            //RCompletionEngine completionEngine = ServiceManager.GetService<RCompletionEngine>(_textBuffer);
            //if (!completionEngine.IsLoaded)
            //    return;

            List<RCompletion> completions = new List<RCompletion>();
            ITextRange range = TextRange.EmptyRange;
            Span itemSpan = new Span(range.Start, range.Length);

            ITrackingSpan trackingSpan = _textBuffer.CurrentSnapshot.CreateTrackingSpan(itemSpan, SpanTrackingMode.EdgeInclusive);

            IList<IRCompletionListProvider> providers = new List<IRCompletionListProvider>();
            // completionEngine.GetCompletionForLocation(position, ..., out range, ...);

            RCompletionContext context = new RCompletionContext();

            foreach (IRCompletionListProvider provider in providers)
            {
                IList<RCompletion> entries = provider.GetEntries(context);
                Debug.Assert(entries != null);

                if (entries.Count > 0)
                {
                    if (session.Properties != null)
                    {
                        if (!session.Properties.ContainsProperty(CompletionTypeKey))
                        {
                            session.Properties.AddProperty(CompletionTypeKey, provider.CompletionType);
                        }
                    }

                    completions.AddRange(entries);
                }
            }

            RemoveDuplicateEntriesAndSort(completions);

            // Currently all providers will always be of the same completion type.
            string completionType = providers.Count > 0 ? providers[0].CompletionType : CompletionTypes.None;

            //foreach (Lazy<IHtmlCompletionListFilter, IContentTypesAndHtmlCompletionFilterAttributes> lazyFilter in _completionListFilters)
            //{
            //    if (lazyFilter.Metadata.CompletionType == completionType)
            //    {
            //        lazyFilter.Value.FilterCompletionList(completions, context);
            //    }
            //}

            // Re-do this, since filters may have added or replaced items
            RemoveDuplicateEntriesAndSort(completions);

            RCompletionSet completionSet = new RCompletionSet(
                trackingSpan,
                completions,
                Enumerable.Empty<RCompletion>(), // builders (none yet)
                context,
                providers);

            completionSets.Add(completionSet);
        }

        private void RemoveDuplicateEntriesAndSort(List<RCompletion> completions)
        {
            // This initial sort allows removal of items with duplicated text
            completions.Sort(RCompletion.Compare);

            int lastFilledIndex = 0;
            for (int i = 1; i < completions.Count; i++)
            {
                RCompletion curCompletion = completions[i];
                RCompletion lastFilledCompletion = completions[lastFilledIndex];

                if (!completions[i].DisplayText.Equals(completions[lastFilledIndex].DisplayText, StringComparison.Ordinal))
                {
                    // The DisplayText differs, make sure the current completion is added
                    completions[++lastFilledIndex] = curCompletion;
                }
                else
                {
                    //if (curCompletion.IconAutomationText.Equals(HtmlIconAutomationText.SnippetIconText, StringComparison.Ordinal) &&
                    //    !lastFilledCompletion.IconAutomationText.Equals(HtmlIconAutomationText.SnippetIconText, StringComparison.Ordinal))
                    //{
                    //    // If the current completion is a snippet and the last filled one isn't, replace the last filled entry
                    //    completions[lastFilledIndex] = curCompletion;
                    //}
                }
            }

            int firstUnfilledIndex = lastFilledIndex + 1;
            if (firstUnfilledIndex < completions.Count)
            {
                completions.RemoveRange(firstUnfilledIndex, completions.Count - firstUnfilledIndex);
            }

            // This sort puts the entries in order based on their SortingPriority
            completions.Sort();
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
