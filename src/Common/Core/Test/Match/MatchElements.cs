// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Microsoft.Common.Core.Test.Match {
    [ExcludeFromCodeCoverage]
    public class MatchElements<T> : IEquatable<IEnumerable<T>> {
        private readonly bool _exactOrder;
        private readonly List<IEquatable<T>> _expected = new List<IEquatable<T>>();

        public MatchElements(bool exactOrder) {
            _exactOrder = exactOrder;
        }

        public MatchElements(bool exactOrder, IEnumerable<IEquatable<T>> elements)
            : this(exactOrder) {
            _expected.AddRange(elements);
        }

        public void Add(IEquatable<T> element) {
            _expected.Add(element);
        }

        private bool Matches(IEquatable<T> expected, T actual) =>
            expected == null ? actual == null : expected.Equals(actual);

        public bool Equals(IEnumerable<T> other) {
            if (other == null) {
                return false;
            }

            var actual = other.ToList();
            if (actual.Count != _expected.Count) {
                return false;
            }

            if (_exactOrder) {
                return _expected.Zip(actual, Matches).All(b => b);
            } else {
                foreach (var e in _expected) {
                    int i = actual.FindIndex(a => Matches(e, a));
                    if (i < 0) {
                        return false;
                    }
                    actual.RemoveAt(i);
                }
                return actual.Count == 0;
            }
        }

        public override bool Equals(object obj) =>
            Equals(obj as IEnumerable<T>);

        public override int GetHashCode() => 0;
    }
}
