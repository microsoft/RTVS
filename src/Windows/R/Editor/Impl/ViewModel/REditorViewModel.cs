// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.ViewModel;
using Microsoft.R.Editor.Commands;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.ViewModel {
    /// <summary>
    /// Represents instance of the editor to the host application
    /// </summary>
    public sealed class REditorViewModel : EditorViewModel {
        public REditorViewModel(ITextBuffer diskBuffer, IServiceContainer services) :
            base(new REditorDocument(new EditorBuffer(diskBuffer, services.GetService<ITextDocumentFactoryService>()), services)) { }

        #region IEditorViewModel
        public override ICommandTarget GetCommandTarget(IEditorView editorView) => RMainController.FromTextView(editorView.As<ITextView>());
        #endregion
    }
}