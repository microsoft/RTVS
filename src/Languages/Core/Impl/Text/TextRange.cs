// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Common.Core.Diagnostics;
using static System.FormattableString;

namespace Microsoft.Languages.Core.Text {
    /// <summary>
    /// Represents a range in a text buffer or a string. Specified start and end of text. 
    /// End is exclusive, i.e. Length = End - Start. Implements IComparable that compares
    /// range start positions. 
    /// </summary>
    [DebuggerDisplay("[{Start}...{End}), Length = {Length}")]
    public class TextRange : IExpandableTextRange {
        /// <summary>
        /// Returns an empty, invalid range.
        /// </summary>
        public static TextRange EmptyRange { get; } = new TextRange(0, 0);

        /// <summary>
        /// Creates text range starting at position 0 and length of 0
        /// </summary>
        [DebuggerStepThrough]
        public TextRange()
            : this(0) {
        }

        /// <summary>
        /// Creates text range starting at given position and length of zero.
        /// </summary>
        /// <param name="position">Start position</param>
        public TextRange(int position) {
            Start = position;
            End = position < int.MaxValue ? position + 1 : position;
        }

        /// <summary>
        /// Creates text range based on start and end positions.
        /// End is exclusive, Length = End - Start
        /// <param name="start">Range start</param>
        /// <param name="length">Range length</param>
        /// </summary>
        [DebuggerStepThrough]
        public TextRange(int start, int length) {
            Check.Argument(nameof(length), () => length >= 0);
            Start = start;
            End = start + length;
        }

        /// <summary>
        /// Creates text range based on another text range
        /// </summary>
        /// <param name="range">Text range to use as position source</param>
        public TextRange(ITextRange range)
            : this(range.Start, range.Length) {
        }

        /// <summary>
        /// Resets text range to (0, 0)
        /// </summary>
        [DebuggerStepThrough]
        public void Empty() {
            Start = 0;
            End = 0;
        }

        /// <summary>
        /// Creates text range based on start and end positions.
        /// End is exclusive, Length = End - Start
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        [DebuggerStepThrough]
        public static TextRange FromBounds(int start, int end) => new TextRange(start, end - start);

        /// <summary>
        /// Finds out of range intersects another range
        /// </summary>
        /// <param name="start">Start of another range</param>
        /// <param name="length">Length of another range</param>
        /// <returns>True if ranges intersect</returns>
        public virtual bool Intersect(int start, int length) => TextRange.Intersect(this, start, length);

        /// <summary>
        /// Finds out of range intersects another range
        /// </summary>
        /// <param name="range">Text range</param>
        /// <returns>True if ranges intersect</returns>
        public virtual bool Intersect(ITextRange range) => TextRange.Intersect(this, range.Start, range.Length);

        /// <summary>
        /// Finds out if range represents valid text range (it's length is greater than zero)
        /// </summary>
        /// <returns>True if range is valid</returns>
        public virtual bool IsValid() => TextRange.IsValid(this);

        #region ITextRange
        /// <summary>
        /// Text range start position
        /// </summary>
        public int Start { get; private set; }

        /// <summary>
        /// Text range end position (excluded)
        /// </summary>
        public int End { get; private set; }

        /// <summary>
        /// Text range length
        /// </summary>
        public int Length => End - Start;

        /// <summary>
        /// Determines if range contains given position
        /// </summary>
        public virtual bool Contains(int position) => TextRange.Contains(this, position);

        /// <summary>
        /// Determines if text range fully contains another range
        /// </summary>
        /// <param name="range"></param>
        public virtual bool Contains(ITextRange range) => Contains(range.Start) && Contains(range.End);

        /// <summary>
        /// Shifts text range by a given offset
        /// </summary>
        [DebuggerStepThrough]
        public virtual void Shift(int offset) {
            Start += offset;
            End += offset;
         }

        public virtual void Expand(int startOffset, int endOffset) {
            if (Start + startOffset > End + endOffset) {
                throw new ArgumentException("Combination of start and end offsets should not be making range invalid");
            }

            Start += startOffset;
            End += endOffset;
        }

        public virtual bool AllowZeroLength => false;
        public virtual bool IsStartInclusive => true;
        public virtual bool IsEndInclusive => false;

        /// <summary>
        /// Determines if range contains given position
        /// </summary>
        public virtual bool ContainsUsingInclusion(int position) {
            if ((position >= Start) && (position <= End)) {
                if (position == Start) {
                    return IsStartInclusive || ((position == End) && IsEndInclusive);
                }

                if (position == End) {
                    return IsEndInclusive;
                }

                return true;
            }
            return false;
        }
        #endregion

        public override string ToString() => Invariant($"[{Start}...{End}]");

        /// <summary>
        /// Determines if ranges are equal. Ranges are equal when they are either both null
        /// or both are not null and their coordinates are equal.
        /// </summary>
        /// <param name="left">First range</param>
        /// <param name="right">Second range</param>
        /// <returns>True if ranges are equal</returns>
        public static bool AreEqual(ITextRange left, ITextRange right) {
            if (object.ReferenceEquals(left, right)) {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)left == null) || ((object)right == null)) {
                return false;
            }

            return (left.Start == right.Start) && (left.End == right.End);
        }

        /// <summary>
        /// Determines if range contains given position
        /// </summary>
        /// <param name="range">Text range</param>
        /// <param name="position">Position</param>
        /// <returns>True if position is inside the range</returns>
        public static bool Contains(ITextRange range, int position) => Contains(range.Start, range.Length, position);

        /// <summary>
        /// Determines if range contains another range
        /// </summary>
        public static bool Contains(ITextRange range, ITextRange other) => range.Contains(other.Start) && range.Contains(other.End);

        /// <summary>
        /// Determines if range contains another range
        /// </summary>
        public static bool Contains(ITextRange range, ITextRange other, bool inclusiveEnd) {
            if (inclusiveEnd) {
                return ContainsInclusiveEnd(range, other);
            }
            return range.Contains(other.Start) && range.Contains(other.End);
        }

        /// <summary>
        /// Determines if range contains another range or it contains start point
        /// of the other range and their end points are the same.
        /// </summary>
        public static bool ContainsInclusiveEnd(ITextRange range, ITextRange other) => range.Contains(other.Start) && (range.Contains(other.End) || range.End == other.End);

        /// <summary>
        /// Determines if range contains given position
        /// </summary>
        /// <param name="rangeStart">Start of the text range</param>
        /// <param name="rangeLength">Length of the text range</param>
        /// <param name="position">Position</param>
        /// <returns>Tru if position is inside the range</returns>
        public static bool Contains(int rangeStart, int rangeLength, int position) {
            if (rangeLength == 0 && position == rangeStart) {
                return true;
            }

            return position >= rangeStart && position < rangeStart + rangeLength;
        }

        /// <summary>
        /// Finds out if range intersects another range
        /// </summary>
        /// <param name="range1">First text range</param>
        /// <param name="range2">Second text range</param>
        /// <returns>True if ranges intersect</returns>
        public static bool Intersect(ITextRange range1, ITextRange range2) => Intersect(range1, range2.Start, range2.Length);

        /// <summary>
        /// Finds out if range intersects another range
        /// </summary>
        /// <param name="range">First text range</param>
        /// <param name="rangeStart2">Start of the second range</param>
        /// <param name="rangeLength2">Length of the second range</param>
        /// <returns>True if ranges intersect</returns>
        public static bool Intersect(ITextRange range1, int rangeStart2, int rangeLength2) => Intersect(range1.Start, range1.Length, rangeStart2, rangeLength2);

        /// <summary>
        /// Finds out if range intersects another range
        /// </summary>
        /// <param name="rangeStart1">Start of the first range</param>
        /// <param name="rangeLength1">Length of the first range</param>
        /// <param name="rangeStart2">Start of the second range</param>
        /// <param name="rangeLength2">Length of the second range</param>
        /// <returns>True if ranges intersect</returns>
        public static bool Intersect(int rangeStart1, int rangeLength1, int rangeStart2, int rangeLength2) {
            // !(rangeEnd2 <= rangeStart1 || rangeStart2 >= rangeEnd1)

            // Support intersection with empty ranges

            if (rangeLength1 == 0 && rangeLength2 == 0) {
                return rangeStart1 == rangeStart2;
            }

            if (rangeLength1 == 0) {
                return Contains(rangeStart2, rangeLength2, rangeStart1);
            }

            if (rangeLength2 == 0) {
                return Contains(rangeStart1, rangeLength1, rangeStart2);
            }

            return rangeStart2 + rangeLength2 > rangeStart1 && rangeStart2 < rangeStart1 + rangeLength1;
        }

        /// <summary>
        /// Finds out if range represents valid text range (when range is not null and it's length is greater than zero)
        /// </summary>
        /// <returns>True if range is valid</returns>
        public static bool IsValid(ITextRange range) => range != null && range.Length > 0;

        /// <summary>
        /// Calculates range that includes both supplied ranges.
        /// </summary>
        public static ITextRange Union(ITextRange range1, ITextRange range2) {
            int start = Math.Min(range1.Start, range2.Start);
            int end = Math.Max(range1.End, range2.End);

            return start <= end ? TextRange.FromBounds(start, end) : TextRange.EmptyRange;
        }

        /// <summary>
        /// Calculates range that includes both supplied ranges.
        /// </summary>
        public static ITextRange Union(ITextRange range1, int rangeStart, int rangeLength) {
            int start = Math.Min(range1.Start, rangeStart);
            int end = Math.Max(range1.End, rangeStart + rangeLength);

            return start <= end ? TextRange.FromBounds(start, end) : TextRange.EmptyRange;
        }

        /// <summary>
        /// Calculates range that is an intersection of the supplied ranges.
        /// </summary>
        /// <returns>Intersection or empty range if ranges don't intersect</returns>
        public static ITextRange Intersection(ITextRange range1, ITextRange range2) {
            int start = Math.Max(range1.Start, range2.Start);
            int end = Math.Min(range1.End, range2.End);

            return start <= end ? TextRange.FromBounds(start, end) : TextRange.EmptyRange;
        }

        /// <summary>
        /// Calculates range that is an intersection of the supplied ranges.
        /// </summary>
        /// <returns>Intersection or empty range if ranges don't intersect</returns>
        public static ITextRange Intersection(ITextRange range1, int rangeStart, int rangeLength) {
            int start = Math.Max(range1.Start, rangeStart);
            int end = Math.Min(range1.End, rangeStart + rangeLength);

            return start <= end ? TextRange.FromBounds(start, end) : TextRange.EmptyRange;
        }

        /// <summary>
        /// Creates copy of the text range object via memberwise cloning
        /// </summary>
        public TextRange Clone() => (TextRange)MemberwiseClone();
    }
}
