// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.Editor.Test.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class EditorSupportMock : IEditorSupport {
        public ICommandTarget TranslateCommandTarget(IEditorView editorView, object commandTarget) => commandTarget as ICommandTarget;
        public object TranslateToHostCommandTarget(IEditorView editorView, object commandTarget) => commandTarget;
        public IEditorUndoAction CreateUndoAction(IEditorView editorView) => new EditorUndoActionMock();
    }
}