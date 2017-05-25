// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Text {
    /// <summary>
    /// Implementation of <see cref="IEditorView"/> over Visual Studio text editor
    /// </summary>
    public sealed class EditorView : IEditorView {
        private const string Key = "XEditorView";

        private readonly Lazy<PropertyDictionary> _properties = Lazy.Create(() => new PropertyDictionary());
        private readonly Lazy<ServiceManager> _services = Lazy.Create(() => new ServiceManager());
        private readonly ITextView _textView;

        public EditorView(ITextView textView) {
            _textView = textView;
            Selection = new EditorSelection(textView);
            Caret = new ViewCaret(_textView);

            _textView.Properties[Key] = this;
            _textView.Closed += OnClosed;

            EditorBuffer = _textView.TextBuffer.ToEditorBuffer() ?? new EditorBuffer(_textView.TextBuffer);
        }

        public IViewCaret Caret { get; }
        public IEditorBuffer EditorBuffer { get; }
        public PropertyDictionary Properties => _properties.Value;
        public IServiceManager Services => _services.Value;
        public IEditorSelection Selection { get; }
        public T As<T>() where T : class => _textView as T;

        public ISnapshotPoint GetCaretPosition(IEditorBuffer editorBuffer = null) {
            var textBuffer = editorBuffer?.As<ITextBuffer>() ?? _textView.TextBuffer;
            var point = _textView.GetCaretPosition(editorBuffer ?? _textView.TextBuffer.ToEditorBuffer());
            return point.HasValue ? new EditorSnapshotPoint(textBuffer.CurrentSnapshot, point.Value) : null;
        }

        public ISnapshotPoint MapToView(IEditorBufferSnapshot snapshot, int position) {
            var target = _textView.BufferGraph
                .MapUpToBuffer(
                    new SnapshotPoint(snapshot.As<ITextSnapshot>(), position), 
                    PointTrackingMode.Positive, PositionAffinity.Successor, _textView.TextBuffer);
            return target.HasValue ? new EditorSnapshotPoint(_textView.TextBuffer.CurrentSnapshot, target.Value) : null;
        }

        public static IEditorView Create(ITextView textView) 
            => textView.ToEditorView() ?? new EditorView(textView);

        public static IEditorView FromTextView(ITextView textView)
            => textView.Properties.TryGetProperty(Key, out IEditorView view) ? view : null;

        private void OnClosed(object sender, EventArgs e) {
            if (_services.IsValueCreated) {
                _services.Value.Dispose();
            }
        }
    }
}
