// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Languages.Core.Text {
    public interface ITextIterator {
        /// <summary>Text length</summary>
        int Length { get; }

        /// <summary>
        /// Retrieves character at a given position. 
        /// Returns 0 if index is out of range. Must not throw.
        /// </summary>
        char this[int position] { get; }
    }
}
