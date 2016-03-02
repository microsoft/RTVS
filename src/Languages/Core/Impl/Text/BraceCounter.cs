// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Languages.Core.Text {
    public class BraceCounter<T> where T : IComparable<T> {
        class BracePair {
            public T OpenBrace;
            public T CloseBrace;

            public BracePair(T openBrace, T closeBrace) {
                OpenBrace = openBrace;
                CloseBrace = closeBrace;
            }
        }

        private List<BracePair> _bracePairs;
        private Stack<T>[] _bracesStacks;

        public BraceCounter(T openCurlyBrace, T closeCurlyBrace) :
            this(new List<T>() { openCurlyBrace, closeCurlyBrace }) {
        }

        public BraceCounter(IEnumerable<T> braces) {
            T[] array = braces.ToArray();
            if ((array.Length & 1) > 0 || array.Length == 0) {
                throw new ArgumentException("Brace count must be even and greater than zero");
            }

            int pairCount = array.Length / 2;
            _bracesStacks = new Stack<T>[pairCount];

            _bracePairs = new List<BracePair>();
            for (int i = 0; i < array.Length; i += 2) {
                var pair = new BracePair(array[i], array[i + 1]);
                _bracePairs.Add(pair);
                _bracesStacks[i / 2] = new Stack<T>();
            }
        }

        public int Count {
            get {
                int c = 0;
                foreach (var s in _bracesStacks) {
                    c += s.Count;
                }
                return c;
            }
        }

        public bool CountBrace(T brace) {
            for (int i = 0; i < _bracePairs.Count; i++) {
                BracePair pair = _bracePairs[i];
                if (0 == pair.OpenBrace.CompareTo(brace)) {
                    _bracesStacks[i].Push(brace);
                    return true;
                } else if (0 == pair.CloseBrace.CompareTo(brace)) {
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
