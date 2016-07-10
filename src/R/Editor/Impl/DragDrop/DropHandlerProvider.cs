// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.DragDrop;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Workspace;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.DragDrop;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.DragDrop {
    [Export(typeof(IDropHandlerProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [DropFormat(DataObjectFormats.VSProjectItems)]
    [Name("RDropHandlerProvider")]
    [Order(Before = "DefaultFileDropHandler")]
    internal sealed class DropHandlerProvider : IDropHandlerProvider {
        private readonly IEditorShell _editorShell;
        private readonly IWorkspaceServices _wsps;

        [ImportingConstructor]
        public DropHandlerProvider(IEditorShell editorShell, IWorkspaceServices wsps) {
            _editorShell = editorShell;
            _wsps = wsps;
        }

        public IDropHandler GetAssociatedDropHandler(IWpfTextView wpfTextView) {
            return new DropHandler(wpfTextView, _editorShell, _wsps);
        }
    }
}
