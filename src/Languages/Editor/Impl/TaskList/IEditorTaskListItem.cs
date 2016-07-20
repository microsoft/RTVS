// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


namespace Microsoft.Languages.Editor.TaskList {
    /// <summary>
    /// Represents an item in a task list
    /// </summary>
    public interface IEditorTaskListItem {
        /// <summary>
        /// Task item description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Task type: error, warnng, informational.
        /// </summary>
        TaskType TaskType { get; }

        /// <summary>
        /// Line number
        /// </summary>
        int Line { get; }

        /// <summary>
        /// Column number
        /// </summary>
        int Column { get; }

        /// <summary>
        /// Length of the error segment
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Name of the file
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Keyword for help
        /// </summary>
        string HelpKeyword { get; }
    }
}
