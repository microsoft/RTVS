// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Html.Core.Parser.Tokens;

namespace Microsoft.Html.Core.Parser {
    public sealed partial class HtmlParser {
        internal void OnCommentState() {
            Debug.Assert(_cs.CurrentChar == '<' && _cs.NextChar == '!' && _cs.LookAhead(2) == '-' && _cs.LookAhead(3) == '-');

            var tokens = _tokenizer.GetComment();
            if (CommentFound != null)
                CommentFound(this, new HtmlParserCommentEventArgs(new CommentToken(tokens)));
        }
    }
}
