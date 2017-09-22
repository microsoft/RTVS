// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Languages.Editor.Completions {
    /// <summary>
    /// Represents entry in the editor completion list
    /// </summary>
    public interface ICompletionEntry {
        /// <summary>
        /// Text displayed in the completion list
        /// </summary>
        string DisplayText { get; }

        /// <summary>
        /// Actual text inserted upon completion
        /// </summary>
        string InsertionText { get; }

        /// <summary>
        /// Completion entry description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Accessible text for screen readers
        /// </summary>
        string AccessibleText { get; }

        /// <summary>
        /// Optional icon to display in the list. Typically reflects
        /// completion entry type (keyword, function, variable, ...)
        /// </summary>
        object ImageSource { get; }

        bool IsVisible { get; }

        /// <summary>
        /// Any associated data
        /// </summary>
        object Data { get; }
    }
}
