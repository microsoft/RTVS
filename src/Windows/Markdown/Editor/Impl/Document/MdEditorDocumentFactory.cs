// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Document {
    /// <summary>
    /// Factory for Markdown language editor document
    /// </summary>
    [Export(typeof(IEditorDocumentFactory))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    public class MdEditorDocumentFactory : IEditorDocumentFactory {
        private readonly IProjectionBufferFactoryService _projectionBufferFactoryService;
        private readonly IContentTypeRegistryService _contentTypeRegistryService;
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public MdEditorDocumentFactory(IProjectionBufferFactoryService projectionBufferFactoryService, IContentTypeRegistryService contentTypeRegistryService, ICoreShell shell) {
            _projectionBufferFactoryService = projectionBufferFactoryService;
            _contentTypeRegistryService = contentTypeRegistryService;
            _shell = shell;
        }

        public IEditorDocument CreateDocument(IEditorInstance editorInstance) {
            return new MdEditorDocument(editorInstance.DiskBuffer, _projectionBufferFactoryService, _contentTypeRegistryService, _shell);
        }
    }
}
