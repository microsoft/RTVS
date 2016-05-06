// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Html.Core.Parser.Tokens {
    public sealed class ScriptAttributeValueToken : ComplexAttributeValueToken {
        public ScriptAttributeValueToken(IHtmlToken token, char openQuote, char closeQuote) :
            base(token, openQuote, closeQuote) {
        }

        public override bool IsScript => true;
    }
}
