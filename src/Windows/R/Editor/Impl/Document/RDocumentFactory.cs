// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Document {
    /// <summary>
    /// Factory for R language editor document. Imported via MEF
    /// in the host document creation function. In VS that is
    /// IVsEditorFactory.CreateInstance.
    /// </summary>
    [Export(typeof(IEditorDocumentFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    public class RDocumentFactory : IEditorDocumentFactory {
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public RDocumentFactory(ICoreShell shell) {
            _shell = shell;
        }

        public IEditorDocument CreateDocument(IEditorInstance editorInstance) {
            return new REditorDocument(editorInstance.DiskBuffer, _shell);
        }
    }
}
