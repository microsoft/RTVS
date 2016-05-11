// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser.Tokens {
    /// <summary>
    /// Composite token is a collection of tokens and is typically
    /// produced if attribute value or comment contains artifacts. For example
    /// &lt;!-- &lt;% %> --> or attribute="&lt;% %>&lt;% %>"; Composite token
    /// also represents HTML comment and consists of opening &lt;!--, inner text
    /// and closing -->
    /// </summary>
    public abstract class CompositeToken : TextRangeCollection<IHtmlToken>, IHtmlToken {
        protected CompositeToken()
            : this(new IHtmlToken[0]) {
        }

        protected CompositeToken(IHtmlToken[] tokens)
            : base(tokens) {
        }

        public override bool Contains(int position) {
            if (Count > 0 && !this[Count - 1].IsWellFormed && position == this[Count - 1].End)
                return true;

            return base.Contains(position);
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            foreach (var t in this) {
                sb.Append(t.ToString());
            }

            return sb.ToString();
        }

        #region IToken<HtmlTokenType> Members

        public HtmlTokenType TokenType {
            get { return HtmlTokenType.Composite; }
        }

        #endregion

        #region IHtmlToken
        public abstract bool IsWellFormed { get; }
        #endregion
    }
}
