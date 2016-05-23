// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    /// <summary>
    /// Host for a contained language service
    /// </summary>
    public interface IContainedLanguageHost {
        /// <summary>
        /// Full path to the host document. Typically used by the contained
        /// language syntax check to output correct path in the task list.
        /// </summary>
        string DocumentPath { get; }

        /// <summary>
        /// Sets command target of the contained language editor.
        /// </summary>
        /// <returns>Command target for the contained language to use as a base</returns>
        ICommandTarget SetContainedCommandTarget(ITextView textView, ICommandTarget containedCommandTarget);

        /// <summary>
        /// Removes contained command target
        /// </summary>
        /// <param name="textView">Text view associated with the command target to remove.</param>
        void RemoveContainedCommandTarget(ITextView textView);

        /// <summary>
        /// Fires when host document is closing. After this event certain properties like BufferGraph
        /// become unavailable and may return null.
        /// </summary>
        event EventHandler<EventArgs> Closing;

        bool CanFormatLine(int lineNumber);
    }
}
