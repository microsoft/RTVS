// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Projection {
    /// <summary>
    /// Manages the projection buffer for the primary language
    /// </summary>
    public sealed class ProjectionBufferManager : IProjectionBufferManager {
        private class ViewPosition {
            public int? CaretPosition;
            public double? ViewportTop;
        }

        private ViewPosition _savedViewPosition;

        public ProjectionBufferManager(ITextBuffer diskBuffer, IServiceContainer services, string topLevelContentTypeName, string secondaryContentTypeName) {
            DiskBuffer = diskBuffer;

            var projectionBufferFactoryService = services.GetService<IProjectionBufferFactoryService>();
            var contentTypeRegistryService = services.GetService<IContentTypeRegistryService>();

            var contentType = contentTypeRegistryService.GetContentType(topLevelContentTypeName);
            ViewBuffer = projectionBufferFactoryService.CreateProjectionBuffer(null, new List<object>(0), ProjectionBufferOptions.None, contentType);
            EditorBuffer.Create(ViewBuffer, services.GetService<ITextDocumentFactoryService>());

            contentType = contentTypeRegistryService.GetContentType(secondaryContentTypeName);
            ContainedLanguageBuffer = projectionBufferFactoryService.CreateProjectionBuffer(null, new List<object>(0), ProjectionBufferOptions.WritableLiteralSpans, contentType);
            EditorBuffer.Create(ContainedLanguageBuffer, services.GetService<ITextDocumentFactoryService>());

            DiskBuffer.AddService(this);
            ViewBuffer.AddService(this);
        }

        public static IProjectionBufferManager FromTextBuffer(ITextBuffer textBuffer) {
            var pbm = textBuffer.GetService<IProjectionBufferManager>();
            if (pbm == null) {
                var pb = textBuffer as IProjectionBuffer;
                pbm = pb?.SourceBuffers?.Select(b => b.GetService<IProjectionBufferManager>())?.FirstOrDefault(b => b != null);
            }
            return pbm;
        }

        #region IProjectionBufferManager
        //  Graph:
        //      View Buffer [ContentType = RMD Projection]
        //        |      \
        //        |    Secondary [ContentType = R]
        //        |      /
        //       Disk Buffer [ContentType = RMD]

        public IProjectionBuffer ViewBuffer { get; }
        public IProjectionBuffer ContainedLanguageBuffer { get; }
        public ITextBuffer DiskBuffer { get; }

        public event EventHandler MappingsChanged;

        public void SetProjectionMappings(string secondaryContent, IReadOnlyList<ProjectionMapping> mappings) {
            // Changing projections can move caret to a visible area unexpectedly.
            // Save caret position so we can place it at the same location when 
            // projections are re-established.
            SaveViewPosition();

            var secondarySpans = CreateSecondarySpans(secondaryContent, mappings);
            if (IdenticalSpans(secondarySpans)) {
                return;
            }

            // While we are changing mappings map everything to the view
            mappings = mappings ?? new List<ProjectionMapping>();
            MapEverythingToView();

            // Now update language spans
            ContainedLanguageBuffer.ReplaceSpans(0, ContainedLanguageBuffer.CurrentSnapshot.SpanCount, secondarySpans, EditOptions.DefaultMinimalChange, this);
            if (secondarySpans.Count > 0) {
                // Update primary (view) buffer projected spans. View buffer spans are all tracking spans:
                // they either come from primary content or secondary content. Inert spans do not participate.
                var viewSpans = CreateViewSpans(mappings);
                if (viewSpans.Count > 0) {
                    ViewBuffer.ReplaceSpans(0, ViewBuffer.CurrentSnapshot.SpanCount, viewSpans, EditOptions.DefaultMinimalChange, this);
                }
            }

            RestoreViewPosition();
            MappingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MapEverythingToView() {
            var diskSnap = DiskBuffer.CurrentSnapshot;
            var everything = new SnapshotSpan(diskSnap, 0, diskSnap.Length);
            var trackingSpan = diskSnap.CreateTrackingSpan(everything, SpanTrackingMode.EdgeInclusive);
            ViewBuffer.ReplaceSpans(0, ViewBuffer.CurrentSnapshot.SpanCount, new List<object>() { trackingSpan }, EditOptions.None, this);
        }

        public void Dispose() {
            DiskBuffer?.RemoveService(this);
            ViewBuffer?.RemoveService(this);
        }
        #endregion

        private List<object> CreateSecondarySpans(string secondaryText, IReadOnlyList<ProjectionMapping> mappings) {
            var spans = new List<object>(mappings.Count);

            var secondaryIndex = 0;
            Span span;

            for (var i = 0; i < mappings.Count; i++) {
                var mapping = mappings[i];
                if (mapping.Length > 0) {
                    span = Span.FromBounds(secondaryIndex, mapping.ProjectionRange.Start);
                    if (!span.IsEmpty) {
                        spans.Add(secondaryText.Substring(span.Start, span.Length)); // inert
                    }
                    span = new Span(mapping.SourceStart, mapping.Length);
                    // Active span comes from the disk buffer
                    spans.Add(new CustomTrackingSpan(DiskBuffer.CurrentSnapshot, span, PointTrackingMode.Positive, PointTrackingMode.Positive)); // active
                    secondaryIndex = mapping.ProjectionRange.End;
                }
            }

            // Add the final inert text after the last span
            span = Span.FromBounds(secondaryIndex, secondaryText.Length);
            if (!span.IsEmpty) {
                spans.Add(secondaryText.Substring(span.Start, span.Length)); // inert
            }
            return spans;
        }

        private List<object> CreateViewSpans(IReadOnlyList<ProjectionMapping> mappings) {
            var spans = new List<object>(mappings.Count);
            var diskSnapshot = DiskBuffer.CurrentSnapshot;
            var primaryIndex = 0;
            Span span;

            for (var i = 0; i < mappings.Count; i++) {
                var mapping = mappings[i];
                if (mapping.Length > 0) {
                    span = Span.FromBounds(primaryIndex, mapping.SourceStart);
                    spans.Add(new CustomTrackingSpan(diskSnapshot, span, i == 0 ? PointTrackingMode.Negative : PointTrackingMode.Positive, PointTrackingMode.Positive)); // Markdown
                    primaryIndex = mapping.SourceRange.End;

                    span = new Span(mapping.ProjectionStart, mapping.Length);
                    spans.Add(new CustomTrackingSpan(ContainedLanguageBuffer.CurrentSnapshot, span, PointTrackingMode.Positive, PointTrackingMode.Positive)); // R
                }
            }
            // Add the final section after the last span
            span = Span.FromBounds(primaryIndex, diskSnapshot.Length);
            spans.Add(new CustomTrackingSpan(diskSnapshot, span, PointTrackingMode.Positive, PointTrackingMode.Positive)); // Markdown
            return spans;
        }

        private ITextCaret GetCaret() => DiskBuffer.GetFirstView()?.Caret;
        private int? GetCaretPosition() => GetCaret()?.Position.BufferPosition.Position;

        private void SaveViewPosition() {
            _savedViewPosition = new ViewPosition {
                CaretPosition = GetCaretPosition(),
                ViewportTop = DiskBuffer.GetFirstView()?.ViewportTop
            };
        }

        private void RestoreViewPosition() {
            var textView = DiskBuffer.GetFirstView();
            if(textView == null) {
                _savedViewPosition = null;
                return;
            }

            if (_savedViewPosition?.CaretPosition != null) {
                var caretPosition = GetCaretPosition();
                if (caretPosition.HasValue && caretPosition.Value != _savedViewPosition.CaretPosition) {
                    textView.Caret.MoveTo(new SnapshotPoint(textView.TextBuffer.CurrentSnapshot, _savedViewPosition.CaretPosition.Value));
                }
            }

            if (_savedViewPosition?.ViewportTop != null) {
                textView.ViewScroller.ScrollViewportVerticallyByPixels(textView.ViewportTop - _savedViewPosition.ViewportTop.Value);
            }

            _savedViewPosition = null;
        }

        private bool IdenticalSpans(IReadOnlyList<object> newSpans) {
            var snapshot = ContainedLanguageBuffer.CurrentSnapshot;

            var currentSpans = snapshot.GetSourceSpans();
            if (currentSpans.Count != newSpans.Count) {
                return false;
            }

            for (var i = 0; i < currentSpans.Count; i++) {
                var cs = currentSpans[i];
                var currentText = cs.Snapshot.GetText(cs.Span);
                string newText;

                var ts = newSpans[i] as ITrackingSpan;
                if (ts != null) {
                    var newSpan = ts.GetSpan(ts.TextBuffer.CurrentSnapshot);
                    if (newSpan.Length != cs.Length || newSpan.Start.Position != cs.Start.Position) {
                        return false;
                    }

                    newText = ts.TextBuffer.CurrentSnapshot.GetText(newSpan);
                } else {
                    // New span is inert text
                    newText = newSpans[i] as string;
                    if(newText == null) {
                        return false;
                    }
                }

                if (!newText.EqualsOrdinal(currentText)) {
                    return false;
                }
            }
            return true;
        }
    }
}
