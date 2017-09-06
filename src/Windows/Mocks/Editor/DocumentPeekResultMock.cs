// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Editor.Mocks {
    public sealed class DocumentPeekResultMock : IDocumentPeekResult {
        private Span _span;

        public DocumentPeekResultMock(IPeekResultDisplayInfo info, string filePath, Span span) {
            _span = span;
            FilePath = filePath;
            DisplayInfo = info;
        }
        public bool CanNavigateTo => true;

        public Guid DesiredEditorGuid => Guid.Empty;

        public IPeekResultDisplayInfo DisplayInfo { get; }

        public IPeekResultDisplayInfo2 DisplayInfo2 {
            get { throw new NotImplementedException(); }
        }

        public string FilePath { get; }

        public IPersistentSpan IdentifyingSpan => new PersistentSpanMock(null, _span, FilePath);

        public ImageMoniker Image => Microsoft.VisualStudio.Imaging.KnownMonikers.AboutBox;

        public bool IsReadOnly => false;

        public object KnownMonikers => null;

        public Action<IPeekResult, object, object> PostNavigationCallback => (a, b, c) => { };

        public IPersistentSpan Span { get; }

        public void Dispose() { }

        public void NavigateTo(object data) { }

#pragma warning disable 67
        public event EventHandler Disposed;
    }
}
