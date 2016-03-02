// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


namespace Microsoft.Languages.Core.Text {
    /// <summary>
    /// Represents an item that has a range in a text document
    /// </summary>
    public interface ITextRange {
        /// <summary>
        /// Range start.
        /// </summary>
        int Start { get; }

        /// <summary>
        /// Range end.
        /// </summary>
        int End { get; }

        /// <summary>
        /// Length of the range.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Determines if range contains given position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Tru if position is inside the range</returns>
        bool Contains(int position);

        /// <summary>
        /// Shifts range by a given offset.
        /// </summary>
        void Shift(int offset);
    }

    /// <summary>
    /// Represents an item that has a range in a text document
    /// </summary>
    public interface IExpandableTextRange : ITextRange {
        /// <summary>
        /// Changes range boundaries by the given offsets
        /// </summary>
        void Expand(int startOffset, int endOffset);

        /// <summary>
        /// Specifies whether this range is maintained or deleted when shrunk to zero length
        /// </summary>
        bool AllowZeroLength { get; }

        /// <summary>
        /// Specifies whether changes at the start should expand this range
        /// </summary>
        bool IsStartInclusive { get; }

        /// <summary>
        /// Specifies whether changes at the end should expand this range
        /// </summary>
        bool IsEndInclusive { get; }

        /// <summary>
        /// Specifies whether this range contains the specified position using inclusion on the boundaries
        /// </summary>
        bool ContainsUsingInclusion(int position);
    }
}
