// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.UI.Commands;

namespace Microsoft.Languages.Editor.Text {
    /// <summary>
    /// Host for Web editing component. This interface provides 
    /// application-specific services and settings.
    /// </summary>
    public interface IEditorSupport {
        /// <summary>
        /// Provides shim that implements ICommandTarget over 
        /// application-specific command target. For example, 
        /// Visual Studio is using IOleCommandTarget.
        /// </summary>
        /// <param name="editorView">Editor view</param>
        /// <param name="commandTarget">Command target</param>
        /// <returns>Web components compatible command target</returns>
        ICommandTarget TranslateCommandTarget(IEditorView editorView, object commandTarget);

        /// <summary>
        /// Provides application-specific command target.
        /// For example, Visual Studio is using IOleCommandTarget.
        /// </summary>
        /// <param name="editorView">Editor view</param>
        /// <param name="commandTarget">Command target</param>
        /// <returns>Host compatible command target</returns>
        object TranslateToHostCommandTarget(IEditorView editorView, object commandTarget);

        /// <summary>
        /// Creates compound undo action
        /// </summary>
        /// <param name="editorView">Editor view</param>
        /// <returns>Undo action instance</returns>
        IEditorUndoAction CreateUndoAction(IEditorView editorView);
    }
}
