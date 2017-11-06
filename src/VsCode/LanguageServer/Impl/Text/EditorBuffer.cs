// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.LanguageServer.Services;
using Microsoft.R.LanguageServer.Services.Editor;

namespace Microsoft.R.LanguageServer.Text {
    internal sealed class EditorBuffer : ServiceAndPropertyHolder, IEditorBuffer, IDisposable {
        private readonly StringBuilder _content;

        private int _version;
        private IEditorBufferSnapshot _currentSnapshot;

        public EditorBuffer(string content, string contentType) {
            _content = new StringBuilder(content, content.Length * 2);
            ContentType = contentType;
        }

        // This is VS-only option
        public T As<T>() where T : class => throw new NotSupportedException();

        public string ContentType { get; }

        public IEditorBufferSnapshot CurrentSnapshot =>
            _currentSnapshot ?? (_currentSnapshot = new EditorBufferSnapshot(this, _content.ToString(), _version));

        public event EventHandler<TextChangeEventArgs> ChangedHighPriority;
        public event EventHandler<TextChangeEventArgs> Changed;
        public event EventHandler Closing;

        public string FilePath => string.Empty;

        public T GetEditorDocument<T>() where T : class, IEditorDocument => Services.GetService<T>();

        public bool Insert(int position, string text) {
            _content.Insert(position, text);
            FireChanged(position, 0, text.Length);
            return true;
        }

        public bool Replace(ITextRange range, string text) {
            _content.Remove(range.Start, range.Length);
            _content.Insert(range.Start, text);
            FireChanged(range.Start, range.Length, text.Length);
            return true;
        }

        public bool Delete(ITextRange range) {
            _content.Remove(range.Start, range.Length);
            FireChanged(range.Start, range.Length, 0);
            return true;
        }

        public void Dispose() => Closing?.Invoke(this, EventArgs.Empty);

        private void FireChanged(int start, int oldLength, int newLength) {
            var oldTextProvider = new TextStream(CurrentSnapshot.GetText());

            _version++;
            _currentSnapshot = new EditorBufferSnapshot(this, _content.ToString(), _version);

            var newTextProvider = new TextStream(_currentSnapshot.GetText());
            var change = new TextChange(start, oldLength, newLength, oldTextProvider, newTextProvider);
            var args = new TextChangeEventArgs(change);

            ChangedHighPriority?.Invoke(this, args);
            Changed?.Invoke(this, args);
        }
    }
}
