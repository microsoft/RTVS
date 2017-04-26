// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.Selection {
    /// <summary>
    /// Implemented if particular editor wants to preserve selection and caret position during autoformatting. 
    /// Typically implemented via mapping caret position to one of known elements in the editor parse tree 
    /// or using token stream.
    /// </summary>
    public interface ISelectionTracker {
        /// <summary>
        /// Editor text view
        /// </summary>
        IEditorView EditorView { get; }

        /// <summary>
        /// Saves current caret position and optionally starts tracking 
        /// of the caret position across text buffer changes using ITrackingSpan
        /// </summary>
        /// <param name="automaticTracking">True if selection tracker should track text buffer changes using tracking span. 
        /// False if End should simply use current caret position as final position rather than attempt to track it
        /// across changes.</param>
        void StartTracking(bool automaticTracking);

        /// <summary>
        /// Stops tracking and saves current caret position as final position as 'after changes' position.
        /// </summary>
        void EndTracking();

        /// <summary>
        /// Moves caret to 'before changes' position.
        /// </summary>
        void MoveToBeforeChanges();

        /// <summary>
        /// Moves caret to 'after changes' position
        /// </summary>
        void MoveToAfterChanges(int virtualSpaces = 0);
    }
}
