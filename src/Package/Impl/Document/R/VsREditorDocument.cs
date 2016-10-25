// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Editor.Document;

namespace Microsoft.VisualStudio.R.Package.Document.R {
    internal class VsREditorDocument : REditorDocument {
        private IEditorInstance _editorInstance;

        public VsREditorDocument(IEditorInstance editorInstance, ICoreShell shell)
            : base(editorInstance.ViewBuffer, shell, editorInstance.IsProjected) {

            _editorInstance = editorInstance;
            ServiceManager.AddService<VsREditorDocument>(this, TextBuffer, shell);
        }

        public override void Close() {
            ServiceManager.RemoveService<VsREditorDocument>(TextBuffer);
            base.Close();

            // Prevent stack overflow
            var instance = _editorInstance;
            _editorInstance = null;
            instance?.Dispose();
        }
    }
}
