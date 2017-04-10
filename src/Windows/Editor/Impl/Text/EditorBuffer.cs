// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Text {
    /// <summary>
    /// Implements IEditorBuffer abstraction over Visual Studio text buffer.
    /// </summary>
    public sealed class EditorBuffer : IEditorBuffer {
        private readonly Lazy<PropertyDictionary> _properties = Lazy.Create(() => new PropertyDictionary());
        private readonly Lazy<ServiceManager> _services = Lazy.Create(() => new ServiceManager());
        private readonly ITextBuffer _textBuffer;
        private readonly ITextDocumentFactoryService _textDocumentFactoryService;

        public EditorBuffer(ITextBuffer textBuffer, ITextDocumentFactoryService textDocumentFactoryService) {
            Check.ArgumentNull(nameof(textBuffer), textBuffer);
            Check.ArgumentNull(nameof(textDocumentFactoryService), textDocumentFactoryService);

            _textBuffer = textBuffer;
            _textBuffer.ChangedHighPriority += OnTextBufferChangedHighPriority;
            _textBuffer.Changed += OnTextBufferChanged;
            _textBuffer.Properties[typeof(IEditorBuffer)] = this;

            _textDocumentFactoryService = textDocumentFactoryService;
            _textDocumentFactoryService.TextDocumentDisposed += OnTextDocumentDisposed;
        }

        private void OnTextBufferChangedHighPriority(object sender, TextContentChangedEventArgs e) {
            var changes = e.ConvertToRelative();
            foreach (var ch in changes) {
                ChangedHighPriority?.Invoke(this, ch);
            }
        }

        private void OnTextDocumentDisposed(object sender, TextDocumentEventArgs e) {
            if(e.TextDocument.TextBuffer == _textBuffer) {
                Dispose();
            }
        }

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) {
            var changes = e.ConvertToRelative();
            foreach (var ch in changes) {
                Changed?.Invoke(this, ch);
            }
        }

        public void Dispose() {
            _textBuffer.ChangedHighPriority -= OnTextBufferChangedHighPriority;
            _textBuffer.Changed -= OnTextBufferChanged;
            _textBuffer.Properties.RemoveProperty(typeof(IEditorBuffer));

            _textDocumentFactoryService.TextDocumentDisposed -= OnTextDocumentDisposed;
            Closing?.Invoke(this, EventArgs.Empty);
        }

        public IBufferSnapshot CurrentSnapshot => new EditorBufferSnapshot(this, _textBuffer.CurrentSnapshot);
        public PropertyDictionary Properties => _properties.Value;
        public IServiceManager Services => _services.Value;

        /// <summary>
        /// Path to the file being edited, if any.
        /// </summary>
        public string FilePath => _textBuffer.GetFileName();

        /// <summary>
        /// Returns underlying platform object such as ITextBuffer in Visual Studio.
        /// May return null if there is no underlying implementation.
        /// </summary>
        public T As<T>() where T: class => _textBuffer as T;

        public void Insert(int position, string text) => _textBuffer.Insert(position, text);
        public void Replace(ITextRange range, string text) => _textBuffer.Replace(range.ToSpan(), text);
        public void Delete(ITextRange range) => _textBuffer.Delete(range.ToSpan());

        public event EventHandler<TextChangeEventArgs> ChangedHighPriority;
        public event EventHandler<TextChangeEventArgs> Changed;
        public event EventHandler Closing;
    }
}
