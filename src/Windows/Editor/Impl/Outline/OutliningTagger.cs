// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.Languages.Editor.Outline {
    public class OutliningTagger : ITagger<IOutliningRegionTag> {
        private OutlineRegionCollection _currentRegions;
        protected OutlineRegionBuilder _regionBuilder;
        private ITextBuffer _textBuffer;

        public OutliningTagger(ITextBuffer textBuffer, OutlineRegionBuilder regionBuilder) {
            _textBuffer = textBuffer;
            _regionBuilder = regionBuilder;
            _regionBuilder.RegionsChanged += OnRegionsChanged;
        }

        #region ITagger<IOutliningRegionTag>
        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans) {
            if (spans.Count == 0 || _currentRegions == null || _currentRegions.Count == 0) {
                yield break;
            }

            ITextSnapshot snapshot = _textBuffer.CurrentSnapshot;
            SnapshotSpan entire = new SnapshotSpan(spans[0].Start, spans[spans.Count - 1].End).TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive);

            int startPosition = entire.Start.GetContainingLine().Start;
            int endPosition = entire.End.GetContainingLine().End;

            foreach (OutlineRegion region in _currentRegions) {
                int end = Math.Min(region.End, snapshot.Length);

                if (region.Start <= endPosition && end >= startPosition) {
                    yield return new TagSpan<IOutliningRegionTag>(
                        new SnapshotSpan(snapshot, Span.FromBounds(region.Start, end)),
                        CreateTag(region));
                }
            }
        }

        public virtual OutliningRegionTag CreateTag(OutlineRegion region) {
            return new OutliningRegionTag(false, false, region.DisplayText, region.HoverText);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
        #endregion

        private void OnRegionsChanged(object sender, OutlineRegionsChangedEventArgs e) {
            ITextSnapshot snapshot = _textBuffer.CurrentSnapshot;

            if (e.Regions.TextBufferVersion == _textBuffer.CurrentSnapshot.Version.VersionNumber) {
                // Update the regions before firing the notification, as core editor 
                //   may ask for the regions during the notification.
                _currentRegions = e.Regions;

                if (TagsChanged != null) {
                    int start = e.ChangedRange.Start;
                    if (start < snapshot.Length) {
                        int end = Math.Min(e.ChangedRange.End, snapshot.Length);

                        TagsChanged(this, new SnapshotSpanEventArgs(
                            new SnapshotSpan(snapshot, Span.FromBounds(start, end))));
                    }
                }
            }
        }

        public bool IsReady {
            get {
                return _regionBuilder.IsReady;
            }
        }
    }
}
