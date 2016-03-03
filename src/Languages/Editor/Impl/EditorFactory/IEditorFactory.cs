// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Workspace;

namespace Microsoft.Languages.Editor.EditorFactory {
    /// <summary>
    /// Editor instance factory. Typically imported via MEF
    /// in the host application editor factory such as in
    /// IVsEditorFactory.CreateEditorInstance.
    /// </summary>
    public interface IEditorFactory {
        /// <summary>
        /// Creates an instance of an editor
        /// </summary>
        /// <param name="workspaceItem">Workspace item that represents document in the workspace</param>
        /// <returns>An editor instance</returns>
        IEditorInstance CreateEditorInstance(object textBuffer, IEditorDocumentFactory documentFactory);
    }
}
