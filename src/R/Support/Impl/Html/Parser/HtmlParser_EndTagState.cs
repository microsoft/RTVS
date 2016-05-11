// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser {
    public sealed partial class HtmlParser {
        internal void OnEndTagState() {
            Debug.Assert(_cs.CurrentChar == '<' && _cs.NextChar == '/');

            int start = _cs.Position;
            _cs.Advance(2);
            // element name may be missing

            if (_cs.CurrentChar == '<')
                return;

            NameToken nameToken = _tokenizer.GetNameToken();
            if (nameToken == null || nameToken.Length == 0)
                return;

            if (EndTagOpen != null)
                EndTagOpen(this, new HtmlParserOpenTagEventArgs(start, nameToken));

            while (!_cs.IsEndOfStream()) {
                _tokenizer.SkipWhitespace();

                if (_cs.CurrentChar == '>') {
                    var range = new TextRange(_cs.Position);

                    if (EndTagClose != null)
                        EndTagClose(this, new HtmlParserCloseTagEventArgs(range, true, false));

                    _cs.MoveToNextChar();
                    return;
                }

                if (_cs.CurrentChar == '<') {
                    // Untermimated end tag (sequence may be '</<', '</{ws}>', '</>', '</name <'
                    // '</name attrib="value" <' and so on. Close tag at the previous position.

                    if (EndTagClose != null)
                        EndTagClose(this, new HtmlParserCloseTagEventArgs(new TextRange(_cs.Position - 1), false, false));

                    return;
                }

                // attributes are not allowed in end tags so skip over any extra characters until > or <
                _cs.MoveToNextChar();
            }
        }
    }
}
