// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.LanguageServer.Text {
    internal sealed class EditorSupport : IEditorSupport {
        public ICommandTarget TranslateCommandTarget(IEditorView editorView, object commandTarget) => commandTarget as ICommandTarget;
        public object TranslateToHostCommandTarget(IEditorView editorView, object commandTarget) => commandTarget;
        public IEditorUndoAction CreateUndoAction(IEditorView editorView) => new EditorUndoAction();

        private class EditorUndoAction : IEditorUndoAction {
            public void Dispose() { }
            public void Open(string name) { }
            public void Commit() { }
        }
    }
}
