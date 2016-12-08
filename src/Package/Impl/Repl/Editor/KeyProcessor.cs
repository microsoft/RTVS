// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Windows.Input;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Editor {
    internal sealed class ReplKeyProcessor : KeyProcessor {
        private readonly IWpfTextView _textView;
        private readonly IRInteractiveWorkflowProvider _provider;

        public ReplKeyProcessor(IWpfTextView textView, IRInteractiveWorkflowProvider provider) {
            _textView = textView;
            _provider = provider;
        }

        public override void PreviewKeyDown(KeyEventArgs args) {
            var vk = KeyInterop.VirtualKeyFromKey(args.Key);
            // VK_0 to VK_Z, VK_OEM*
            if ((vk >= 0x30 && vk <= 0x5A) || (vk >= 0xBA && vk <= 0xC0) || (vk >= 0xDB && vk <= 0xDF)) {
                var tb = _provider.GetOrCreate()?.ActiveWindow?.InteractiveWindow?.CurrentLanguageBuffer;
                if (tb != null) {
                    var spans = _textView.TextBuffer.GetReadOnlyExtents(new Text.Span(0, _textView.TextBuffer.CurrentSnapshot.Length));
                    var caret = _textView.Caret.Position.BufferPosition;
                    var span = spans.FirstOrDefault(s => s.Contains(caret.Position));
                    if (span != default(Span)) {
                        try {
                            var viewPoint = _textView.BufferGraph.MapUpToBuffer(new SnapshotPoint(tb.CurrentSnapshot, tb.CurrentSnapshot.Length), PointTrackingMode.Positive, PositionAffinity.Predecessor, _textView.TextBuffer);
                            if (viewPoint.HasValue) {
                                _textView.Caret.MoveTo(viewPoint.Value);
                            }
                        } catch(ArgumentException) { }
                    }
                }
            }
            base.PreviewKeyDown(args);
        }
    }
}
