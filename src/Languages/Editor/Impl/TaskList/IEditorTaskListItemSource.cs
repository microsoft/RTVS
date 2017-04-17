// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Languages.Editor.TaskList {
    /// <summary>
    /// Implemented by a language validation engine. 
    /// </summary>
    public interface IEditorTaskListItemSource {
        /// <summary>
        /// Fires when source has new tasks pending addition to the list
        /// </summary>
        event EventHandler<TasksListItemsChangedEventArgs> TasksAdded;

        /// <summary>
        /// Fires when source wants to remove some tasks from the list
        /// </summary>
        event EventHandler<TasksListItemsChangedEventArgs> TasksRemoved;

        /// <summary>
        /// Fires when task sources wants to remove all its tasks from the list
        /// </summary>
        event EventHandler<EventArgs> TasksCleared;

        /// <summary>
        /// Fires when the set of tasks is about to change
        /// </summary>
        event EventHandler<EventArgs> BeginUpdatingTasks;

        /// <summary>
        /// Fires when the set of tasks has finished changing
        /// </summary>
        event EventHandler<EventArgs> EndUpdatingTasks;

        /// <summary>
        /// Fires when the set of tasks has finished updating
        /// </summary>
        event EventHandler<EventArgs> TasksUpdated;

        /// <summary>
        /// Returns all currently active tasks in this source
        /// </summary>
        IReadOnlyCollection<IEditorTaskListItem> Tasks { get; }

        /// <summary>
        /// Text buffer associated with the source. Typically a top level text buffer.
        /// Can be null if source is not a text based document.
        /// </summary>
        object EditorBuffer { get; }
    }
}
