// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class TextDataModelMock : ITextDataModel {
        public TextDataModelMock(ITextBuffer textBuffer) {
            ContentType = textBuffer.ContentType;
            DataBuffer = textBuffer;
            DocumentBuffer = textBuffer;
        }

        public IContentType ContentType { get; }

        public ITextBuffer DataBuffer { get; }

        public ITextBuffer DocumentBuffer { get; }

#pragma warning disable 67
        public event EventHandler<TextDataModelContentTypeChangedEventArgs> ContentTypeChanged;
    }
}
