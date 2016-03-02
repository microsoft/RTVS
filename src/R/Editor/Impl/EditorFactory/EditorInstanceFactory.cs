// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Workspace;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.EditorFactory
{
    [Export(typeof(IEditorFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal class EditorInstanceFactory : IEditorFactory
    {
        public IEditorInstance CreateEditorInstance(IWorkspaceItem workspaceItem, object textBuffer, IEditorDocumentFactory documentFactory)
        {
            if (textBuffer == null)
                throw new ArgumentNullException("textBuffer");

            if (documentFactory == null)
                throw new ArgumentNullException("documentFactory");

            if (!(textBuffer is ITextBuffer))
                throw new ArgumentException("textBuffer parameter must be a text buffer");

            if (documentFactory == null)
                documentFactory = new DocumentFactory();

            return new EditorInstance(workspaceItem, textBuffer as ITextBuffer, documentFactory);
        }
    }
}
