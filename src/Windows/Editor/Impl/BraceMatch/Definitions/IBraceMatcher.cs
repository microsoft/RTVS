// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.BraceMatch.Definitions {
    /// <summary>
    /// Generic brace or element matcher. Used by GoTo Brace commands 
    /// typically Ctrl+{, Ctrl+Shift+{ shortcuts.
    /// </summary>
    public interface IBraceMatcher {
        /// <summary>
        /// Retrieves brace positions given text snapshot and a position inside it.
        /// </summary>
        /// <param name="snapshot">Text snapshot</param>
        /// <param name="currentPosition">Caret position in the snapshot</param>
        /// <param name="extendSelection">True if command is 'select block' and false if it is a simple brace match</param>
        /// <param name="startPosition">Start brace position</param>
        /// <param name="endPosition">End brace position</param>
        /// <returns>True if matcher did find something to match, false otherwise</returns>
        bool GetBracesFromPosition(ITextSnapshot snapshot, int currentPosition, bool extendSelection, out int startPosition, out int endPosition);
    }
}
