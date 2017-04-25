// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.ViewModel;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Document {
    /// <summary>
    /// Factory for Markdown language editor document
    /// </summary>
    [Export(typeof(IEditorViewModelFactory))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    public class MdEditorDocumentFactory : IEditorViewModelFactory {
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public MdEditorDocumentFactory(ICoreShell shell) {
            _shell = shell;
        }

        public IEditorViewModel CreateEditorViewModel(IEditorBuffer editorBuffer) {
            return new MdEditorDocument(editorBuffer, _shell.Services);
        }
    }
}
