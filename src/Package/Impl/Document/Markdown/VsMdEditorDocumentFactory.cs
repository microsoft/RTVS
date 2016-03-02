// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Markdown.Editor.ContentTypes;

namespace Microsoft.VisualStudio.R.Package.Document.Markdown {
    [Export(typeof(IVsEditorDocumentFactory))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal sealed class VsMdEditorDocumentFactory : IVsEditorDocumentFactory {
        public IEditorDocument CreateDocument(IEditorInstance editorInstance) {
            return new VsMdEditorDocument(editorInstance);
        }
    }
}
