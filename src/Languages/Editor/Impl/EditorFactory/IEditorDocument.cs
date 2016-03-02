// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.Text;
using Microsoft.Languages.Editor.Workspace;

namespace Microsoft.Languages.Editor.EditorFactory {
    /// <summary>
    /// Common interface implemented by editor documents
    /// </summary>
    public interface IEditorDocument : IDisposable {
        /// <summary>
        /// Closes the document
        /// </summary>
        void Close();

        /// <summary>
        /// Fires when document is closing.
        /// </summary>
        event EventHandler<EventArgs> DocumentClosing;

        /// <summary>
        /// Fires when document is activated in the host IDE and its view is getting focus.
        /// </summary>
        event EventHandler<EventArgs> Activated;

        /// <summary>
        /// Fires when document is deactivated in the IDE such as when user switches 
        /// to another tab in a tabbed interface or to another window.
        /// </summary>
        event EventHandler<EventArgs> Deactivated;

        /// <summary>
        /// Document representation in the current workspace.
        /// </summary>
        IWorkspaceItem WorkspaceItem { get; }

        /// <summary>
        /// Document text buffer
        /// </summary>
        ITextBuffer TextBuffer { get; }

        /// <summary>
        /// Workspace (project/site/application).
        /// </summary>
        IWorkspace Workspace { get; }
    }
}
