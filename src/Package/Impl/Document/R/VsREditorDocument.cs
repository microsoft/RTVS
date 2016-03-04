// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Editor.Document;

namespace Microsoft.VisualStudio.R.Package.Document.R {
    internal class VsREditorDocument : REditorDocument {
        private IEditorInstance _editorInstance;

        public VsREditorDocument(IEditorInstance editorInstance) 
            : base(editorInstance.ViewBuffer) {

            _editorInstance = editorInstance;
            ServiceManager.AddService<VsREditorDocument>(this, TextBuffer);
        }

        public override void Close() {
            ServiceManager.RemoveService<VsREditorDocument>(TextBuffer);

            base.Close();

            if (_editorInstance != null) {
                _editorInstance.Dispose();
                _editorInstance = null;
            }
        }
    }
}