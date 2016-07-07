// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.EditorFactory {
    public sealed class MdEditorInstance : ProjectionEditorInstance {
        public MdEditorInstance(ITextBuffer diskBuffer, IEditorDocumentFactory documentFactory, ICoreShell coreShell) : 
            base(diskBuffer, documentFactory, coreShell) {
        }

        #region IEditorInstance
        /// <summary>
        /// Retrieves editor instance command target for a particular view
        /// </summary>
        public override ICommandTarget GetCommandTarget(ITextView textView) {
            return MdMainController.FromTextView(textView);
        }
        #endregion
    }
}