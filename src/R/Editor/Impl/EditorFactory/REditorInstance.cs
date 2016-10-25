// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.R.Components.Controller;
using Microsoft.R.Editor.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.EditorFactory {
    /// <summary>
    /// Represents instance of the editor to the host application
    /// </summary>
    public sealed class REditorInstance : EditorInstance {
        public REditorInstance(ITextBuffer diskBuffer, IEditorDocumentFactory documentFactory, ICoreShell coreShell, bool projected) : 
            base(diskBuffer, documentFactory, coreShell, projected) {
        }

        #region IEditorInstance
        public override ICommandTarget GetCommandTarget(ITextView textView) {
            return RMainController.FromTextView(textView);
        }
        #endregion
    }
}