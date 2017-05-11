// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Outline {
    internal sealed class RSectionsCollection : IDisposable {
        private class SpanContent {
            public ITrackingSpan TrackingSpan;
            public int OriginalLength;
        }
        private readonly List<SpanContent> _spans = new List<SpanContent>();
        private readonly IREditorTree _tree;
        private bool _changed;

        public RSectionsCollection(IREditorTree tree, IReadOnlyList<ITextRange> sections) {
            _tree = tree;
            _tree.EditorBuffer.Changed += OnTextBufferChanged;

            foreach (var s in sections) {
                var span = s.ToSpan();
                _spans.Add(new SpanContent() {
                    TrackingSpan = _tree.TextSnapshot().CreateTrackingSpan(span, SpanTrackingMode.EdgePositive),
                    OriginalLength = s.Length
                });
            }
        }

        private void OnTextBufferChanged(object sender, TextChangeEventArgs e) {
            if (!_changed && _tree.AstRoot != null) {
                var c = e.Change;
                if (c.OldText.IndexOf('-', 0) >= 0 || c.NewText.IndexOf('-', 0) >= 0) {
                    if (_tree.AstRoot.Comments.GetItemContaining(c.Start) >= 0) {
                        _changed = true;
                    }
                }
            }
        }

        public bool Changed {
            get {
                if (!_changed) {
                    try {
                        foreach (var s in _spans) {
                            var start = s.TrackingSpan.GetStartPoint(_tree.TextSnapshot());
                            var end = s.TrackingSpan.GetEndPoint(_tree.TextSnapshot());
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
        public void Dispose() => _tree.EditorBuffer.Changed -= OnTextBufferChanged;
    }
}
