// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Application.Core
{
    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("ContentType={ContentType}")]
    internal class TextDataModel : ITextDataModel
    {
        public TextDataModel(ITextBuffer diskBuffer, ITextBuffer projectedBuffer)
        {
            DataBuffer = projectedBuffer;
            DocumentBuffer = diskBuffer;
            DocumentBuffer.ContentTypeChanged += DocumentBuffer_ContentTypeChanged;
        }

        private void DocumentBuffer_ContentTypeChanged(object sender, ContentTypeChangedEventArgs e)
        {
            if (ContentTypeChanged != null)
            {
                var ne = new TextDataModelContentTypeChangedEventArgs(e.BeforeContentType, e.AfterContentType);
                ContentTypeChanged(sender, ne);
            }
        }

        public IContentType ContentType // from disk buffer
        {
            get
            {
                return DocumentBuffer.ContentType;
            }
        }

        public event EventHandler<TextDataModelContentTypeChangedEventArgs> ContentTypeChanged;

        public ITextBuffer DataBuffer // disk buffer
        {
            get;
            private set;
        }

        public ITextBuffer DocumentBuffer // projected buffer
        {
            get;
            private set;
        }
    }
}
