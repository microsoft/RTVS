// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using static System.FormattableString;

namespace Microsoft.Common.Core.Test.Match {
    [ExcludeFromCodeCoverage]
    public class MatchRange<T> : IEquatable<T> where T : IComparable<T> {
        private readonly IComparable<T> _from, _to;

        public MatchRange(IComparable<T> from, IComparable<T> to) {
            _from = from;
            _to = to;
        }

        public bool Equals(T other) =>
            _from.CompareTo(other) <= 0 && _to.CompareTo(other) >= 0;

        public override bool Equals(object other) =>
            other is T ? Equals((T)other) : false;

        public override int GetHashCode() =>
            new { _from, _to }.GetHashCode();

        public override string ToString() =>
            Invariant($"[{_from} .. {_to}]");
    }
}
