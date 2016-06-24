// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Document.R {
    [Export(typeof(IVsEditorDocumentFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal sealed class VsREditorDocumentFactory : IVsEditorDocumentFactory {
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public VsREditorDocumentFactory(ICoreShell shell) {
            _shell = shell;
        }

        public IEditorDocument CreateDocument(IEditorInstance editorInstance) {
            return new VsREditorDocument(editorInstance, _shell);
        }
    }
}
