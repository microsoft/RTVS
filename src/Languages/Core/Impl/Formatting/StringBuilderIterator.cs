// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Formatting {
    public sealed class StringBuilderIterator : ITextIterator {
        private readonly StringBuilder _sb;

        public StringBuilderIterator(StringBuilder sb) {
            _sb = sb;
        }

        public char this[int position] => position >= 0 && position < _sb.Length ? _sb[position] : '\0';
        public int Length => _sb.Length;
    }
}
