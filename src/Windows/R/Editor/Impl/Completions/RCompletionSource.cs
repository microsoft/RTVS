// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Completions.Engine;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Completions {
    /// <summary>
    /// Provides actual content for the intellisense dropdown
    /// </summary>
    public sealed class RCompletionSource : ICompletionSource {
        private readonly RCompletionEngine _completionEngine;
        private readonly IServiceContainer _services;
        private readonly ITextBuffer _textBuffer;

        public RCompletionSource(ITextBuffer textBuffer, IServiceContainer services) {
            _textBuffer = textBuffer;
            _services = services;
            _completionEngine = new RCompletionEngine(services);
        }

        /// <summary>
        /// Primary entry point for intellisense. This is where intellisense list is getting created.
        /// </summary>
        /// <param name="session">Completion session</param>
        /// <param name="completionSets">Completion sets to populate</param>
        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets) {
            _services.MainThread().Assert();

            var doc = _textBuffer.GetEditorDocument<IREditorDocument>();
            if (doc == null) {
                return;
            }

            var position = session.GetTriggerPoint(_textBuffer).GetPosition(_textBuffer.CurrentSnapshot);
            if (!doc.EditorTree.IsReady) {
                var textView = session.TextView;
                doc.EditorTree.InvokeWhenReady((o) => {
                    var controller = CompletionController.FromTextView<RCompletionController>(textView);
                    if (controller != null) {
                        controller.ShowCompletion(autoShownCompletion: true);
                        controller.FilterCompletionSession();
                    }
                }, null, GetType(), processNow: true);
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
            var textViewProperties = session.TextView.Properties;
            if (textViewProperties.TryGetProperty(RCompletionController.IsRHistoryRequest, out bool isRHistoryRequest)) {
                textViewProperties.RemoveProperty(RCompletionController.IsRHistoryRequest);
            }

            var context = new RIntellisenseContext(new EditorIntellisenseSession(session, _services)
                , _textBuffer.ToEditorBuffer()
                , ast
                , position
                , isRHistoryRequest: isRHistoryRequest);
            var providers = _completionEngine.GetCompletionForLocation(context);

            // Position is in R as is the applicable spa, so no need to map down
            var applicableSpan = GetApplicableSpan(position, session);
            if (applicableSpan.HasValue) {
                var snapshot = context.EditorBuffer.CurrentSnapshot.As<ITextSnapshot>();
                var trackingSpan = snapshot.CreateTrackingSpan(applicableSpan.Value, SpanTrackingMode.EdgeInclusive);
                var completions = new List<ICompletionEntry>();
                var sort = true;

                foreach (var provider in providers) {
                    var entries = provider.GetEntries(context);
                    Debug.Assert(entries != null);

                    if (entries.Count > 0) {
                        completions.AddRange(entries);
                    }
                    sort &= provider.AllowSorting;
                }

                if (sort) {
                    completions.Sort(new CompletionEntryComparer(StringComparison.OrdinalIgnoreCase));
                    completions.RemoveDuplicates(new CompletionEntryComparer(StringComparison.Ordinal));
                }

                completionSets.Add(new RCompletionSet(trackingSpan, completions));
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

            var snapshot = _textBuffer.CurrentSnapshot;
            var line = snapshot.GetLineFromPosition(position);
            var lineText = snapshot.GetText(line.Start, line.Length);
            var linePosition = position - line.Start;

            var start = 0;
            var end = line.Length;

            for (var i = linePosition - 1; i >= 0; i--) {
                var ch = lineText[i];
                if (!RTokenizer.IsIdentifierCharacter(ch)) {
                    start = i + 1;
                    break;
                }
            }

            for (var i = linePosition; i < lineText.Length; i++) {
                var ch = lineText[i];
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

        public void Dispose() { }
    }
}
