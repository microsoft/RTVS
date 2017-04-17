// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Languages.Editor.TaskList {
    public class TasksListItemsChangedEventArgs : EventArgs {
        public ReadOnlyCollection<IEditorTaskListItem> Tasks { get; private set; }

        public TasksListItemsChangedEventArgs(IList<IEditorTaskListItem> tasks) {
            Tasks = new ReadOnlyCollection<IEditorTaskListItem>(tasks);
        }
    }
}
