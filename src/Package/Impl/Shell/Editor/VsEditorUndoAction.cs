// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.Undo;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public sealed class VsEditorUndoAction : IEditorUndoAction {
        private readonly CompoundUndoAction _undoAction;

        public VsEditorUndoAction(IEditorView editorView, IServiceContainer services) {
            _undoAction = new CompoundUndoAction(editorView, services);
        }
        public void Dispose() => _undoAction.Dispose();
        public void Open(string name) => _undoAction.Open(name);
        public void Commit() => _undoAction.Commit();
    }
}
