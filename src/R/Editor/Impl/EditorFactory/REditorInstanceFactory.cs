// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.EditorFactory {
    [Export(typeof(IEditorFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal class REditorInstanceFactory : IEditorFactory {
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public REditorInstanceFactory(ICoreShell coreShell) {
            _coreShell = coreShell;
        }

        public IEditorInstance CreateEditorInstance(ITextBuffer textBuffer, IEditorDocumentFactory documentFactory, bool projected) {
            if (textBuffer == null) {
                throw new ArgumentNullException(nameof(textBuffer));
            }
            if (documentFactory == null) {
                throw new ArgumentNullException(nameof(documentFactory));
            }
            return new REditorInstance(textBuffer, documentFactory, _coreShell, projected);
        }
    }
}
