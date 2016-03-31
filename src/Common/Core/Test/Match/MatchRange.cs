// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using static System.FormattableString;

namespace Microsoft.Common.Core.Test.Match {
    public class MatchRange<T> : IEquatable<T> where T : IComparable<T> {
        private readonly T _from, _to;

        public MatchRange(T from, T to) {
            _from = from;
            _to = to;
        }

        public bool Equals(T other) =>
            other == null ? false : other.CompareTo(_from) >= 0 && other.CompareTo(_to) <= 0;

        public override bool Equals(object other) =>
            other is T ? Equals((T)other) : false;

        public override int GetHashCode() =>
            new { _from, _to }.GetHashCode();

        public override string ToString() =>
            Invariant($"[{_from} .. {_to}]");
    }
}
