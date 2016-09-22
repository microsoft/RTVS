// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
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

namespace Microsoft.R.Editor.Completion {
    using Core.Tokens;
    using Languages.Editor.Services;

    /// <summary>
    /// Provides actual content for the intellisense dropdown
    /// </summary>
    public sealed class RCompletionSource : ICompletionSource {
        private ITextBuffer _textBuffer;

        public RCompletionSource(ITextBuffer textBuffer) {
            _textBuffer = textBuffer;
        }

        /// <summary>
        /// Primary entry point for intellisense. This is where intellisense list is getting created.
        /// </summary>
        /// <param name="session">Completion session</param>
        /// <param name="completionSets">Completion sets to populate</param>
        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets) {
            Debug.Assert(EditorShell.IsUIThread);

            IREditorDocument doc = REditorDocument.TryFromTextBuffer(_textBuffer);
            if (doc == null) {
                return;
            }

            int position = session.GetTriggerPoint(_textBuffer).GetPosition(_textBuffer.CurrentSnapshot);

            if (!doc.EditorTree.IsReady) {
                var textView = session.TextView;
                doc.EditorTree.InvokeWhenReady((o) => {
                    RCompletionController controller = ServiceManager.GetService<RCompletionController>(textView);
                    if (controller != null) {
                        controller.ShowCompletion(autoShownCompletion: true);
                        controller.FilterCompletionSession();
                    }
                }, null, this.GetType(), processNow: true);
            } else {
                PopulateCompletionList(position, session, completionSets, doc.EditorTree.AstRoot);
            }
        }

        /// <summary>
        /// Populates R completion list for a given position
        /// </summary>
        /// <param name="position">Position in R text buffer</param>
        /// <param name="session">Completion session</param>
        /// <param name="completionSets">Completion sets to add to</param>
        /// <param name="ast">Document abstract syntax tree</param>
        internal void PopulateCompletionList(int position, ICompletionSession session, IList<CompletionSet> completionSets, AstRoot ast) {
            RCompletionContext context = new RCompletionContext(session, _textBuffer, ast, position);

            bool autoShownCompletion = true;
            if (session.TextView.Properties.ContainsProperty(CompletionController.AutoShownCompletion))
                autoShownCompletion = session.TextView.Properties.GetProperty<bool>(CompletionController.AutoShownCompletion);

            IReadOnlyCollection<IRCompletionListProvider> providers =
                RCompletionEngine.GetCompletionForLocation(context, autoShownCompletion);

            // Position is in R as is the applicable spa, so no need to map down
            Span? applicableSpan = GetApplicableSpan(position, session);
            if (applicableSpan.HasValue) {
                ITrackingSpan trackingSpan = context.TextBuffer.CurrentSnapshot.CreateTrackingSpan(applicableSpan.Value, SpanTrackingMode.EdgeInclusive);
                List<RCompletion> completions = new List<RCompletion>();
                bool sort = true;

                foreach (IRCompletionListProvider provider in providers) {
                    IReadOnlyCollection<RCompletion> entries = provider.GetEntries(context);
                    Debug.Assert(entries != null);

                    if (entries.Count > 0) {
                        completions.AddRange(entries);
                    }
                    sort &= provider.AllowSorting;
                }

                if (sort) {
                    completions.Sort(RCompletion.CompareOrdinal);
                    completions.RemoveDuplicates(RCompletion.CompareOrdinal);
                }

                CompletionSet completionSet = new RCompletionSet(session.TextView.TextBuffer, trackingSpan, completions);
                completionSets.Add(completionSet);
            }
        }

        /// <summary>
        /// Calculates span in the text buffer that contains data
        /// applicable to the current completion session. A tracking
        /// span will be created over it and editor will grow and shrink
        /// tracking span as user types and filter completion session
        /// based on the data inside the tracking span.
        /// </summary>
        private Span? GetApplicableSpan(int position, ICompletionSession session) {
            var selectedSpans = session.TextView.Selection.SelectedSpans;
            if (selectedSpans.Count == 1 && selectedSpans[0].Span.Length > 0) {
                var spans = session.TextView.MapDownToR(selectedSpans[0]);
                if (spans.Count > 0) {
                    return spans[0].Span;
                }
                return null;
            }

            ITextSnapshot snapshot = _textBuffer.CurrentSnapshot;
            ITextSnapshotLine line = snapshot.GetLineFromPosition(position);
            string lineText = snapshot.GetText(line.Start, line.Length);
            int linePosition = position - line.Start;

            int start = 0;
            int end = line.Length;

            for (int i = linePosition - 1; i >= 0; i--) {
                char ch = lineText[i];
                if (!RTokenizer.IsIdentifierCharacter(ch)) {
                    start = i + 1;
                    break;
                }
            }

            for (int i = linePosition; i < lineText.Length; i++) {
                char ch = lineText[i];
                if (!RTokenizer.IsIdentifierCharacter(ch)) {
                    end = i;
                    break;
                }
            }

            if (start < end) {
                return new Span(start + line.Start, end - start);
            }

            return new Span(position, 0);
        }

        #region Dispose
        public void Dispose() {
            _textBuffer = null;
        }
        #endregion
    }
}
