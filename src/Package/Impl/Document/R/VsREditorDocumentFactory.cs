// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.R.Editor.ContentType;

namespace Microsoft.VisualStudio.R.Package.Document.R {
    [Export(typeof(IVsEditorDocumentFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal sealed class VsREditorDocumentFactory : IVsEditorDocumentFactory {
        public IEditorDocument CreateDocument(IEditorInstance editorInstance) {
            return new VsREditorDocument(editorInstance);
        }
    }
}
