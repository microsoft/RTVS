// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Document {
    /// <summary>
    /// Factory for Markdown language editor document
    /// </summary>
    [Export(typeof(IEditorDocumentFactory))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    public class MdDocumentFactory : IEditorDocumentFactory {
        public IEditorDocument CreateDocument(IEditorInstance editorInstance) {
            return new MdEditorDocument(editorInstance.ViewBuffer);
        }
    }
}
