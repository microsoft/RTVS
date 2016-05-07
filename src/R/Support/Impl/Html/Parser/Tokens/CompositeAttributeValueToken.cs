// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser.Tokens {
    public sealed class CompositeAttributeValueToken : CompositeToken, IHtmlAttributeValueToken {
        public CompositeAttributeValueToken(IHtmlToken[] tokens, char openQuote, char closeQuote, bool isScript)
            : base(tokens) {
            Debug.Assert(tokens.Length > 1, "Use AttributeValueToken for token count less than 2");

            IsScript = isScript;

            OpenQuote = openQuote;
            CloseQuote = closeQuote;
        }

        #region IHtmlAttributeValueToken

        public char OpenQuote { get; private set; }
        public char CloseQuote { get; private set; }

        public bool IsScript { get; private set; }

        public ReadOnlyTextRangeCollection<IHtmlToken> Tokens {
            get {
                return new ReadOnlyTextRangeCollection<IHtmlToken>(this);
            }
        }
        #endregion

        #region IHtmlToken
        public override bool IsWellFormed {
            get { return true; }
        }
        #endregion
    }
}
