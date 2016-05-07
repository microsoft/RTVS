// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Html.Core.Parser.Utility;

namespace Microsoft.Html.Core.Test.Utility {
    [ExcludeFromCodeCoverage]
    internal static class ParserAccessor {
        public static HtmlParser CreateParser(string text) {
            var target = new HtmlParser();
            target._cs = new HtmlCharStream(text);
            target._tokenizer = new HtmlTokenizer(target._cs);
            return target;
        }

    }
}
