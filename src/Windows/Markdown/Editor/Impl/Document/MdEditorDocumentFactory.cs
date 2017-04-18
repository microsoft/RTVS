// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Document;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Document {
    /// <summary>
    /// Factory for Markdown language editor document
    /// </summary>
    [Export(typeof(IEditorDocumentFactory))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    public class MdEditorDocumentFactory : IEditorDocumentFactory {
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public MdEditorDocumentFactory(ICoreShell shell) {
            _shell = shell;
        }

        public IEditorDocument CreateDocument(IEditorInstance editorInstance) {
            return new MdEditorDocument(editorInstance.DiskBuffer.As<ITextBuffer>(), _shell);
        }
    }
}
