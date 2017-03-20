// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI.Commands;

namespace Microsoft.Common.Core.UI {
    /// <summary>
    /// Basic shell provides access to services such as 
    /// composition container, export provider, global VS IDE
    /// services and so on.
    /// </summary>
    public interface IUIService {
        /// <summary>
        /// Fires when host application UI theme changed.
        /// </summary>
        event EventHandler<EventArgs> UIThemeChanged;

        /// <summary>
        /// Displays error message in a host-specific UI
        /// </summary>
        void ShowErrorMessage(string message);

        /// <summary>
        /// Shows the context menu with the specified command ID at the specified location
        /// </summary>
        void ShowContextMenu(CommandId commandId, int x, int y, object commandTarget = null);

        /// <summary>
        /// Displays message with specified buttons in a host-specific UI
        /// </summary>
        MessageButtons ShowMessage(string message, MessageButtons buttons, MessageType messageType = MessageType.Information);

        /// <summary>
        /// If the specified file is opened as a document, and it has unsaved changes, save those changes.
        /// </summary>
        /// <param name="fullPath">The full path to the document to be saved.</param>
        /// <returns> The path to which the file was saved. This is either the original path or a new path specified by the user.</returns>
        string SaveFileIfDirty(string fullPath);

        /// <summary>
        /// Informs the environment to update the state of the commands
        /// </summary>
        /// <param name="immediate">True if the update is performed immediately</param>
        void UpdateCommandStatus(bool immediate = false);

        UIColorTheme UIColorTheme { get; }

        IProgressDialog ProgressDialog { get; }
        IFileDialog FileDialog { get; }
    }
}
