// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Languages.Core.Text {
    /// <summary>
    /// Represents a set of text ranges. It may or may not be actual collection
    /// internally, but it supports shifting its content according to the supplied start
    /// position and an offset.
    /// </summary>
    public interface ICompositeTextRange : ITextRange {
        /// <summary>
        /// Shifts items in collection starting from given position by the specified offset.
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="offset">Offset to shift items by</param>
        void ShiftStartingFrom(int start, int offset);
    }
}
