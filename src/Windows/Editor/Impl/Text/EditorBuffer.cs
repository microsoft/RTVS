// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Document;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Text {
    /// <summary>
    /// Implements IEditorBuffer abstraction over Visual Studio text buffer.
    /// </summary>
    public sealed class EditorBuffer : IEditorBuffer {
        private const string Key = "XEditorBuffer";

        private readonly Lazy<PropertyDictionary> _properties = Lazy.Create(() => new PropertyDictionary());
        private readonly Lazy<ServiceManager> _services = Lazy.Create(() => new ServiceManager());
        private readonly ITextBuffer _textBuffer;
        private readonly ITextDocumentFactoryService _textDocumentFactoryService;

        public EditorBuffer(ITextBuffer textBuffer, ITextDocumentFactoryService textDocumentFactoryService = null) {
            Check.ArgumentNull(nameof(textBuffer), textBuffer);

            _textBuffer = textBuffer;
            _textBuffer.ChangedHighPriority += OnTextBufferChangedHighPriority;
            _textBuffer.Changed += OnTextBufferChanged;
            _textBuffer.Properties[Key] = this;

            _textDocumentFactoryService = textDocumentFactoryService;
            if (_textDocumentFactoryService != null) {
                _textDocumentFactoryService.TextDocumentDisposed += OnTextDocumentDisposed;
            }
        }

        public static IEditorBuffer Create(ITextBuffer textBuffer, ITextDocumentFactoryService textDocumentFactoryService)
            => textBuffer.ToEditorBuffer() ?? new EditorBuffer(textBuffer, textDocumentFactoryService);

        public static IEditorBuffer FromTextBuffer(ITextBuffer textBuffer)
            => textBuffer.Properties.TryGetProperty(Key, out IEditorBuffer buffer) ? buffer : null;

        #region IEditorBuffer
        public string ContentType => _textBuffer.ContentType.TypeName;

        public IEditorBufferSnapshot CurrentSnapshot => new EditorBufferSnapshot(_textBuffer.CurrentSnapshot);
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
        public T As<T>() where T : class => _textBuffer as T;

        /// <summary>
        /// Attempts to locate associated editor document. Implementation depends on the platform.
        /// </summary>
        /// <typeparam name="T">Type of the document to locate</typeparam>
        public T GetEditorDocument<T>() where T : class, IEditorDocument => _textBuffer.GetEditorDocument<T>();

        public bool Insert(int position, string text) {
            var snapshot = _textBuffer.CurrentSnapshot;
            var newSnapshot = _textBuffer.Insert(position, text);
            return newSnapshot.Version.VersionNumber > snapshot.Version.VersionNumber;
        }

        public bool Replace(ITextRange range, string text) {
            var snapshot = _textBuffer.CurrentSnapshot;
            var newSnapshot = _textBuffer.Replace(range.ToSpan(), text);
            return newSnapshot.Version.VersionNumber > snapshot.Version.VersionNumber;
        }

        public bool Delete(ITextRange range) {
            var snapshot = _textBuffer.CurrentSnapshot;
            var newSnapshot = _textBuffer.Delete(range.ToSpan());
            return newSnapshot.Version.VersionNumber > snapshot.Version.VersionNumber;
        }

        public event EventHandler<TextChangeEventArgs> ChangedHighPriority;
        public event EventHandler<TextChangeEventArgs> Changed;
        public event EventHandler Closing;
        #endregion

        #region IDisposable
        public void Dispose() {
            _textBuffer.ChangedHighPriority -= OnTextBufferChangedHighPriority;
            _textBuffer.Changed -= OnTextBufferChanged;
            _textBuffer.Properties.RemoveProperty(Key);

            if (_textDocumentFactoryService != null) {
                _textDocumentFactoryService.TextDocumentDisposed -= OnTextDocumentDisposed;
            }
            Closing?.Invoke(this, EventArgs.Empty);

            if (_services.IsValueCreated) {
                _services.Value.Dispose();
            }
        }
        #endregion

        private void OnTextBufferChangedHighPriority(object sender, TextContentChangedEventArgs e) 
            => ChangedHighPriority?.Invoke(this, new TextChangeEventArgs(e.ToTextChange()));

        private void OnTextDocumentDisposed(object sender, TextDocumentEventArgs e) {
            if (e.TextDocument.TextBuffer == _textBuffer) {
                Dispose();
            }
        }

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
            => Changed?.Invoke(this, new TextChangeEventArgs(e.ToTextChange()));
    }
}
