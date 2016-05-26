// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Document;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Document.Markdown {
    [Export(typeof(IVsEditorDocumentFactory))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal sealed class VsMdEditorDocumentFactory : MdEditorDocumentFactory, IVsEditorDocumentFactory { }
}
