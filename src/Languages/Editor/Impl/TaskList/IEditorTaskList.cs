// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Languages.Editor.TaskList {
    /// <summary>
    /// Primary interface to the editor host application task list
    /// </summary>
    public interface IEditorTaskList {
        /// <summary>
        /// Registers task source with the task list. Typically each document
        /// instance provides a single task source.
        /// </summary>
        /// <param name="source"></param>
        void AddTaskSource(IEditorTaskListItemSource source);

        /// <summary>
        /// Removes task source from the task list.
        /// </summary>
        /// <param name="source"></param>
        void RemoveTaskSource(IEditorTaskListItemSource source);

        /// <summary>
        /// Ensurse that all tasks have been added to the host's task window
        /// </summary>
        void FlushTaskList();

        /// <summary>
        /// Fires when task list is about to close. Task sources should
        /// disconnect from the task list and release associated resources.
        /// </summary>
        event EventHandler<EventArgs> TaskListClosing;
    }
}
