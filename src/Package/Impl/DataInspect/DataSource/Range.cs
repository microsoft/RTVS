// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Range of integers
    /// </summary>
    [DebuggerDisplay("[{Start},{_end})")]
    public struct Range {
        long _end;

        public Range(long start, long count) {
            Start = start;
            Count = count;
            _end = start + count;
        }

        public long Start { get; }

        public long Count { get; }

        public bool Contains(long value) {
            return (value >= Start) && (value < _end);
        }

        public bool Contains(Range other) {
            if (Count == 0) {
                return false;
            }

            return (other.Start <= this.Start) && (other._end >= this._end);
        }

        public IEnumerable<long> GetEnumerable(bool ascending = true) {
            if (ascending) {
                for (long i = Start; i < _end; i++) {
                    yield return i;
                }
            } else {
                for (long i = _end - 1; i >= Start; i--) {
                    yield return i;
                }
            }
        }

        public string ToRString() {
            return Invariant($"{Start + 1}:{Start + Count}");
        }
    }
}
