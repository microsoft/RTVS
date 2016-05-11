// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser {
    public sealed partial class HtmlParser {
        internal void OnScriptState(string scriptType, NameToken nameToken) {
            string scriptQualifiedName = _cs.GetSubstringAt(nameToken.QualifiedName.Start, nameToken.QualifiedName.Length);
            string endScript = string.Concat("</", scriptQualifiedName);

            ITextRange range = FindEndOfBlock(endScript, simpleSearch: true);
            ScriptBlockFound?.Invoke(this, new HtmlParserBlockRangeEventArgs(range, scriptType));
        }

        internal ITextRange FindEndOfBlock(string endTag, bool simpleSearch = false) {
            bool ignoreCase = ParsingMode == ParsingMode.Html;
            int start = _cs.Position;

            if (simpleSearch) {
                int pos = _cs.Text.IndexOf(endTag, start, ignoreCase);
                _cs.Position = pos >= 0 ? pos : _cs.Position + _cs.DistanceFromEnd;
            } else {
                // Plain text search for script or style end tags
                // with artifact processing
                while (!_cs.IsEndOfStream()) {
                    if (_cs.CurrentChar == '<' && _cs.NextChar == '/') {
                        if (_cs.CompareTo(_cs.Position, endTag.Length, endTag, true)) {
                            break;
                        }
                    }
                    _cs.MoveToNextChar();
                }
            }
            return TextRange.FromBounds(start, _cs.Position);
        }
    }
}
