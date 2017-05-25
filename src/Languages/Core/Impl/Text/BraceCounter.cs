// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Languages.Core.Text {
    public class BraceCounter<T> where T : IComparable<T> {
        private class BracePair {
            public readonly T OpenBrace;
            public readonly T CloseBrace;

            public BracePair(T openBrace, T closeBrace) {
                OpenBrace = openBrace;
                CloseBrace = closeBrace;
            }
        }

        private readonly List<BracePair> _bracePairs;
        private readonly Stack<T>[] _bracesStacks;

        public BraceCounter(T openCurlyBrace, T closeCurlyBrace) :
            this(new List<T> { openCurlyBrace, closeCurlyBrace }) {
        }

        public BraceCounter(IEnumerable<T> braces) {
            var array = braces.ToArray();
            if ((array.Length & 1) > 0 || array.Length == 0) {
                throw new ArgumentException("Brace count must be even and greater than zero");
            }

            var pairCount = array.Length / 2;
            _bracesStacks = new Stack<T>[pairCount];

            _bracePairs = new List<BracePair>();
            for (var i = 0; i < array.Length; i += 2) {
                var pair = new BracePair(array[i], array[i + 1]);
                _bracePairs.Add(pair);
                _bracesStacks[i / 2] = new Stack<T>();
            }
        }

        public int Count => _bracesStacks.Sum(s => s.Count);

        public bool CountBrace(T brace) {
            for (var i = 0; i < _bracePairs.Count; i++) {
                var pair = _bracePairs[i];
                if (0 == pair.OpenBrace.CompareTo(brace)) {
                    _bracesStacks[i].Push(brace);
                    return true;
                }

                if (0 == pair.CloseBrace.CompareTo(brace)) {
                    if (_bracesStacks[i].Count > 0) {
                        _bracesStacks[i].Pop();
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
