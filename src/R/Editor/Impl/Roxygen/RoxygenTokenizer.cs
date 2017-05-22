// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Editor.Roxygen {
    public sealed class RoxygenTokenizer : BaseTokenizer<RToken> {
        public override void AddNextToken() {
            if (_cs.CurrentChar == '#' && _cs.NextChar == '\'') {
                _cs.Advance(2);
                SkipWhitespace();

                if (_cs.CurrentChar == '@') {
                    var start = _cs.Position;
                    AddWord(RTokenType.Keyword);
                    TryAddExport(start, _cs.Position - start);
                }
            } else {
                _cs.Position = _cs.Length;
            }
        }

        private void TryAddExport(int start, int length) {
            var text = _cs.GetSubstringAt(start, length);
            if (text.EqualsOrdinal("@export")) {
                AddWord(RTokenType.Identifier);
            }
        }

        private void AddWord(RTokenType type) {
            SkipWhitespace();
            var start = _cs.Position;
            SkipWord();
            if (_cs.Position > start) {
                AddToken(type, start, _cs.Position - start);
            }
        }

        private void SkipWord() => 
            Tokenizer.SkipIdentifier(
                _cs,
                (CharacterStream cs) => !cs.IsWhiteSpace(),
                (CharacterStream cs) => !cs.IsWhiteSpace());

        private void AddToken(RTokenType type, int start, int length)
            => _tokens.Add(new RToken(type, start, length));
    }
}
