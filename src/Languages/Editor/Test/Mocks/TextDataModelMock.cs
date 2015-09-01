using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Test.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class TextDataModelMock : ITextDataModel
    {
        public TextDataModelMock(ITextBuffer textBuffer)
        {
            ContentType = textBuffer.ContentType;
            DataBuffer = textBuffer;
            DocumentBuffer = textBuffer;
        }

        public IContentType ContentType { get; private set; }

        public ITextBuffer DataBuffer { get; private set; }

        public ITextBuffer DocumentBuffer { get; private set; }

#pragma warning disable 67
        public event EventHandler<TextDataModelContentTypeChangedEventArgs> ContentTypeChanged;
    }
}
