// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.DragDrop;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
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
        private readonly IRInteractiveWorkflowProvider _workflowProvider;

        [ImportingConstructor]
        public DropHandlerProvider(IEditorShell editorShell, IRInteractiveWorkflowProvider workflowProvider) {
            _editorShell = editorShell;
            _workflowProvider = workflowProvider;
        }

        public IDropHandler GetAssociatedDropHandler(IWpfTextView wpfTextView) {
            return new DropHandler(wpfTextView, _editorShell, _workflowProvider);
        }
    }
}
