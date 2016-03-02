// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Document
{
    /// <summary>
    /// Factory for R language editor document. Imported via MEF
    /// in the host document creation function. In VS that is
    /// IVsEditorFactory.CreateInstance.
    /// </summary>
    [Export(typeof(IEditorDocumentFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    public class DocumentFactory : IEditorDocumentFactory
    {
        public IEditorDocument CreateDocument(IEditorInstance editorInstance)
        {
            var document =  new REditorDocument(editorInstance.ViewBuffer, editorInstance.WorkspaceItem);
            return document;
        }
    }
}
