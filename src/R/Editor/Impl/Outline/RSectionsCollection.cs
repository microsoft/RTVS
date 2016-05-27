// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Outline {
    internal sealed class RSectionsCollection {
        private class SpanContent {
            public ITrackingSpan TrackingSpan;
            public int OriginalLength;
        }
        private readonly List<SpanContent> _spans = new List<SpanContent>();
        private bool _changed;

        public RSectionsCollection(ITextSnapshot snapshot, IReadOnlyList<ITextRange> sections) {
            foreach (var s in sections) {
                var span = s.ToSpan();
                _spans.Add(new SpanContent() {
                    TrackingSpan = snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgePositive),
                    OriginalLength = s.Length
                });
            }
        }

        public bool Changed(ITextSnapshot snapshot) {
            if (!_changed) {
                try {
                    foreach (var s in _spans) {
                        var start = s.TrackingSpan.GetStartPoint(snapshot);
                        var end = s.TrackingSpan.GetEndPoint(snapshot);
                        var currentLength = end - start;
                        if (currentLength != s.OriginalLength) {
                            _changed = true;
                            break;
                        }
                    }
                } catch (ArgumentException) { } catch (IndexOutOfRangeException) { }
            }
            return _changed;
        }
    }
}
