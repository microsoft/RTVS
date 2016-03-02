// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Workspace;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.EditorFactory {
    /// <summary>
    /// Editor instance factory. Typically imported via MEF
    /// in the host application editor factory such as in
    /// IVsEditorFactory.CreateEditorInstance.
    /// </summary>
    [Export(typeof(IEditorFactory))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal class EditorInstanceFactory : IEditorFactory {
        public IEditorInstance CreateEditorInstance(IWorkspaceItem workspaceItem, object textBuffer, IEditorDocumentFactory documentFactory) {
            if (textBuffer == null)
                throw new ArgumentNullException("textBuffer");

            if (documentFactory == null)
                throw new ArgumentNullException("documentFactory");

            if (!(textBuffer is ITextBuffer))
                throw new ArgumentException("textBuffer parameter must be a text buffer");

            if (documentFactory == null)
                documentFactory = new MdDocumentFactory();

            return new EditorInstance(workspaceItem, textBuffer as ITextBuffer, documentFactory);
        }
    }
}
