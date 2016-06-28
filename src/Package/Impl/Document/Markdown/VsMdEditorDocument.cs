// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.Markdown.Editor.Document;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Document.Markdown {
    internal class VsMdEditorDocument : MdEditorDocument {
        private readonly IEditorInstance _editorInstance;

        public VsMdEditorDocument(IEditorInstance editorInstance, IProjectionBufferFactoryService projectionBufferFactoryService, IContentTypeRegistryService contentTypeRegistryService, ICoreShell shell)
            : base(editorInstance.ViewBuffer, projectionBufferFactoryService, contentTypeRegistryService, shell) {

            _editorInstance = editorInstance;
            ServiceManager.AddService<VsMdEditorDocument>(this, TextBuffer, shell);
        }

        public override void Close() {
            ServiceManager.RemoveService<VsMdEditorDocument>(TextBuffer);
            base.Close();
            _editorInstance?.Dispose();
        }
    }
}