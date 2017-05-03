// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.DragDrop;
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
        private readonly ICoreShell _shell;
        private readonly IRInteractiveWorkflowProvider _workflowProvider;

        [ImportingConstructor]
        public DropHandlerProvider(ICoreShell shell, IRInteractiveWorkflowProvider workflowProvider) {
            _shell = shell;
            _workflowProvider = workflowProvider;
        }

        public IDropHandler GetAssociatedDropHandler(IWpfTextView wpfTextView)
        => wpfTextView.Properties.GetOrCreateSingletonProperty(() => new DropHandler(wpfTextView, _shell.Services, _workflowProvider));
    }
}
