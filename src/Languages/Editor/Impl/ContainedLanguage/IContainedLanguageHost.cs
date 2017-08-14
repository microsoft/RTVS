// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    /// <summary>
    /// Host for a contained language service
    /// </summary>
    public interface IContainedLanguageHost {
        /// <summary>
        /// Full path to the primary document. Typically used by the contained
        /// language syntax check to output correct path in the task list.
        /// </summary>
        string DocumentPath { get; }

        /// <summary>
        /// Sets command target of the contained language editor.
        /// </summary>
        /// <returns>Command target for the contained language to use as a base</returns>
        ICommandTarget SetContainedCommandTarget(IEditorView editorView, ICommandTarget containedCommandTarget);

        /// <summary>
        /// Removes contained command target
        /// </summary>
        /// <param name="editorView">Text view associated with the command target to remove.</param>
        void RemoveContainedCommandTarget(IEditorView editorView);

        /// <summary>
        /// Fires when primary document is closing. After this event certain properties 
        /// like BufferGraph become unavailable and may return null.
        /// </summary>
        event EventHandler<EventArgs> Closing;

        /// <summary>
        /// Determines if secondary language can format given line.
        /// </summary>
        /// <param name="editorView">Text view</param>
        /// <param name="containedLanguageBuffer">Contained language buffer</param>
        /// <param name="lineNumber">Line number in the contained language buffer</param>
        /// <returns></returns>
        bool CanFormatLine(IEditorView editorView, IEditorBuffer containedLanguageBuffer, int lineNumber);
    }
}
