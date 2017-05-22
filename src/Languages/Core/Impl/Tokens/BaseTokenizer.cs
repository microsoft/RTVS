// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Tokens {
    public abstract class BaseTokenizer<T> : ITokenizer<T> where T : ITextRange {
        protected TextRangeCollection<T> _tokens;
        protected CharacterStream _cs;

        #region ITokenizer
        public IReadOnlyTextRangeCollection<T> Tokenize(string text) 
            => Tokenize(new TextStream(text), 0, text.Length);

        public IReadOnlyTextRangeCollection<T> Tokenize(ITextProvider textProvider, int start, int length) 
            => Tokenize(textProvider, start, length, false);

        public virtual IReadOnlyTextRangeCollection<T> Tokenize(ITextProvider textProvider, int start, int length, bool excludePartialTokens) {
            var end = start + length;

            InitializeTokenizer(textProvider, start, length);

            while (!_cs.IsEndOfStream()) {
                // Keep on adding tokens
                AddNextToken();

                if (_cs.Position >= end) {
                    break;
                }
            }

            if (excludePartialTokens) {
                // Exclude tokens that are beyond the specified range
                int i;
                for (i = _tokens.Count - 1; i >= 0; i--) {
                    if (_tokens[i].End <= end) {
                        break;
                    }
                }

                i++;
                if (i < _tokens.Count) {
                    _tokens.RemoveRange(i, _tokens.Count - i);
                }
            }

            return new ReadOnlyTextRangeCollection<T>(_tokens);
        }
        #endregion

        internal virtual void InitializeTokenizer(ITextProvider textProvider, int start, int length) {
            Debug.Assert(start >= 0 && length >= 0 && start + length <= textProvider.Length);

            _cs = new CharacterStream(textProvider) {Position = start};
            _tokens = new TextRangeCollection<T>();
        }

        public abstract void AddNextToken();

        public void SkipWhitespace() {
            if (_cs.IsEndOfStream()) {
                return;
            }

            while (_cs.IsWhiteSpace()) {
                if (!_cs.MoveToNextChar()) {
                    break;
                }
            }
        }
    }
}
