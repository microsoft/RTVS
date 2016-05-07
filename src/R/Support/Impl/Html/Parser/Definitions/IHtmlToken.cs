// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Languages.Core.Tokens;

namespace Microsoft.Html.Core.Parser {
    public interface IHtmlToken : IToken<HtmlTokenType>
    {
        /// <summary>
        /// True if token is properly terminated. False, if, for example,
        /// HTML comment is not closed and final --> is missing.
        /// </summary>
        bool IsWellFormed { get; }
    }
}
