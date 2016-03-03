// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.Markdown.Editor.Document;

namespace Microsoft.VisualStudio.R.Package.Document.Markdown {
    internal class VsMdEditorDocument : MdEditorDocument {
        private IEditorInstance _editorInstance;

        public VsMdEditorDocument(IEditorInstance editorInstance)
            : base(editorInstance.ViewBuffer) {

            _editorInstance = editorInstance;
            ServiceManager.AddService<VsMdEditorDocument>(this, TextBuffer);
        }

        public override void Close() {
            ServiceManager.RemoveService<VsMdEditorDocument>(TextBuffer);
            base.Close();

            _editorInstance?.Dispose();
            _editorInstance = null;
        }
    }
}