// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Languages.Editor.Undo {
    /// <summary>
    /// Allows text buffer changes to be batched together in a single named action.
    /// This applies to a single text buffer. When action is disposed it either 
    /// places undo unit on the stack or aborts it depending of if <see cref="Commit"/>
    /// was called or not.
    /// </summary>
    public interface ICompoundUndoAction: IDisposable {
        /// <summary>
        /// Opens undo action that will be including all changes
        /// to the text buffer until disposed.
        /// </summary>
        /// <param name="name">Name of the action as it should appear in the Undo menu</param>
        void Open(string name);

        /// <summary>
        /// Marks action as successfull. Dispose will place the undo unit on the undo stack.
        /// </summary>
        void Commit();
    }

    public interface ICompoundUndoActionOptions {
        // These can only be called between Open and Close
        void SetMergeDirections(bool mergePrevious, bool mergeNext);
        void SetUndoAfterClose(bool undoAfterClose);
    }
}
