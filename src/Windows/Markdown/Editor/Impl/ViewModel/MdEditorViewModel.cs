// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.ViewModel;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.Markdown.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.ViewModel {
    public sealed class MdEditorViewModel : ProjectionEditorViewModel {
        public MdEditorViewModel(ITextBuffer diskBuffer, ICoreShell coreShell) :
            base(
                new MdEditorDocument(diskBuffer, coreShell),
                coreShell.GetService<ITextDocumentFactoryService>()
            ) { }

        #region IEditorInstance
        /// <summary>
        /// Retrieves editor instance command target for a particular view
        /// </summary>
        public override ICommandTarget GetCommandTarget(IEditorView editorView) => MdMainController.FromTextView(editorView.As<ITextView>());
        #endregion
    }
}