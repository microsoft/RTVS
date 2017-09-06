// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class TextDocumentMock : ITextDocument {
        public TextDocumentMock(ITextBuffer textBuffer, string filePath) {
            TextBuffer = textBuffer;
            FilePath = filePath;
        }

        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public string FilePath { get; private set; }
        public bool IsDirty => false;
        public bool IsReloading => false;
        public DateTime LastContentModifiedTime => DateTime.Now;
        public DateTime LastSavedTime => DateTime.Now;
        public ITextBuffer TextBuffer { get; }

        public void Dispose() { }
        public ReloadResult Reload() => ReloadResult.Succeeded;
        public ReloadResult Reload(EditOptions options) => ReloadResult.Succeeded;

        public void Rename(string newFilePath) {
            FilePath = newFilePath;
        }

        public void Save() { }

        public void SaveAs(string filePath, bool overwrite) {
            FilePath = filePath;
        }

        public void SaveAs(string filePath, bool overwrite, IContentType newContentType) {
            FilePath = filePath;
        }

        public void SaveAs(string filePath, bool overwrite, bool createFolder) {
            FilePath = filePath;
        }

        public void SaveAs(string filePath, bool overwrite, bool createFolder, IContentType newContentType) {
            FilePath = filePath;
        }

        public void SaveCopy(string filePath, bool overwrite) {
            FilePath = filePath;
        }

        public void SaveCopy(string filePath, bool overwrite, bool createFolder) {
            FilePath = filePath;
        }

        public void SetEncoderFallback(EncoderFallback fallback) { }
        public void UpdateDirtyState(bool isDirty, DateTime lastContentModifiedTime) { }

#pragma warning disable 67
        public event EventHandler DirtyStateChanged;
        public event EventHandler<EncodingChangedEventArgs> EncodingChanged;
        public event EventHandler<TextDocumentFileActionEventArgs> FileActionOccurred;
    }
}
